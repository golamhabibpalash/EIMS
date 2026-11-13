using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.Notifications;
using SMS_App.ViewModels.Notifications;

namespace SMS_App.Controllers
{
    /// <summary>
    /// The per-user notification centre: the header bell's data endpoints and the full
    /// "All notifications" page. Every action is scoped to the signed-in user, so there is no
    /// claim policy - any authenticated user may read and clear their own notifications.
    /// </summary>
    [Authorize]
    public class NotificationsController : Controller
    {
        private const int RecentCount = 10;

        private readonly IUserNotificationManager _userNotificationManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            IUserNotificationManager userNotificationManager,
            UserManager<ApplicationUser> userManager)
        {
            _userNotificationManager = userNotificationManager;
            _userManager = userManager;
        }

        // GET: /Notifications - full history page
        public async Task<IActionResult> Index()
        {
            GlobalUI.PageTitle = "Notifications";

            var userId = _userManager.GetUserId(User);
            var items = await _userNotificationManager.GetByRecipientAsync(userId);

            return View(new NotificationListVM
            {
                UnreadCount = items.Count(n => !n.IsRead),
                Items = items.Select(ToItemVM).ToList()
            });
        }

        // GET: /Notifications/UnreadCount - badge value (polling / initial load fallback)
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            return Json(new { count = await _userNotificationManager.GetUnreadCountAsync(userId) });
        }

        // GET: /Notifications/Recent - dropdown contents
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _userNotificationManager.GetByRecipientAsync(userId, unreadOnly: false, take: RecentCount);

            return Json(new
            {
                unreadCount = await _userNotificationManager.GetUnreadCountAsync(userId),
                items = items.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    url = n.Url,
                    category = n.Category.ToString(),
                    isRead = n.IsRead,
                    createdAtText = n.CreatedAt.ToString("dd MMM yyyy HH:mm")
                })
            });
        }

        // POST: /Notifications/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _userNotificationManager.MarkReadAsync(id, userId);
            return Json(new { ok = true, unreadCount = await _userNotificationManager.GetUnreadCountAsync(userId) });
        }

        // POST: /Notifications/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User);
            var changed = await _userNotificationManager.MarkAllReadAsync(userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { ok = true, changed });
            }

            TempData["success"] = changed > 0 ? $"{changed} notification(s) marked read." : "No unread notifications.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notifications/Open/5 - mark read then go where the notification points
        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _userNotificationManager.MarkReadAsync(id, userId);

            var notification = await _userNotificationManager.GetByIdAsync(id);

            // Only honour the stored link if the notification really belongs to this user and the
            // target is a local path - never redirect to an absolute/off-site URL from stored data.
            if (notification is not null
                && notification.RecipientUserId == userId
                && !string.IsNullOrWhiteSpace(notification.Url)
                && Url.IsLocalUrl(notification.Url))
            {
                return Redirect(notification.Url);
            }

            return RedirectToAction(nameof(Index));
        }

        private static NotificationItemVM ToItemVM(UserNotification n) => new NotificationItemVM
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Url = n.Url,
            Category = n.Category,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
