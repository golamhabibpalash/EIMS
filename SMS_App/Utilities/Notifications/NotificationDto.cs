namespace SMS_App.Utilities.Notifications
{
    /// <summary>
    /// The shape pushed to the browser over SignalR and returned by the notifications JSON
    /// endpoints. Kept flat and presentation-ready so the client can render without extra lookups.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public bool IsRead { get; set; }
        public string CreatedAtText { get; set; }

        /// <summary>The recipient's unread total after this notification, so the badge can be set exactly.</summary>
        public int UnreadCount { get; set; }
    }
}
