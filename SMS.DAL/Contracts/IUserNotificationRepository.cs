using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IUserNotificationRepository : IRepository<UserNotification>
    {
        /// <summary>A recipient's notifications, newest first; optionally unread only, optionally capped.</summary>
        Task<IReadOnlyCollection<UserNotification>> GetByRecipientAsync(string userId, bool unreadOnly = false, int? take = null);

        /// <summary>Count of a recipient's unread notifications (for the header badge).</summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>Marks one notification read, but only if it belongs to the given recipient. Returns false if not found/owned.</summary>
        Task<bool> MarkReadAsync(int id, string userId);

        /// <summary>Marks every unread notification of a recipient as read; returns how many changed.</summary>
        Task<int> MarkAllReadAsync(string userId);
    }
}
