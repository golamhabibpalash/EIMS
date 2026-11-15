using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMS.DB;
using SMS.Entities;
using SMS_App.Configurations;
using SMS_App.ViewModels.ModuleSubModuleVM;
using Xunit;

namespace SMS.Tests.MultiTenancy
{
    /// <summary>A new school database must come out of tenant-init with a visible menu for its admin.</summary>
    public class PermissionSeederTests
    {
        private static SiteMap LoadSiteMap()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SMS_App", "SiteMap.Config")))
                dir = dir.Parent;
            Assert.NotNull(dir);
            return SiteMapLoader.Load(Path.Combine(dir.FullName, "SMS_App", "SiteMap.Config"));
        }

        private static ApplicationDbContext NewDb() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static UserManager<ApplicationUser> Users(ApplicationDbContext db) =>
            new(new UserStore<ApplicationUser>(db), null, new PasswordHasher<ApplicationUser>(),
                null, null, null, new IdentityErrorDescriber(), null, null);

        [Fact]
        public void PolicyClaims_lists_every_registered_policy()
        {
            var policies = AuthorizationPolicies.PolicyClaims;
            Assert.True(policies.Count >= 250);
            Assert.Contains(("IndexAcademicClassesPolicy", "View Academic Class"), policies);
            Assert.All(policies, p => Assert.False(string.IsNullOrWhiteSpace(p.ClaimType)));
        }

        [Fact]
        public async Task Fresh_database_gets_modules_and_an_admin_who_sees_the_menu()
        {
            using var db = NewDb();
            var users = Users(db);
            var admin = new ApplicationUser { UserName = "admin@brs", Email = "admin@brs", UserType = 'e' };
            Assert.True((await users.CreateAsync(admin)).Succeeded);
            var siteMap = LoadSiteMap();

            var first = await PermissionSeeder.SeedAsync(db, siteMap, users, admin);

            Assert.True(first.Modules > 0);
            Assert.True(first.ClaimStores > 0);
            Assert.Equal(siteMap.Modules.Count(m => !string.IsNullOrWhiteSpace(m.SystemName)),
                await db.ProjectModules.CountAsync(m => m.Status));

            // Same check NavigationViewComponent makes: every menu item with a real policy is visible.
            var policyNames = AuthorizationPolicies.PolicyClaims.Select(p => p.Policy).ToHashSet();
            var claimValues = (await users.GetClaimsAsync(admin)).Select(c => c.Value).ToHashSet();
            var menuPolicies = siteMap.Modules.SelectMany(m => m.Submodules).SelectMany(s => s.Items)
                .Select(i => i.Claim?.Trim()).Where(policyNames.Contains).ToList();
            Assert.NotEmpty(menuPolicies);
            Assert.All(menuPolicies, p => Assert.Contains(p, claimValues));

            var second = await PermissionSeeder.SeedAsync(db, siteMap, users, admin);
            Assert.Equal(new PermissionSeeder.Result(0, 0, 0, 0), second);
        }
    }
}
