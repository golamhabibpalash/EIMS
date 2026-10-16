using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;

namespace SMS_App.ViewComponents
{
    /// <summary>
    /// Renders the signed-in user's unread notification count for the header bell, so the badge
    /// is correct on first paint without waiting for the JS/SignalR layer. Outputs just the number.
    /// </summary>
    public class NotificationCountViewComponent : ViewComponent
    {
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationCountViewComponent(
            IUserNotificationManager userNotificationManager,
            UserManager<ApplicationUser> userManager)
        {
            _userNotificationManager = userNotificationManager;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;
            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                if (!string.IsNullOrEmpty(userId))
                {
                    count = await _userNotificationManager.GetUnreadCountAsync(userId);
                }
            }
            catch (Exception)
            {
                // This renders inside the shared layout on every admin page. If the
                // UserNotifications table is not present yet (DLLs deployed before the
                // CREATE TABLE script was run), degrade to "no badge" rather than break
                // every page. The bell simply shows 0 until the table exists.
                count = 0;
            }

            return Content(count.ToString());
        }
    }
}
