using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMS_App.Configurations;

/// <summary>
/// Rehydrates permission claims (user + role claims from AspNetUserClaims /
/// AspNetRoleClaims) into the request principal. Runs server-side on every
/// authenticated request, so the login cookie only carries the slim identity
/// built by <see cref="AppClaimsPrincipalFactory"/> and can never blow past
/// header limits again.
///
/// Results are cached per user + security stamp (15 min). A stamp change
/// (password change, admin stamp reset) busts the cache immediately; plain
/// claim edits take effect within the cache window or at next login.
/// </summary>
public class PermissionClaimsTransformation : IClaimsTransformation
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(15);

    public static string CacheKey(string userId, string securityStamp) =>
        $"permclaims:{userId}:{securityStamp}";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMemoryCache _cache;

    public PermissionClaimsTransformation(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _cache = cache;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return principal;

        var userId = _userManager.GetUserId(principal);
        if (string.IsNullOrEmpty(userId))
            return principal;

        var stamp = principal.FindFirstValue(_userManager.Options.ClaimsIdentity.SecurityStampClaimType);
        var cacheKey = CacheKey(userId, stamp);

        if (!_cache.TryGetValue(cacheKey, out List<Claim> claims))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return principal;

            var collected = new List<Claim>(await _userManager.GetClaimsAsync(user));
            foreach (var roleName in await _userManager.GetRolesAsync(user))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                    collected.AddRange(await _roleManager.GetClaimsAsync(role));
            }

            claims = collected
                .GroupBy(c => new { c.Type, c.Value })
                .Select(g => g.First())
                .ToList();

            _cache.Set(cacheKey, claims, CacheLifetime);
        }

        var identity = (ClaimsIdentity)principal.Identity;
        foreach (var claim in claims)
        {
            if (!principal.HasClaim(claim.Type, claim.Value))
                identity.AddClaim(claim);
        }

        return principal;
    }
}
