using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SMS.BLL.MultiTenancy;
using SMS.DAL.Platform;
using SMS.Entities.Platform;

namespace SMS.Tests.MultiTenancy
{
    /// <summary>In-memory platform registry with two schools on two different database servers.</summary>
    public sealed class FakePlatformRepository : IPlatformRepository
    {
        public List<Tenant> Tenants { get; } = new();
        public List<TenantDomain> Domains { get; } = new();
        public List<DatabaseServer> Servers { get; } = new();
        public List<TenantDatabaseConnection> Connections { get; } = new();
        public Dictionary<string, PlatformSecret> Secrets { get; } = new();
        public List<PlatformAuditLog> Audit { get; } = new();
        public int RouteLookups;

        public static (FakePlatformRepository repo, ISecretProtector protector) TwoSchools()
        {
            var protector = new AesGcmSecretProtector(RandomNumberGenerator.GetBytes(32));
            var repo = new FakePlatformRepository();
            repo.Servers.Add(new DatabaseServer { Id = 1, ServerName = "DB-SERVER-01", Provider = DatabaseProviderKind.SqlServer, Host = "sql1.internal", Port = 1433, IsActive = true });
            repo.Servers.Add(new DatabaseServer { Id = 2, ServerName = "DB-SERVER-02", Provider = DatabaseProviderKind.PostgreSQL, Host = "pg2.internal", Port = 5432, IsActive = true });
            repo.AddSchool(1, "abc", "ABC School", 1, "abcDB", "abc.schoolbd.net", protector);
            repo.AddSchool(2, "xyz", "XYZ College", 2, "xyzDB", "xyz.schoolbd.net", protector);
            return (repo, protector);
        }

        public void AddSchool(int id, string code, string name, int serverId, string db, string domain, ISecretProtector protector)
        {
            Tenants.Add(new Tenant { Id = id, TenantCode = code, TenantName = name, DisplayName = name, Status = TenantStatus.Active });
            Domains.Add(new TenantDomain { Id = id, TenantId = id, Domain = domain, IsActive = true, IsVerified = true, IsPrimary = true });
            Connections.Add(new TenantDatabaseConnection
            {
                Id = id, TenantId = id, DatabaseServerId = serverId, DatabaseName = db, IsPrimary = true,
                Status = TenantDatabaseStatus.Active, SecretReference = $"platform:tenant-{code}-db",
            });
            Secrets[$"tenant-{code}-db"] = new PlatformSecret
            {
                Name = $"tenant-{code}-db",
                Ciphertext = protector.Protect(CredentialsJson.Serialize(new DatabaseCredentials($"{code}_app", $"{code}-secret"))),
            };
        }

        private TenantRoute Route(TenantDomain domain, Tenant tenant)
        {
            var c = tenant == null ? null : Connections.FirstOrDefault(x => x.TenantId == tenant.Id && x.IsPrimary);
            var s = c == null ? null : Servers.First(x => x.Id == c.DatabaseServerId);
            return new TenantRoute(domain, tenant, c, s);
        }

        public Task<TenantRoute> FindRouteByDomainAsync(string normalizedDomain, CancellationToken ct = default)
        {
            Interlocked.Increment(ref RouteLookups);
            var d = Domains.FirstOrDefault(x => x.Domain == normalizedDomain);
            return Task.FromResult(d == null ? null : Route(d, Tenants.FirstOrDefault(t => t.Id == d.TenantId && !t.IsDeleted)));
        }

        public Task<TenantRoute> FindRouteByTenantCodeAsync(string tenantCode, CancellationToken ct = default)
        {
            var t = Tenants.FirstOrDefault(x => x.TenantCode == tenantCode && !x.IsDeleted);
            return Task.FromResult(t == null ? null : Route(null, t));
        }

        public Task<PlatformSecret> GetSecretAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(Secrets.TryGetValue(name, out var s) ? s : null);

        public Task<List<Tenant>> GetTenantsAsync(CancellationToken ct = default) => Task.FromResult(Tenants.ToList());
        public Task<List<DatabaseServer>> GetDatabaseServersAsync(CancellationToken ct = default) => Task.FromResult(Servers.ToList());
        public Task<Tenant> GetTenantByCodeAsync(string tenantCode, CancellationToken ct = default) => Task.FromResult(Tenants.FirstOrDefault(t => t.TenantCode == tenantCode));
        public Task<TenantDomain> GetDomainAsync(string normalizedDomain, CancellationToken ct = default) => Task.FromResult(Domains.FirstOrDefault(d => d.Domain == normalizedDomain));
        public Task<DatabaseServer> GetDatabaseServerByNameAsync(string serverName, CancellationToken ct = default) => Task.FromResult(Servers.FirstOrDefault(s => s.ServerName == serverName));

        public Task SaveAsync<T>(T entity, CancellationToken ct = default) where T : class
        {
            switch (entity)
            {
                case TenantDomain d when !Domains.Contains(d):
                    d.Id = Domains.Count + 100;
                    Domains.Add(d);
                    break;
                case Tenant t when !Tenants.Contains(t):
                    t.Id = Tenants.Count + 100;
                    Tenants.Add(t);
                    break;
            }
            return Task.CompletedTask;
        }

        public Task UpsertSecretAsync(string name, string ciphertext, int keyVersion, CancellationToken ct = default)
        {
            Secrets[name] = new PlatformSecret { Name = name, Ciphertext = ciphertext, KeyVersion = keyVersion };
            return Task.CompletedTask;
        }

        public Task AddAuditAsync(PlatformAuditLog entry, CancellationToken ct = default)
        {
            Audit.Add(entry);
            return Task.CompletedTask;
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> CanConnectAsync(CancellationToken ct = default) => Task.FromResult(true);
    }

    public static class TestCatalog
    {
        public static (TenantCatalog catalog, FakePlatformRepository repo) Create(int cacheSeconds = 60)
        {
            var (repo, protector) = FakePlatformRepository.TwoSchools();
            var options = new MultiTenancyOptions { Enabled = true, ResolutionCacheSeconds = cacheSeconds, NegativeCacheSeconds = cacheSeconds };
            return (new TenantCatalog(repo, new SecretResolver(repo, protector), new TenantResolutionCache(), options), repo);
        }

        public static TenantInfo Tenant(int id, string code, DatabaseProviderKind provider = DatabaseProviderKind.SqlServer, string cs = "Server=s;Database=d") =>
            new(id, code, code.ToUpperInvariant(), TenantStatus.Active, provider, cs, id, code + "DB");
    }
}
