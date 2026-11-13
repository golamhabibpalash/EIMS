using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// A request raised by a user for the developer to act on: a fault, an idea,
    /// a question. Discussion lives in <see cref="TicketComments"/> and files in
    /// <see cref="TicketAttachments"/>.
    /// </summary>
    public class Ticket : CommonProps
    {
        [Display(Name = "Title"), Required, StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Description"), Required]
        public string Description { get; set; }

        [Display(Name = "Raise Date")]
        public DateTime RaiseDate { get; set; }

        [Display(Name = "Status")]
        public TicketStatus Status { get; set; } = TicketStatus.Pending;

        [Display(Name = "Type")]
        public TicketType TicketType { get; set; } = TicketType.Issue;

        /// <summary>AspNetUsers.Id of the person who raised it.</summary>
        [Display(Name = "Raised By"), StringLength(450)]
        public string RaisedByUserId { get; set; }

        /// <summary>AspNetUsers.Id of the developer handling it; null until assigned.</summary>
        [Display(Name = "Assigned To"), StringLength(450)]
        public string AssignedToUserId { get; set; }

        /// <summary>Set when the status first moves to Solved or Rejected.</summary>
        [Display(Name = "Closed At")]
        public DateTime? ClosedAt { get; set; }

        public List<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

        public List<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();
    }
}
