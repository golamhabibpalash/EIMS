using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SMS_App.Utilities.Automation.Hangfire
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)        
        {
            var httpContext = context.GetHttpContext();
            //bool isAuthenticated = httpContext.User.Identity.IsAuthenticated;
            //if (!isAuthenticated)
            //{
            //    return false;
            //}
            //bool isAuthorized = httpContext.User.IsInRole("SuperAdmin"); 
            //if (!isAuthorized)
            //{
            //    return false;
            //}
            return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("SuperAdmin");
        }
    }
}
