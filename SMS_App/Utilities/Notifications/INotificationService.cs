using System.Threading.Tasks;
using SMS.Entities;

namespace SMS_App.Utilities.Notifications
{
    /// <summary>
    /// Presentation-layer glue over <see cref="SMS.BLL.Contracts.IUserNotificationManager"/>:
    /// resolves recipients from Identity, persists the notifications, and pushes them live over
    /// SignalR. Every method is best-effort and must never throw into the caller's workflow -
    /// a failed notification can never be allowed to break raising or updating a ticket.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Notify every SuperAdmin (except the raiser) that a ticket was raised.</summary>
        Task NotifyTicketRaisedAsync(Ticket ticket, string raisedByUserId, string macAddress);

        /// <summary>Notify the ticket's owner that its status changed (unless they made the change).</summary>
        Task NotifyTicketStatusChangedAsync(Ticket ticket, TicketStatus newStatus, string actingUserId, string macAddress);

        /// <summary>Notify every user in any of <paramref name="roles"/> (except <paramref name="excludeUserId"/>). Best-effort.</summary>
        Task NotifyRolesAsync(System.Collections.Generic.IEnumerable<string> roles, string title, string message, string url,
            NotificationCategory category, string excludeUserId, string macAddress);

        /// <summary>Push an already-persisted notification to one recipient's SignalR group (best-effort).</summary>
        Task PushAsync(string recipientUserId, NotificationDto payload);
    }
}
