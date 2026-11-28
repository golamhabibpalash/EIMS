using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMS.Entities.AdditionalModels;
using SMS.Entities.Enums;

namespace SMS.Entities
{
    /// <summary>
    /// An external link that belongs to the institute: official website, a social-media page,
    /// or any other online resource. An institute may hold any number of these, including
    /// several of the same <see cref="LinkType"/> (e.g. two Facebook pages), so links live in
    /// their own table rather than as fixed columns on <see cref="Institute"/>.
    /// </summary>
    public class InstituteLink : CommonProps
    {
        public int InstituteId { get; set; }

        [ForeignKey(nameof(InstituteId))]
        public Institute Institute { get; set; }

        [Required(ErrorMessage = "Please enter a title for the link.")]
        [StringLength(100)]
        [Display(Name = "Link Title")]
        public string LinkTitle { get; set; }

        [Display(Name = "Link Type")]
        public InstituteLinkType LinkType { get; set; }

        [Required(ErrorMessage = "Please enter the URL.")]
        [StringLength(2048)]
        [AbsoluteHttpUrl]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
