using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMS.DB;
using SMS.Entities;
using SMS_App.ViewModels.ModuleSubModuleVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMS_App.Configurations;

/// <summary>
/// Gives a freshly created school database what the menu and the permission screens need:
/// ProjectModules / ProjectSubModules / ClaimStores built from SiteMap.Config, and every
/// policy claim granted to the school's admin. Provider-neutral C# counterpart of
/// Resources/sql/Seed_Modules_SubModules_ClaimStores_FromSiteMap.sql +
/// 2026-08-30_SeedClaimStores_and_GrantSuperAdmin.sql (which are SQL Server only).
///
/// Idempotent and additive: existing rows are never updated or removed, so module Status flags
/// a school changed by hand are kept and re-running is safe.
/// </summary>
public static class PermissionSeeder
{
    private const string SeedUser = "SiteMapSeed";

    public sealed record Result(int Modules, int SubModules, int ClaimStores, int AdminClaims)
    {
        public override string ToString() =>
            $"added {Modules} modules, {SubModules} sub-modules, {ClaimStores} permissions, {AdminClaims} admin claims";
    }

    public static async Task<Result> SeedAsync(ApplicationDbContext db, SiteMap siteMap,
        UserManager<ApplicationUser> userManager, ApplicationUser admin)
    {
        var policies = AuthorizationPolicies.PolicyClaims;
        var claimTypeOf = policies
            .GroupBy(p => p.Policy, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().ClaimType, StringComparer.OrdinalIgnoreCase);

        var now = DateTime.Now;
        int addedModules = 0, addedSubs = 0, addedClaims = 0;

        var modules = await db.ProjectModules.AsTracking().Include(m => m.SubModuleList).ToListAsync();
        var knownClaimValues = new HashSet<string>(
            await db.ClaimStores.Select(c => c.ClaimValue).ToListAsync(), StringComparer.OrdinalIgnoreCase);

        // Menu items first, remembering which sub-module each "Index..." page belongs to so the
        // page's Create/Edit/Delete policies can be filed under the same sub-module.
        var subModuleByEntity = new List<(string Suffix, ProjectSubModule Sub)>();

        foreach (var module in siteMap.Modules.Where(m => !string.IsNullOrWhiteSpace(m.SystemName)))
        {
            var dbModule = modules.FirstOrDefault(m => m.ModuleName == module.SystemName);
            if (dbModule == null)
            {
                dbModule = new ProjectModule
                {
                    ModuleName = module.SystemName,
                    Status = true,
                    Remarks = module.DisplayName,
                    SubModuleList = new List<ProjectSubModule>(),
                    CreatedBy = SeedUser, CreatedAt = now, EditedBy = SeedUser, EditedAt = now
                };
                db.ProjectModules.Add(dbModule);
                modules.Add(dbModule);
                addedModules++;
            }
            dbModule.SubModuleList ??= new List<ProjectSubModule>();

            foreach (var sub in module.Submodules.Where(s => !string.IsNullOrWhiteSpace(s.SystemName)))
            {
                var dbSub = dbModule.SubModuleList.FirstOrDefault(s => s.SubModuleName == sub.SystemName);
                if (dbSub == null)
                {
                    dbSub = new ProjectSubModule
                    {
                        SubModuleName = sub.SystemName,
                        ProjectModule = dbModule,
                        Status = true,
                        Remarks = sub.DisplayName,
                        ClaimStoresList = new List<ClaimStores>(),
                        CreatedBy = SeedUser, CreatedAt = now, EditedBy = SeedUser, EditedAt = now
                    };
                    dbModule.SubModuleList.Add(dbSub);
                    addedSubs++;
                }

                foreach (var policy in sub.Items.Select(i => i.Claim?.Trim()).Where(c => !string.IsNullOrEmpty(c)))
                {
                    var suffix = EntitySuffix(policy);
                    if (suffix != null)
                        subModuleByEntity.Add((suffix, dbSub));

                    if (claimTypeOf.TryGetValue(policy, out var claimType) && knownClaimValues.Add(policy))
                    {
                        db.ClaimStores.Add(NewClaim(claimType, policy, dbSub, now));
                        addedClaims++;
                    }
                }
            }
        }

        // Page-action policies (Create/Edit/Delete/Details...): file each under the sub-module of the
        // longest matching Index page. Ones with no menu page are still granted to the admin below.
        foreach (var (policy, claimType) in policies)
        {
            if (knownClaimValues.Contains(policy))
                continue;
            var owner = subModuleByEntity
                .Where(e => policy.EndsWith(e.Suffix, StringComparison.Ordinal))
                .OrderByDescending(e => e.Suffix.Length)
                .Select(e => e.Sub)
                .FirstOrDefault();
            if (owner != null && knownClaimValues.Add(policy))
            {
                db.ClaimStores.Add(NewClaim(claimType, policy, owner, now));
                addedClaims++;
            }
        }

        await db.SaveChangesAsync();

        // Grant the admin every policy. A claim's Type satisfies the policy; its Value (the policy
        // name) is what NavigationViewComponent matches menu items against.
        var existing = (await userManager.GetClaimsAsync(admin))
            .Select(c => (c.Type, c.Value))
            .ToHashSet();
        var grant = policies
            .Select(p => (p.ClaimType, p.Policy))
            .Distinct()
            .Where(p => !existing.Contains(p))
            .Select(p => new Claim(p.ClaimType, p.Policy))
            .ToList();
        if (grant.Count > 0)
        {
            var result = await userManager.AddClaimsAsync(admin, grant);
            if (!result.Succeeded)
                throw new InvalidOperationException("Granting admin claims failed: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return new Result(addedModules, addedSubs, addedClaims, grant.Count);
    }

    /// <summary>"IndexAcademicClassesPolicy" → "AcademicClassesPolicy"; null for non-index pages.</summary>
    private static string EntitySuffix(string policy)
    {
        foreach (var prefix in new[] { "Index", "View" })
        {
            if (policy.StartsWith(prefix, StringComparison.Ordinal) && policy.Length > prefix.Length + "Policy".Length)
                return policy[prefix.Length..];
        }
        return null;
    }

    private static ClaimStores NewClaim(string claimType, string policy, ProjectSubModule sub, DateTime now) => new()
    {
        ClaimType = claimType,
        ClaimValue = policy,
        SubModule = sub,
        CreatedBy = SeedUser, CreatedAt = now, EditedBy = SeedUser, EditedAt = now
    };
}
