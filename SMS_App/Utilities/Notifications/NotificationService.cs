using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Hubs;
using SMS_App.Utilities.LoggerService;

namespace SMS_App.Utilities.Notifications
{
    public class NotificationService : INotificationService
    {
        public const string SuperAdminRole = "SuperAdmin";

        private readonly IUserNotificationManager _userNotificationManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IAppLogger _appLogger;

        public NotificationService(
            IUserNotificationManager userNotificationManager,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hub,
            IAppLogger appLogger)
        {
            _userNotificationManager = userNotificationManager;
            _userManager = userManager;
            _hub = hub;
            _appLogger = appLogger;
        }

        public async Task NotifyTicketRaisedAsync(Ticket ticket, string raisedByUserId, string macAddress)
        {
            if (ticket is null)
            {
                return;
            }

            try
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(SuperAdminRole);

                // A SuperAdmin raising their own ticket should not be pinged about it.
                var recipientIds = superAdmins
                    .Select(u => u.Id)
                    .Where(id => id != raisedByUserId)
                    .ToList();

                if (recipientIds.Count == 0)
                {
                    return;
                }

                var title = "New ticket raised";
                var message = $"{ticket.TicketType}: {ticket.Title}";
                var url = $"/Tickets/Details/{ticket.Id}";

                await PersistAndPushAsync(recipientIds, title, message, url, NotificationCategory.Ticket, raisedByUserId, macAddress);
            }
            catch (Exception ex)
            {
                await SafeLogAsync($"Failed to notify SuperAdmins of ticket #{ticket.Id}", ex);
            }
        }

        public async Task NotifyTicketStatusChangedAsync(Ticket ticket, TicketStatus newStatus, string actingUserId, string macAddress)
        {
            if (ticket is null || string.IsNullOrEmpty(ticket.RaisedByUserId))
            {
                return;
            }

            // No point telling someone about a change they just made themselves.
            if (ticket.RaisedByUserId == actingUserId)
            {
                return;
            }

            try
            {
                var title = "Ticket status updated";
                var message = $"Your ticket \"{ticket.Title}\" is now {newStatus}.";
                var url = $"/Tickets/Details/{ticket.Id}";

                await PersistAndPushAsync(
                    new[] { ticket.RaisedByUserId },
                    title, message, url, NotificationCategory.Ticket, actingUserId, macAddress);
            }
            catch (Exception ex)
            {
                await SafeLogAsync($"Failed to notify owner of ticket #{ticket.Id} status change", ex);
            }
        }

        public async Task PushAsync(string recipientUserId, NotificationDto payload)
        {
            if (string.IsNullOrEmpty(recipientUserId) || payload is null)
            {
                return;
            }

            try
            {
                await _hub.Clients.Group(NotificationHub.GroupFor(recipientUserId))
                    .SendAsync(NotificationHub.ReceiveNotification, payload);
            }
            catch (Exception ex)
            {
                await SafeLogAsync($"SignalR push to {recipientUserId} failed", ex);
            }
        }

        private async Task PersistAndPushAsync(
            IReadOnlyCollection<string> recipientIds,
            string title,
            string message,
            string url,
            NotificationCategory category,
            string createdBy,
            string macAddress)
        {
            // Persist first - a stored notification the user sees on next load is the floor we
            // guarantee; the live push on top is best-effort.
            var saved = await _userNotificationManager.NotifyAsync(
                recipientIds, title, message, url, category, createdBy, macAddress);

            foreach (var notification in saved)
            {
                var unread = await _userNotificationManager.GetUnreadCountAsync(notification.RecipientUserId);
                await PushAsync(notification.RecipientUserId, new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Url = notification.Url,
                    Category = notification.Category.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAtText = notification.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                    UnreadCount = unread
                });
            }
        }

        private async Task SafeLogAsync(string context, Exception ex)
        {
            try
            {
                await _appLogger.ErrorAsync(context, ex.Message);
            }
            catch
            {
                // Logging must never become the reason a workflow fails.
            }
        }
    }
}
