using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;
using SMS.BLL.MultiTenancy;
using SMS.Entities.Platform;
using Xunit;

namespace SMS.Tests.MultiTenancy
{
    public class HostNormalizationTests
    {
        [Theory]
        [InlineData("abc.schoolbd.net", "abc.schoolbd.net")]
        [InlineData("ABC.SchoolBD.Net", "abc.schoolbd.net")]
        [InlineData("abc.schoolbd.net.", "abc.schoolbd.net")]
        [InlineData("abc.schoolbd.net:443", "abc.schoolbd.net")]
        [InlineData("  abc.schoolbd.net  ", "abc.schoolbd.net")]
        [InlineData("abcschool.edu.bd", "abcschool.edu.bd")]
        [InlineData("Bücher.de", "xn--bcher-kva.de")]
        public void Normalizes_valid_hosts(string input, string expected) =>
            Assert.Equal(expected, TenantNames.NormalizeHost(input));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        [InlineData("[::1]:5001")]
        [InlineData("abc.schoolbd.net:abc")]
        [InlineData("abc.schoolbd.net@xyz.schoolbd.net")]
        [InlineData("abc.schoolbd.net/xyz")]
        [InlineData("abc_school.schoolbd.net")]
        [InlineData("-abc.schoolbd.net")]
        [InlineData("abc..schoolbd.net")]
        public void Rejects_anything_that_is_not_a_plain_domain(string input) =>
            Assert.Null(TenantNames.NormalizeHost(input));

        [Theory]
        [InlineData("abc.schoolbd.net", true)]
        [InlineData("a.b.schoolbd.net", false)]
        [InlineData("schoolbd.net", false)]
        [InlineData("evilschoolbd.net", false)]
        [InlineData("abc.schoolbd.net.evil.com", false)]
        public void Platform_subdomain_detection(string domain, bool expected) =>
            Assert.Equal(expected, TenantNames.IsPlatformSubdomain(domain, "schoolbd.net"));

        [Theory]
        [InlineData("abc", true)]
        [InlineData("green-school", true)]
        [InlineData("ABC", false)]
        [InlineData("a_b", false)]
        [InlineData("-abc", false)]
        [InlineData("../abc", false)]
        [InlineData("", false)]
        public void Tenant_code_rules(string code, bool expected) =>
            Assert.Equal(expected, TenantNames.IsValidTenantCode(code));
    }

    public class TenantCatalogTests
    {
        [Fact]
        public async Task Each_domain_resolves_to_its_own_school_and_database_server()
        {
            var (catalog, _) = TestCatalog.Create();

            var abc = await catalog.ResolveHostAsync("abc.schoolbd.net");
            var xyz = await catalog.ResolveHostAsync("xyz.schoolbd.net");

            Assert.Equal(TenantResolutionStatus.Resolved, abc.Status);
            Assert.Equal("abc", abc.Tenant.TenantCode);
            Assert.Equal(DatabaseProviderKind.SqlServer, abc.Tenant.Provider);
            Assert.Equal("abcDB", new SqlConnectionStringBuilder(abc.Tenant.ConnectionString).InitialCatalog);
            Assert.Equal("sql1.internal,1433", new SqlConnectionStringBuilder(abc.Tenant.ConnectionString).DataSource);

            Assert.Equal("xyz", xyz.Tenant.TenantCode);
            Assert.Equal(DatabaseProviderKind.PostgreSQL, xyz.Tenant.Provider);
            var pg = new NpgsqlConnectionStringBuilder(xyz.Tenant.ConnectionString);
            Assert.Equal("xyzDB", pg.Database);
            Assert.Equal("pg2.internal", pg.Host);
            Assert.Equal("xyz_app", pg.Username);
        }

        [Fact]
        public async Task Unknown_domain_is_rejected()
        {
            var (catalog, _) = TestCatalog.Create();
            Assert.Equal(TenantResolutionStatus.UnknownHost, (await catalog.ResolveHostAsync("evil.schoolbd.net")).Status);
            Assert.Equal(TenantResolutionStatus.UnknownHost, (await catalog.ResolveHostAsync("localhost")).Status);
        }

        [Fact]
        public async Task Unverified_custom_domain_does_not_resolve()
        {
            var (catalog, repo) = TestCatalog.Create();
            repo.Domains.Add(new TenantDomain { Id = 9, TenantId = 1, Domain = "abcschool.edu.bd", DomainType = TenantDomainType.CustomDomain, IsVerified = false, IsActive = false });

            var result = await catalog.ResolveHostAsync("abcschool.edu.bd");

            Assert.Equal(TenantResolutionStatus.DomainNotActive, result.Status);
            Assert.Null(result.Tenant);
        }

        [Fact]
        public async Task Verified_custom_domain_resolves_to_the_same_school()
        {
            var (catalog, repo) = TestCatalog.Create();
            repo.Domains.Add(new TenantDomain { Id = 9, TenantId = 1, Domain = "abcschool.edu.bd", DomainType = TenantDomainType.CustomDomain, IsVerified = true, IsActive = true });

            var result = await catalog.ResolveHostAsync("AbcSchool.edu.bd");

            Assert.Equal("abc", result.Tenant.TenantCode);
        }

        [Fact]
        public async Task Suspended_school_gets_no_connection_string()
        {
            var (catalog, repo) = TestCatalog.Create();
            repo.Tenants[1].Status = TenantStatus.Suspended;

            var result = await catalog.ResolveHostAsync("xyz.schoolbd.net");

            Assert.Equal(TenantResolutionStatus.TenantNotActive, result.Status);
            Assert.Equal(TenantStatus.Suspended, result.Tenant.Status);
            Assert.Null(result.Tenant.ConnectionString);
        }

        [Fact]
        public async Task Database_not_active_or_missing_secret_is_not_served()
        {
            var (catalog, repo) = TestCatalog.Create();
            repo.Connections[0].Status = TenantDatabaseStatus.Failed;
            repo.Secrets.Remove("tenant-xyz-db");

            Assert.Equal(TenantResolutionStatus.DatabaseNotConfigured, (await catalog.ResolveHostAsync("abc.schoolbd.net")).Status);
            Assert.Equal(TenantResolutionStatus.DatabaseNotConfigured, (await catalog.ResolveHostAsync("xyz.schoolbd.net")).Status);
        }

        [Fact]
        public async Task Lookups_are_cached_and_invalidation_drops_them()
        {
            var (catalog, repo) = TestCatalog.Create();
            for (int i = 0; i < 10; i++)
            {
                await catalog.ResolveHostAsync("abc.schoolbd.net");
            }
            Assert.Equal(1, repo.RouteLookups);

            repo.Tenants[0].Status = TenantStatus.Suspended;
            catalog.Invalidate();
            Assert.Equal(TenantResolutionStatus.TenantNotActive, (await catalog.ResolveHostAsync("abc.schoolbd.net")).Status);
            Assert.Equal(2, repo.RouteLookups);
        }

        [Fact]
        public async Task Concurrent_resolution_for_many_schools_never_mixes_them()
        {
            var (catalog, _) = TestCatalog.Create(cacheSeconds: 1);
            var tasks = new Task[400];
            for (int i = 0; i < tasks.Length; i++)
            {
                var host = i % 2 == 0 ? "abc.schoolbd.net" : "xyz.schoolbd.net";
                var expected = i % 2 == 0 ? "abcDB" : "xyzDB";
                tasks[i] = Task.Run(async () =>
                {
                    var r = await catalog.ResolveHostAsync(host);
                    Assert.Equal(expected, r.Tenant.DatabaseName);
                });
            }
            await Task.WhenAll(tasks);
        }
    }

    public class SecretAndConnectionStringTests
    {
        [Fact]
        public void Secrets_round_trip_and_are_not_plaintext()
        {
            var protector = new AesGcmSecretProtector(RandomNumberGenerator.GetBytes(32));
            var a = protector.Protect("P@ss;word");
            var b = protector.Protect("P@ss;word");

            Assert.DoesNotContain("P@ss", a);
            Assert.NotEqual(a, b); // random nonce
            Assert.Equal("P@ss;word", protector.Unprotect(a));
        }

        [Fact]
        public void Tampered_ciphertext_or_wrong_key_fails()
        {
            var protector = new AesGcmSecretProtector(RandomNumberGenerator.GetBytes(32));
            var other = new AesGcmSecretProtector(RandomNumberGenerator.GetBytes(32));
            var data = Convert.FromBase64String(protector.Protect("secret"));
            data[^1] ^= 1;

            Assert.ThrowsAny<CryptographicException>(() => protector.Unprotect(Convert.ToBase64String(data)));
            Assert.ThrowsAny<CryptographicException>(() => other.Unprotect(protector.Protect("secret")));
        }

        [Fact]
        public void Missing_or_short_key_fails_fast()
        {
            Assert.Throws<InvalidOperationException>(() => AesGcmSecretProtector.FromBase64(null));
            Assert.Throws<ArgumentException>(() => AesGcmSecretProtector.FromBase64(Convert.ToBase64String(new byte[16])));
        }

        [Fact]
        public void Credentials_cannot_inject_connection_keywords()
        {
            var server = new DatabaseServer { Provider = DatabaseProviderKind.SqlServer, Host = "sql1", ConnectionOptions = "TrustServerCertificate=True" };
            var cs = TenantConnectionStrings.Build(server, "abcDB", new DatabaseCredentials("abc_app", "x;Database=xyzDB;User Id=sa"));
            var parsed = new SqlConnectionStringBuilder(cs);

            Assert.Equal("abcDB", parsed.InitialCatalog);
            Assert.Equal("abc_app", parsed.UserID);
            Assert.Equal("x;Database=xyzDB;User Id=sa", parsed.Password);
            Assert.True(parsed.TrustServerCertificate);
        }
    }
}
