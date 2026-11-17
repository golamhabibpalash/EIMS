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
        /// <summary>Student or employee name, when the account is linked to one.</summary>
        public string Name { get; set; }
        /// <summary>Class roll, for a student account.</summary>
        public int? ClassRoll { get; set; }
        /// <summary>Employee record Id, for an employee account.</summary>
        public int? EmployeeId { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsLockedOut { get; set; }
    }

    public class UserPasswordIndexVM
    {
        public static readonly int[] PageSizes = { 25, 50, 100 };

        public string Search { get; set; }
        /// <summary>"" = all, "s" = students, "e" = employees, "o" = other.</summary>
        public string Type { get; set; }
        public List<UserPasswordListItemVM> Users { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
        public int FirstRow => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastRow => System.Math.Min(Page * PageSize, TotalCount);
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

        // Where to return to on the list.
        public string Search { get; set; }
        public string Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
