using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class UserNotificationRepository : Repository<UserNotification>, IUserNotificationRepository
    {
        public UserNotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<UserNotification>> GetByRecipientAsync(string userId, bool unreadOnly = false, int? take = null)
        {
            var query = _context.UserNotifications
                .Where(n => n.RecipientUserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            query = query
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.UserNotifications
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkReadAsync(int id, string userId)
        {
            // Scope to the recipient so a crafted id can never read/flip another user's row.
            var notification = await _context.UserNotifications
                .AsTracking()
                .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId);

            if (notification is null || notification.IsRead)
            {
                return notification is not null; // already read is still "success" from the caller's view
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> MarkAllReadAsync(string userId)
        {
            var unread = await _context.UserNotifications
                .AsTracking()
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
            {
                return 0;
            }

            var now = DateTime.Now;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }

            await _context.SaveChangesAsync();
            return unread.Count;
        }
    }
}
