using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// A file attached to a ticket. The bytes live on disk under
    /// wwwroot/Uploads/Tickets; only the metadata is stored here.
    /// </summary>
    public class TicketAttachment : CommonProps
    {
        [Display(Name = "Ticket")]
        public int TicketId { get; set; }

        public Ticket Ticket { get; set; }

        /// <summary>Name on disk - generated, never the name the user supplied.</summary>
        [Display(Name = "Stored File"), Required, StringLength(260)]
        public string StoredFileName { get; set; }

        /// <summary>Name to show and to use when the file is downloaded.</summary>
        [Display(Name = "File Name"), Required, StringLength(260)]
        public string OriginalFileName { get; set; }

        [Display(Name = "Content Type"), StringLength(150)]
        public string ContentType { get; set; }

        [Display(Name = "Size (bytes)")]
        public long FileSize { get; set; }
    }
}
