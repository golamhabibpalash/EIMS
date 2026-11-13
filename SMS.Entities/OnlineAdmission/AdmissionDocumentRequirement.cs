using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>A document the guardian must (or may) upload for a session.</summary>
    public class AdmissionDocumentRequirement : CommonProps
    {
        public int AdmissionSessionId { get; set; }
        public AdmissionSession AdmissionSession { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Mandatory")]
        public bool IsMandatory { get; set; } = true;

        [Display(Name = "Allowed file types"), Required, StringLength(100)]
        public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.pdf";

        [Display(Name = "Max size (KB)")]
        public int MaxSizeKb { get; set; } = 1024;

        [Display(Name = "Order")]
        public int SortOrder { get; set; }
    }
}
