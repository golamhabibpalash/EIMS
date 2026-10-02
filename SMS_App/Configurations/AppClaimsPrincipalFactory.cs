using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SMS.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMS_App.Configurations;

/// <summary>
/// Identity's default factory embeds EVERY user/role claim into the login
/// cookie. With ~250 permission claims the cookie outgrows IIS header limits
/// and every request dies with "400 - headers too long". This factory keeps
/// only the essentials (id, name, roles, security stamp) in the cookie;
/// <see cref="PermissionClaimsTransformation"/> rehydrates the permission
/// claims server-side on each request, so the cookie stays ~1 KB no matter
/// how many permissions a user holds.
/// </summary>
public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
            IdentityConstants.ApplicationScheme,
            Options.ClaimsIdentity.UserNameClaimType,
            Options.ClaimsIdentity.RoleClaimType);

        identity.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, await UserManager.GetUserIdAsync(user)));
        identity.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, await UserManager.GetUserNameAsync(user)));

        if (UserManager.SupportsUserSecurityStamp)
        {
            identity.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType,
                await UserManager.GetSecurityStampAsync(user)));
        }

        if (UserManager.SupportsUserRole)
        {
            foreach (var roleName in await UserManager.GetRolesAsync(user))
                identity.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, roleName));
        }

        return identity;
    }
}
