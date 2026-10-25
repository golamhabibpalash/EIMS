using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMS_App.Utilities.OnlineAdmission
{
    /// <summary>
    /// The guardian portal's own cookie scheme. It is separate from the staff/student Identity
    /// cookie, so a guardian session can never satisfy a staff authorization policy and signing
    /// in/out of one never affects the other.
    /// </summary>
    public static class GuardianPortalAuth
    {
        public const string Scheme = "AdmissionGuardian";
        public const string CookieName = ".EIMS.AdmissionPortal";
        public const string AccountIdClaim = "admission_guardian_id";

        public static Task SignInAsync(HttpContext httpContext, AdmissionGuardianAccount account)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(AccountIdClaim, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.FullName ?? account.MobileNumber)
            }, Scheme);

            return httpContext.SignInAsync(Scheme, new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });
        }

        public static Task SignOutAsync(HttpContext httpContext) => httpContext.SignOutAsync(Scheme);
    }

    /// <summary>Marks a guardian-portal action that works without a guardian sign-in.</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class GuardianAnonymousAttribute : Attribute
    {
    }

    /// <summary>
    /// Base for the guardian portal. The site-wide staff AuthorizeFilter is bypassed with
    /// [AllowAnonymous]; this class then enforces the guardian scheme itself, redirecting to the
    /// portal sign-in unless the action is marked [GuardianAnonymous].
    /// </summary>
    [AllowAnonymous]
    public abstract class GuardianPortalControllerBase : Controller
    {
        private readonly IAdmissionGuardianManager _guardianManager;
        private readonly IAdmissionSetupManager _setupManager;

        protected GuardianPortalControllerBase(IAdmissionGuardianManager guardianManager, IAdmissionSetupManager setupManager)
        {
            _guardianManager = guardianManager;
            _setupManager = setupManager;
        }

        /// <summary>The signed-in guardian, or null.</summary>
        protected AdmissionGuardianAccount Guardian { get; private set; }

        protected int GuardianId => Guardian?.Id ?? 0;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var auth = await HttpContext.AuthenticateAsync(GuardianPortalAuth.Scheme);
            if (auth.Succeeded && int.TryParse(auth.Principal?.FindFirst(GuardianPortalAuth.AccountIdClaim)?.Value, out var id))
            {
                Guardian = await _guardianManager.GetByIdAsync(id);
                if (Guardian == null)
                {
                    // The account was removed - drop the stale cookie.
                    await GuardianPortalAuth.SignOutAsync(HttpContext);
                }
            }

            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<GuardianAnonymousAttribute>().Any();
            if (!allowAnonymous)
            {
                if (Guardian == null)
                {
                    // Only a GET can be safely replayed after sign-in.
                    var returnUrl = HttpMethods.IsGet(Request.Method) ? Request.Path + Request.QueryString : null;
                    context.Result = RedirectToAction("Login", "AdmissionPortal", new { returnUrl });
                    return;
                }

                // When the school requires a verified mobile, an unverified account can only verify.
                var settings = await _setupManager.GetSettingsAsync();
                if (settings.RequireOtpVerification && !Guardian.IsMobileVerified)
                {
                    context.Result = RedirectToAction("VerifyMobile", "AdmissionPortal");
                    return;
                }
            }

            ViewData["GuardianName"] = Guardian?.FullName;
            await next();
        }
    }
}
