using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SMS.BLL.MultiTenancy;

namespace SMS_App.MultiTenancy
{
    /// <summary>
    /// Every login (staff/student cookie, guardian cookie, mobile JWT) carries the code of the
    /// school it was issued for. All schools share one Data Protection key ring and one JWT key,
    /// so without this a cookie or token copied from abc.schoolbd.net would decrypt on
    /// xyz.schoolbd.net. A mismatch (or, in multi-tenant mode, a missing claim) rejects the login.
    /// </summary>
    public static class TenantClaims
    {
        public const string Type = "schoolbd_tenant";

        public static Claim For(ITenantContext tenant) => new(Type, tenant.Required().TenantCode);

        public static bool Matches(ClaimsPrincipal principal, ITenantContext tenant)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return true;
            }
            var claimed = principal.FindFirst(Type)?.Value;
            if (!tenant.IsMultiTenant)
            {
                // Old single-tenant cookies have no claim; accept them, but never a foreign one.
                return claimed == null || claimed == TenantInfo.SingleTenantCode;
            }
            return claimed != null && tenant.Current != null
                && string.Equals(claimed, tenant.Current.TenantCode, StringComparison.Ordinal);
        }

        /// <summary>Cookie OnValidatePrincipal step: reject a cookie that belongs to another school.</summary>
        public static async Task<bool> ValidateCookieAsync(CookieValidatePrincipalContext context)
        {
            var tenant = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
            if (Matches(context.Principal, tenant))
            {
                return true;
            }
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
            return false;
        }

        public static void AddTo(ClaimsIdentity identity, HttpContext httpContext)
        {
            var tenant = httpContext.RequestServices.GetRequiredService<ITenantContext>();
            identity.AddClaim(For(tenant));
        }
    }
}
