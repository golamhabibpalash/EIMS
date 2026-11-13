using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Per-user in-app notifications. Persists only; resolving recipients (e.g. "all SuperAdmins")
    /// and pushing over SignalR stay in the presentation layer, which owns Identity and the hub.
    /// </summary>
    public class UserNotificationManager : Manager<UserNotification>, IUserNotificationManager
    {
        private readonly IUserNotificationRepository _userNotificationRepository;

        public UserNotificationManager(IUserNotificationRepository userNotificationRepository)
            : base(userNotificationRepository)
        {
            _userNotificationRepository = userNotificationRepository;
        }

        public async Task<IReadOnlyCollection<UserNotification>> GetByRecipientAsync(string userId, bool unreadOnly = false, int? take = null)
        {
            return await _userNotificationRepository.GetByRecipientAsync(userId, unreadOnly, take);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _userNotificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkReadAsync(int id, string userId)
        {
            return await _userNotificationRepository.MarkReadAsync(id, userId);
        }

        public async Task<int> MarkAllReadAsync(string userId)
        {
            return await _userNotificationRepository.MarkAllReadAsync(userId);
        }

        public async Task<IReadOnlyCollection<UserNotification>> NotifyAsync(
            IEnumerable<string> recipientUserIds,
            string title,
            string message,
            string url,
            NotificationCategory category,
            string createdBy,
            string macAddress)
        {
            var saved = new List<UserNotification>();

            if (recipientUserIds is null || string.IsNullOrWhiteSpace(title))
            {
                return saved;
            }

            var now = DateTime.Now;

            foreach (var recipientId in recipientUserIds
                         .Where(id => !string.IsNullOrWhiteSpace(id))
                         .Distinct())
            {
                var notification = new UserNotification
                {
                    RecipientUserId = recipientId,
                    Title = title,
                    Message = message,
                    Url = url,
                    Category = category,
                    IsRead = false,
                    CreatedBy = createdBy,
                    CreatedAt = now,
                    EditedBy = createdBy,
                    EditedAt = now,
                    MACAddress = macAddress ?? string.Empty
                };

                if (await _userNotificationRepository.AddAsync(notification))
                {
                    saved.Add(notification);
                }
            }

            return saved;
        }
    }
}
