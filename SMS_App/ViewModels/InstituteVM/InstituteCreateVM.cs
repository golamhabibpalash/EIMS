using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.InstituteVM;

/// <summary>
/// The first-time "Institute Profile" form. Only the institute name is required; everything
/// else can be filled in later from the Institute Profile page.
/// </summary>
public class InstituteCreateVM : IValidatableObject
{
    [Required(ErrorMessage = "Please enter the institute name.")]
    [StringLength(200)]
    [Display(Name = "Institute Name")]
    public string Name { get; set; }

    [StringLength(50)]
    [Display(Name = "Short Name")]
    public string ShortName { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^\d+$", ErrorMessage = "EIIN must contain digits only.")]
    [Display(Name = "EIIN")]
    public string EIIN { get; set; }

    [StringLength(100)]
    [Display(Name = "Branch Name")]
    public string BranchName { get; set; }

    [StringLength(200)]
    [Display(Name = "Slogan")]
    public string Slogan { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^\+?[0-9][0-9\s-]{5,18}$", ErrorMessage = "Enter a valid phone number, e.g. 01712345678.")]
    [Display(Name = "Phone (1)")]
    public string Phone1 { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^\+?[0-9][0-9\s-]{5,18}$", ErrorMessage = "Enter a valid phone number, e.g. 01712345678.")]
    [Display(Name = "Phone (2)")]
    public string Phone2 { get; set; }

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Address")]
    public string Address { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Opening Time")]
    public DateTime StartingTime { get; set; } = DateTime.Today.AddHours(8);

    [DataType(DataType.Time)]
    [Display(Name = "Late After")]
    public DateTime LateTime { get; set; } = DateTime.Today.AddHours(8).AddMinutes(15);

    [DataType(DataType.Time)]
    [Display(Name = "Closing Time")]
    public DateTime ClosingTime { get; set; } = DateTime.Today.AddHours(14);

    [Display(Name = "Logo")]
    public IFormFile LogoFile { get; set; }

    [Display(Name = "Banner")]
    public IFormFile BannerFile { get; set; }

    [Display(Name = "Title Icon")]
    public IFormFile FavIconFile { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Only the time of day matters; the date part comes from the binder.
        if (ClosingTime.TimeOfDay <= StartingTime.TimeOfDay)
        {
            yield return new ValidationResult("Closing time must be after the opening time.", new[] { nameof(ClosingTime) });
        }
        if (LateTime.TimeOfDay < StartingTime.TimeOfDay || LateTime.TimeOfDay > ClosingTime.TimeOfDay)
        {
            yield return new ValidationResult("Late time must be between the opening and closing time.", new[] { nameof(LateTime) });
        }
    }
}
