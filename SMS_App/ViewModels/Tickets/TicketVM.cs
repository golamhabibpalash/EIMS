using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.Tickets
{
    /// <summary>Form model for raising a ticket.</summary>
    public class TicketCreateVM
    {
        [Display(Name = "Title"), Required(ErrorMessage = "Give the ticket a short title.")]
        [StringLength(200, ErrorMessage = "Keep the title under 200 characters.")]
        public string Title { get; set; }

        [Display(Name = "Description"), Required(ErrorMessage = "Describe what you need.")]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public TicketType TicketType { get; set; } = TicketType.Issue;

        [Display(Name = "Attachments")]
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();

        public SelectList TicketTypeList { get; set; }
    }

    /// <summary>Form model for editing a ticket the user already raised.</summary>
    public class TicketEditVM
    {
        public int Id { get; set; }

        [Display(Name = "Title"), Required, StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Description"), Required]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public TicketType TicketType { get; set; }

        [Display(Name = "Attachments")]
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();

        public SelectList TicketTypeList { get; set; }

        public List<TicketAttachment> ExistingAttachments { get; set; } = new List<TicketAttachment>();
    }

    /// <summary>One row on the ticket list.</summary>
    public class TicketListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TicketType TicketType { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime RaiseDate { get; set; }
        public string RaisedByName { get; set; }
        public string AssignedToName { get; set; }
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    /// <summary>The ticket list, its filters and the status summary tiles.</summary>
    public class TicketListVM
    {
        public List<TicketListItemVM> Tickets { get; set; } = new List<TicketListItemVM>();

        [Display(Name = "Status")]
        public TicketStatus? StatusFilter { get; set; }

        [Display(Name = "Type")]
        public TicketType? TypeFilter { get; set; }

        [Display(Name = "Search")]
        public string SearchText { get; set; }

        [Display(Name = "Only my tickets")]
        public bool MineOnly { get; set; }

        public SelectList StatusList { get; set; }
        public SelectList TypeList { get; set; }

        public int PendingCount { get; set; }
        public int WorkingCount { get; set; }
        public int SolvedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    /// <summary>Ticket detail: the request, its discussion and its files.</summary>
    public class TicketDetailsVM
    {
        public Ticket Ticket { get; set; }

        public string RaisedByName { get; set; }
        public string AssignedToName { get; set; }

        public List<TicketCommentVM> Comments { get; set; } = new List<TicketCommentVM>();
        public List<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

        [Display(Name = "Add a comment")]
        public string NewComment { get; set; }

        [Display(Name = "Status")]
        public TicketStatus NewStatus { get; set; }

        [Display(Name = "Note")]
        public string StatusNote { get; set; }

        public SelectList StatusList { get; set; }

        /// <summary>True when the signed-in user may change status or assign.</summary>
        public bool CanManage { get; set; }
    }

    /// <summary>A discussion entry with its author resolved to a display name.</summary>
    public class TicketCommentVM
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsSystemNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; }
    }
}
