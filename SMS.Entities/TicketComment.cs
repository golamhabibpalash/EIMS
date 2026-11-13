using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// One entry in a ticket's discussion. CreatedBy / CreatedAt on
    /// <see cref="CommonProps"/> carry the author and the time.
    /// </summary>
    public class TicketComment : CommonProps
    {
        [Display(Name = "Ticket")]
        public int TicketId { get; set; }

        public Ticket Ticket { get; set; }

        [Display(Name = "Comment"), Required]
        public string Message { get; set; }

        /// <summary>
        /// True for entries the application writes itself - status changes and
        /// assignments - so the view can style them apart from real discussion.
        /// </summary>
        [Display(Name = "System Note")]
        public bool IsSystemNote { get; set; }
    }
}
