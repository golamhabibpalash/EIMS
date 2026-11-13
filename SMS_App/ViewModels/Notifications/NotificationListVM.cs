using System;
using System.Collections.Generic;
using SMS.Entities;

namespace SMS_App.ViewModels.Notifications
{
    public class NotificationListVM
    {
        public int UnreadCount { get; set; }
        public List<NotificationItemVM> Items { get; set; } = new List<NotificationItemVM>();
    }

    public class NotificationItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public NotificationCategory Category { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
