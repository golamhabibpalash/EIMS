using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.AdministrationVM
{
    /// <summary>One row of the Reset Password user search.</summary>
    public class UserPasswordListItemVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsLockedOut { get; set; }
    }

    public class UserPasswordIndexVM
    {
        public string Search { get; set; }
        public List<UserPasswordListItemVM> Users { get; set; } = new();
        public bool HasMore { get; set; }
    }

    public class UserPasswordResetVM
    {
        [Required]
        public string UserId { get; set; }

        // Display only (re-read from the database on post).
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }

        [Required(ErrorMessage = "Enter the new password.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be at least 5 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Type the new password again.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The two passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string Search { get; set; }
    }
}
