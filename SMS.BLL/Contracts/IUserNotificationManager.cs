using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IUserNotificationManager : IManager<UserNotification>
    {
        /// <summary>A recipient's notifications, newest first; optionally unread only, optionally capped.</summary>
        Task<IReadOnlyCollection<UserNotification>> GetByRecipientAsync(string userId, bool unreadOnly = false, int? take = null);

        /// <summary>Count of a recipient's unread notifications (header badge).</summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>Marks one notification read if it belongs to the recipient.</summary>
        Task<bool> MarkReadAsync(int id, string userId);

        /// <summary>Marks all of a recipient's notifications read; returns how many changed.</summary>
        Task<int> MarkAllReadAsync(string userId);

        /// <summary>
        /// Persists one notification per recipient, stamping the audit fields. Returns the saved
        /// notifications so the caller (presentation layer) can push them over SignalR.
        /// </summary>
        Task<IReadOnlyCollection<UserNotification>> NotifyAsync(
            IEnumerable<string> recipientUserIds,
            string title,
            string message,
            string url,
            NotificationCategory category,
            string createdBy,
            string macAddress);
    }
}
