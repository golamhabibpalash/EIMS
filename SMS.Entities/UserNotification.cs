using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// One in-app notification addressed to a single user (the recipient), shown in the
    /// header bell and the notifications page. Deliberately separate from the existing
    /// <see cref="Notification"/> entity, which is unrelated SMS/Email event configuration.
    /// Kept generic so any module can raise one; Tickets is the first producer.
    /// </summary>
    public class UserNotification : CommonProps
    {
        /// <summary>AspNetUsers.Id of the person who should see this notification.</summary>
        [Required, StringLength(450)]
        public string RecipientUserId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(1000)]
        public string Message { get; set; }

        /// <summary>Where clicking the notification takes the user (e.g. "/Tickets/Details/12"); may be null.</summary>
        [StringLength(500)]
        public string Url { get; set; }

        public NotificationCategory Category { get; set; } = NotificationCategory.General;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
    }

    /// <summary>Broad grouping for the icon/colour shown against a notification.</summary>
    public enum NotificationCategory
    {
        [Display(Name = "General")]
        General = 0,

        [Display(Name = "Ticket")]
        Ticket = 1,
        Admission = 2
    }
}
