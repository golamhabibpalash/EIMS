using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Entities
{
    /// <summary>
    /// A student (or alumni) testimonial managed from the admin area. Only rows that are both
    /// <see cref="Status"/> = true AND <see cref="IsPublished"/> = true are shown on the public
    /// (published) listing; drafts/inactive ones stay admin-only. Content is plain text and is
    /// HTML-encoded when rendered, so it cannot carry script.
    /// </summary>
    public class Testimonial : CommonProps
    {
        /// <summary>Optional link to a registered student; null for an external/alumni voice.</summary>
        [Display(Name = "Student")]
        public int? StudentId { get; set; }

        [Required, StringLength(150), Display(Name = "Display Name")]
        public string StudentName { get; set; }

        [StringLength(150), Display(Name = "Designation / Class / Batch")]
        public string Designation { get; set; }

        [Required, Display(Name = "Testimonial")]
        public string Content { get; set; }

        /// <summary>Optional photo filename under wwwroot/Images/Testimonial; falls back to the student photo.</summary>
        [Display(Name = "Photo")]
        public string Photo { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool Status { get; set; } = true;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = false;

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
    }
}
