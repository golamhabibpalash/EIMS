using System.ComponentModel.DataAnnotations;
using SMS.Entities.Enums;

namespace SMS_App.ViewModels.InstituteVM;

/// <summary>
/// Add/Edit form for a single institute link. Used by <c>InstituteLinksController</c>.
/// Note there is deliberately no InstituteId here: the system holds exactly one institute,
/// so the controller resolves it server-side rather than trusting a value from the client.
/// </summary>
public class InstituteLinkFormVM
{
    /// <summary>0 for a new link; the existing link's id when editing.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a title for the link.")]
    [StringLength(100)]
    [Display(Name = "Link Title")]
    public string LinkTitle { get; set; }

    [Display(Name = "Link Type")]
    public InstituteLinkType LinkType { get; set; } = InstituteLinkType.Website;

    // The scheme (http/https) is normalised and validated server-side in InstituteLinksController
    // so users can type "example.com"; validating it here would reject that before we can fix it up.
    [Required(ErrorMessage = "Please enter the URL.")]
    [StringLength(2048)]
    [Display(Name = "URL")]
    public string Url { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order cannot be negative.")]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
