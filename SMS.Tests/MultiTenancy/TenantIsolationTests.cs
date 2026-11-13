using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SMS.BLL.MultiTenancy;
using SMS.DB;
using SMS.Entities.Platform;
using SMS_App.Configurations;
using SMS_App.Hubs;
using SMS_App.MultiTenancy;
using SMS_App.Utilities.ShortMessageService;
using Xunit;

namespace SMS.Tests.MultiTenancy
{
    public class TenantResolutionMiddlewareTests
    {
        private static async Task<(HttpContext ctx, TenantInfo bound, bool nextCalled)> SendAsync(
            string host, Action<HttpRequest> tweak = null, FakePlatformRepository repo = null)
        {
            var (catalog, defaultRepo) = TestCatalog.Create();
            if (repo != null)
            {
                catalog = new TenantCatalog(repo, new SecretResolver(repo, new AesGcmSecretProtector(new byte[32])), new TenantResolutionCache(), new MultiTenancyOptions());
            }
            var services = new ServiceCollection();
            services.AddSingleton<ITenantCatalog>(catalog);
            var provider = services.BuildServiceProvider();

            var ctx = new DefaultHttpContext { RequestServices = provider };
            ctx.Request.Host = new HostString(host);
            ctx.Request.Path = "/API/Students";
            ctx.Response.Body = new MemoryStream();
            tweak?.Invoke(ctx.Request);

            var accessor = new HttpContextAccessor { HttpContext = ctx };
            var mode = new TenancyMode(true, null);
            var tenantContext = new TenantContext(accessor, mode);
            TenantInfo bound = null;
            bool nextCalled = false;
            var middleware = new TenantResolutionMiddleware(_ =>
            {
                nextCalled = true;
                bound = tenantContext.Current;
                return Task.CompletedTask;
            }, NullLogger<TenantResolutionMiddleware>.Instance);

            await middleware.InvokeAsync(ctx, mode, tenantContext);
            return (ctx, bound, nextCalled);
        }

        [Fact]
        public async Task Resolves_from_the_host_header()
        {
            var (_, bound, next) = await SendAsync("abc.schoolbd.net");
            Assert.True(next);
            Assert.Equal("abc", bound.TenantCode);
        }

        [Fact]
        public async Task Tenant_headers_query_strings_and_forwarded_host_are_ignored()
        {
            var (_, bound, _) = await SendAsync("abc.schoolbd.net", r =>
            {
                r.Headers["X-Tenant-Id"] = "2";
                r.Headers["X-Tenant"] = "xyz";
                r.Headers["X-Forwarded-Host"] = "xyz.schoolbd.net";
                r.QueryString = new QueryString("?tenant=xyz&tenantId=2&TenantCode=xyz");
            });
            Assert.Equal("abc", bound.TenantCode);
        }

        [Theory]
        [InlineData("unknown.schoolbd.net")]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task Unknown_hosts_never_reach_the_app(string host)
        {
            var (ctx, _, next) = await SendAsync(host);
            Assert.False(next);
            Assert.Equal(StatusCodes.Status404NotFound, ctx.Response.StatusCode);
        }

        [Fact]
        public async Task Suspended_school_is_refused_with_403()
        {
            var (repo, protector) = FakePlatformRepository.TwoSchools();
            repo.Tenants[1].Status = TenantStatus.Suspended;
            var catalog = new TenantCatalog(repo, new SecretResolver(repo, protector), new TenantResolutionCache(), new MultiTenancyOptions());
            var services = new ServiceCollection().AddSingleton<ITenantCatalog>(catalog).BuildServiceProvider();
            var ctx = new DefaultHttpContext { RequestServices = services };
            ctx.Request.Host = new HostString("xyz.schoolbd.net");
            ctx.Request.Path = "/API/Students";
            ctx.Response.Body = new MemoryStream();
            var mode = new TenancyMode(true, null);
            bool next = false;

            await new TenantResolutionMiddleware(_ => { next = true; return Task.CompletedTask; }, NullLogger<TenantResolutionMiddleware>.Instance)
                .InvokeAsync(ctx, mode, new TenantContext(new HttpContextAccessor { HttpContext = ctx }, mode));

            Assert.False(next);
            Assert.Equal(StatusCodes.Status403Forbidden, ctx.Response.StatusCode);
        }
    }

    public class TenantContextTests
    {
        [Fact]
        public void A_scope_cannot_be_switched_to_another_tenant()
        {
            var ctx = new TenantContext(new HttpContextAccessor(), new TenancyMode(true, null));
            ctx.Set(TestCatalog.Tenant(1, "abc"));
            ctx.Set(TestCatalog.Tenant(1, "abc")); // same tenant again is fine
            Assert.Throws<InvalidOperationException>(() => ctx.Set(TestCatalog.Tenant(2, "xyz")));
            Assert.Equal("abc", ctx.Current.TenantCode);
        }

        [Fact]
        public void Multi_tenant_mode_has_no_tenant_until_resolved_and_single_mode_falls_back()
        {
            Assert.Null(new TenantContext(new HttpContextAccessor(), new TenancyMode(true, null)).Current);
            Assert.Throws<TenantNotResolvedException>(() => new TenantContext(new HttpContextAccessor(), new TenancyMode(true, null)).Required());

            var single = TestCatalog.Tenant(0, TenantInfo.SingleTenantCode);
            Assert.Same(single, new TenantContext(new HttpContextAccessor(), new TenancyMode(false, single)).Current);
        }

        [Fact]
        public void Connection_string_is_never_in_ToString()
        {
            var t = TestCatalog.Tenant(1, "abc", cs: "Server=s;Password=TopSecret");
            Assert.DoesNotContain("TopSecret", t.ToString());
        }
    }

    public class TenantClaimTests
    {
        private static ClaimsPrincipal Principal(string tenantCode) =>
            new(new ClaimsIdentity(tenantCode == null
                ? new[] { new Claim(ClaimTypes.Name, "u") }
                : new[] { new Claim(ClaimTypes.Name, "u"), new Claim(TenantClaims.Type, tenantCode) }, "test"));

        private static ITenantContext Bound(string code)
        {
            var c = new TenantContext(new HttpContextAccessor(), new TenancyMode(true, null));
            c.Set(TestCatalog.Tenant(code == "abc" ? 1 : 2, code));
            return c;
        }

        [Fact]
        public void Login_from_abc_is_rejected_on_xyz()
        {
            Assert.True(TenantClaims.Matches(Principal("abc"), Bound("abc")));
            Assert.False(TenantClaims.Matches(Principal("abc"), Bound("xyz")));
        }

        [Fact]
        public void Login_without_a_school_claim_is_rejected_in_multi_tenant_mode()
        {
            Assert.False(TenantClaims.Matches(Principal(null), Bound("abc")));
        }

        [Fact]
        public void Existing_single_tenant_cookies_keep_working()
        {
            var single = new TenantContext(new HttpContextAccessor(), new TenancyMode(false, TestCatalog.Tenant(0, TenantInfo.SingleTenantCode)));
            Assert.True(TenantClaims.Matches(Principal(null), single));
            Assert.True(TenantClaims.Matches(Principal(TenantInfo.SingleTenantCode), single));
            Assert.False(TenantClaims.Matches(Principal("abc"), single));
        }

        [Fact]
        public void Cache_keys_and_signalr_groups_differ_per_school_for_the_same_user()
        {
            var abc = Bound("abc");
            var xyz = Bound("xyz");
            Assert.NotEqual(PermissionClaimsTransformation.CacheKey(abc, "user-1", "stamp"), PermissionClaimsTransformation.CacheKey(xyz, "user-1", "stamp"));
            Assert.NotEqual(NotificationHub.GroupFor(abc, "user-1"), NotificationHub.GroupFor(xyz, "user-1"));
        }

        [Fact]
        public void Duplicate_sms_gate_is_per_school()
        {
            var number = "0170000" + Guid.NewGuid().ToString("N")[..4];
            Assert.True(SmsSendGate.TryClaim(number, "CheckIn", "t:abc"));
            Assert.False(SmsSendGate.TryClaim(number, "CheckIn", "t:abc"));
            Assert.True(SmsSendGate.TryClaim(number, "CheckIn", "t:xyz"));
        }
    }

    public class TenantDbContextTests
    {
        private static IServiceProvider BuildServices(bool multiTenant)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["MultiTenancy:Enabled"] = multiTenant.ToString(),
                ["MultiTenancy:SecretsKey"] = Convert.ToBase64String(new byte[32]),
                ["MultiTenancy:Platform:Provider"] = "SqlServer",
                ["MultiTenancy:Platform:ConnectionString"] = "Server=platform;Database=SchoolBD_PlatformDB",
            });
            builder.AddSchoolBdTenancy("SqlServer", "Server=single;Database=singleDB");
            return builder.Services.BuildServiceProvider();
        }

        [Fact]
        public async Task Concurrent_scopes_for_different_schools_get_their_own_database()
        {
            var services = BuildServices(multiTenant: true);
            var abc = TestCatalog.Tenant(1, "abc", DatabaseProviderKind.SqlServer, "Server=sql1;Database=abcDB;User Id=abc_app;Password=a");
            var xyz = TestCatalog.Tenant(2, "xyz", DatabaseProviderKind.PostgreSQL, "Host=pg2;Database=xyzDB;Username=xyz_app;Password=x");

            var tasks = Enumerable.Range(0, 200).Select(i => Task.Run(() =>
            {
                var tenant = i % 2 == 0 ? abc : xyz;
                using var scope = services.CreateScope();
                scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenant);
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var connection = db.Database.GetConnectionString();
                Assert.Contains(tenant.DatabaseName, connection);
                Assert.Equal(tenant.Provider == DatabaseProviderKind.PostgreSQL ? "Npgsql.EntityFrameworkCore.PostgreSQL" : "Microsoft.EntityFrameworkCore.SqlServer",
                    db.Database.ProviderName);
                Assert.DoesNotContain(tenant.TenantCode == "abc" ? "xyzDB" : "abcDB", connection);
            })).ToArray();
            await Task.WhenAll(tasks);
        }

        [Fact]
        public void School_database_cannot_be_opened_without_a_resolved_school()
        {
            var services = BuildServices(multiTenant: true);
            using var scope = services.CreateScope();
            Assert.Throws<TenantNotResolvedException>(() => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        }

        [Fact]
        public void Single_tenant_mode_still_uses_DefaultConnection()
        {
            var services = BuildServices(multiTenant: false);
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Contains("singleDB", db.Database.GetConnectionString());
        }
    }
}
