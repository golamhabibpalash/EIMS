using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Resources/sql/Seed_AllMenus_SuperAdmin_PostgreSQL.sql is generated from SiteMap.Config and
    /// AuthorizationPolicies through PermissionSeeder, so it gives exactly what tenant-init gives.
    /// This test fails when the script is out of date; regenerate it with
    ///   REGENERATE_MENU_SCRIPT=1 dotnet test SMS.Tests --filter SuperAdminMenuScriptTests
    /// </summary>
    public class SuperAdminMenuScriptTests
    {
        private const string ScriptName = "Seed_AllMenus_SuperAdmin_PostgreSQL.sql";

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SMS_App", "SiteMap.Config")))
                dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }

        [Fact]
        public async Task Script_matches_sitemap_and_policies()
        {
            var root = RepoRoot();
            var expected = await BuildScriptAsync(SiteMapLoader.Load(Path.Combine(root, "SMS_App", "SiteMap.Config")));
            var path = Path.Combine(root, "Resources", "sql", ScriptName);

            if (Environment.GetEnvironmentVariable("REGENERATE_MENU_SCRIPT") == "1")
                await File.WriteAllTextAsync(path, expected);

            Assert.True(File.Exists(path), $"{ScriptName} is missing; regenerate it (see the class comment).");
            Assert.Equal(expected, (await File.ReadAllTextAsync(path)).Replace("\r\n", "\n"));
        }

        private static async Task<string> BuildScriptAsync(SiteMap siteMap)
        {
            // Run the real seeder on an empty in-memory database, then turn its rows into SQL.
            using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var users = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db), null,
                new PasswordHasher<ApplicationUser>(), null, null, null, new IdentityErrorDescriber(), null, null);
            var admin = new ApplicationUser { UserName = "seed", Email = "seed", UserType = 'e' };
            await users.CreateAsync(admin);
            await PermissionSeeder.SeedAsync(db, siteMap, users, admin);

            var modules = await db.ProjectModules.OrderBy(m => m.Id).ToListAsync();
            var subs = await db.ProjectSubModules.OrderBy(s => s.Id).ToListAsync();
            var claims = await db.ClaimStores.OrderBy(c => c.Id).ToListAsync();
            var moduleName = modules.ToDictionary(m => m.Id, m => m.ModuleName);
            var subKey = subs.ToDictionary(s => s.Id, s => (Module: moduleName[s.ProjectModuleId], Sub: s.SubModuleName));
            var grants = AuthorizationPolicies.PolicyClaims.Select(p => (p.ClaimType, p.Policy)).Distinct()
                .OrderBy(p => p.Policy, StringComparer.Ordinal).ThenBy(p => p.ClaimType, StringComparer.Ordinal).ToList();

            var sb = new StringBuilder();
            sb.Append(@"-- ============================================================================
-- Full menu + every permission for one SuperAdmin - POSTGRESQL
-- GENERATED from SMS_App/SiteMap.Config + AuthorizationPolicies (see
-- SMS.Tests/MultiTenancy/SuperAdminMenuScriptTests.cs); do not edit by hand.
--
-- For a school database whose menu is empty. It:
--   1. adds every missing ProjectModule / ProjectSubModule / ClaimStores row and switches
--      every menu module ON,
--   2. makes sure the SuperAdmin role exists and the user below is in it,
--   3. grants every permission to the SuperAdmin ROLE and to the user directly.
-- IDEMPOTENT and additive: safe to re-run; nothing is deleted.
--
-- Works in DBeaver / pgAdmin and in psql: it is ONE statement (a DO block), all or nothing.
--   1. Change the email on the line marked  <<< ADMIN EMAIL  if needed.
--   2. DBeaver: open on the school database (sbd_<code>) and run it (Ctrl+Enter or Alt+X).
--      psql:    sudo -u postgres psql -p 5433 -d sbd_<code> -v ON_ERROR_STOP=1 -f Seed_AllMenus_SuperAdmin_PostgreSQL.sql
--   3. Restart the app (sudo systemctl restart schoolbd-platform) and log in again.
-- Progress is reported as NOTICE messages (DBeaver: Output tab).
-- ============================================================================
DO $seed$
DECLARE
    v_email   text := 'golamhabibpalash@gmail.com';   -- <<< ADMIN EMAIL
    v_user_id text;
    v_role_id text;
    n         integer;
BEGIN
    SELECT ""Id"" INTO v_user_id FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" = upper(v_email) LIMIT 1;
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'No user with email % in this database. Create the user first (or change the email at the top).', v_email;
    END IF;

    CREATE TEMP TABLE seed_module (name text, remarks text);
    CREATE TEMP TABLE seed_sub (module text, name text, remarks text);
    CREATE TEMP TABLE seed_claim (module text, sub text, claim_type text, claim_value text);
    CREATE TEMP TABLE seed_grant (claim_type text, claim_value text);

");
            AppendValues(sb, "seed_module", modules.Select(m => new[] { m.ModuleName, m.Remarks }));
            AppendValues(sb, "seed_sub", subs.Select(s => new[] { moduleName[s.ProjectModuleId], s.SubModuleName, s.Remarks }));
            AppendValues(sb, "seed_claim", claims.Select(c =>
                new[] { subKey[c.SubModuleId].Module, subKey[c.SubModuleId].Sub, c.ClaimType, c.ClaimValue }));
            AppendValues(sb, "seed_grant", grants.Select(g => new[] { g.ClaimType, g.Policy }));

            sb.Append(@"    -- 1. Modules, sub-modules, permissions
    INSERT INTO ""ProjectModules"" (""ModuleName"", ""Status"", ""Remarks"", ""CreatedBy"", ""CreatedAt"", ""EditedBy"", ""EditedAt"", ""MACAddress"")
    SELECT m.name, true, m.remarks, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', ''
    FROM seed_module m
    WHERE NOT EXISTS (SELECT 1 FROM ""ProjectModules"" p WHERE p.""ModuleName"" = m.name);
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'modules added: %', n;

    UPDATE ""ProjectModules"" p SET ""Status"" = true
    FROM seed_module m WHERE p.""ModuleName"" = m.name AND p.""Status"" = false;
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'modules switched on: %', n;

    INSERT INTO ""ProjectSubModules"" (""SubModuleName"", ""ProjectModuleId"", ""Status"", ""Remarks"", ""CreatedBy"", ""CreatedAt"", ""EditedBy"", ""EditedAt"", ""MACAddress"")
    SELECT s.name, pm.""Id"", true, s.remarks, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', ''
    FROM seed_sub s
    JOIN LATERAL (SELECT ""Id"" FROM ""ProjectModules"" WHERE ""ModuleName"" = s.module ORDER BY ""Id"" LIMIT 1) pm ON true
    WHERE NOT EXISTS (SELECT 1 FROM ""ProjectSubModules"" x WHERE x.""SubModuleName"" = s.name AND x.""ProjectModuleId"" = pm.""Id"");
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'sub-modules added: %', n;

    INSERT INTO ""ClaimStores"" (""ClaimType"", ""ClaimValue"", ""SubModuleId"", ""CreatedBy"", ""CreatedAt"", ""EditedBy"", ""EditedAt"", ""MACAddress"")
    SELECT c.claim_type, c.claim_value, ps.""Id"", 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', ''
    FROM seed_claim c
    JOIN LATERAL (SELECT ""Id"" FROM ""ProjectModules"" WHERE ""ModuleName"" = c.module ORDER BY ""Id"" LIMIT 1) pm ON true
    JOIN LATERAL (SELECT ""Id"" FROM ""ProjectSubModules"" WHERE ""SubModuleName"" = c.sub AND ""ProjectModuleId"" = pm.""Id"" ORDER BY ""Id"" LIMIT 1) ps ON true
    WHERE NOT EXISTS (SELECT 1 FROM ""ClaimStores"" x WHERE x.""ClaimValue"" = c.claim_value);
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'permissions added: %', n;

    -- 2. SuperAdmin role and membership
    SELECT ""Id"" INTO v_role_id FROM ""AspNetRoles"" WHERE ""NormalizedName"" = 'SUPERADMIN' LIMIT 1;
    IF v_role_id IS NULL THEN
        v_role_id := gen_random_uuid()::text;
        INSERT INTO ""AspNetRoles"" (""Id"", ""Name"", ""NormalizedName"", ""ConcurrencyStamp"")
        VALUES (v_role_id, 'SuperAdmin', 'SUPERADMIN', gen_random_uuid()::text);
        RAISE NOTICE 'SuperAdmin role created';
    END IF;
    INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
    SELECT v_user_id, v_role_id
    WHERE NOT EXISTS (SELECT 1 FROM ""AspNetUserRoles"" WHERE ""UserId"" = v_user_id AND ""RoleId"" = v_role_id);

    -- 3. Every permission: on the role (all SuperAdmins) and on the user (ticked in User Profile)
    INSERT INTO ""AspNetRoleClaims"" (""RoleId"", ""ClaimType"", ""ClaimValue"")
    SELECT v_role_id, g.claim_type, g.claim_value FROM seed_grant g
    WHERE NOT EXISTS (SELECT 1 FROM ""AspNetRoleClaims"" x
                      WHERE x.""RoleId"" = v_role_id AND x.""ClaimType"" = g.claim_type AND x.""ClaimValue"" = g.claim_value);
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'role permissions granted: %', n;

    INSERT INTO ""AspNetUserClaims"" (""UserId"", ""ClaimType"", ""ClaimValue"")
    SELECT v_user_id, g.claim_type, g.claim_value FROM seed_grant g
    WHERE NOT EXISTS (SELECT 1 FROM ""AspNetUserClaims"" x
                      WHERE x.""UserId"" = v_user_id AND x.""ClaimType"" = g.claim_type AND x.""ClaimValue"" = g.claim_value);
    GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'user permissions granted: %', n;

    DROP TABLE pg_temp.seed_module, pg_temp.seed_sub, pg_temp.seed_claim, pg_temp.seed_grant;
END $seed$;
");
            return sb.ToString();
        }

        private static void AppendValues(StringBuilder sb, string table, IEnumerable<string[]> rows)
        {
            var list = rows.ToList();
            if (list.Count == 0) return;
            sb.Append("    INSERT INTO ").Append(table).Append(" VALUES\n");
            sb.Append(string.Join(",\n", list.Select(r => "        (" + string.Join(", ", r.Select(Literal)) + ")")));
            sb.Append(";\n\n");
        }

        private static string Literal(string value) =>
            value == null ? "NULL" : "'" + value.Replace("'", "''") + "'";
    }
}
