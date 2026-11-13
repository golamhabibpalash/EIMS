using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.OnlineAdmission
{
    public class AdmissionNoticeCardVM
    {
        public AdmissionSession Session { get; set; }
        public AdmissionWindowState State { get; set; }
    }

    public class AdmissionNoticeListVM
    {
        public List<AdmissionNoticeCardVM> Notices { get; set; } = new();
    }

    /// <summary>The session's landing page (also used for the Eligibility and Guidelines pages).</summary>
    public class AdmissionLandingVM
    {
        public AdmissionSession Session { get; set; }
        public AdmissionWindowState State { get; set; }
        public Dictionary<int, AdmissionSeatUsage> SeatUsage { get; set; } = new();
        public bool IsSignedIn { get; set; }
    }

    public class AdmissionTrackVM
    {
        [Required(ErrorMessage = "Enter the application number.")]
        [Display(Name = "Application number / আবেদন নম্বর")]
        public string ApplicationNo { get; set; }

        [Required(ErrorMessage = "Enter the guardian's mobile number.")]
        [Display(Name = "Guardian's mobile / অভিভাবকের মোবাইল")]
        public string MobileNumber { get; set; }

        public bool Searched { get; set; }
        public AdmissionApplication Result { get; set; }
    }

    #region Guardian account

    public class GuardianLoginVM
    {
        [Required(ErrorMessage = "Enter your mobile number.")]
        [Display(Name = "Mobile number / মোবাইল নম্বর")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Enter your password."), DataType(DataType.Password)]
        [Display(Name = "Password / পাসওয়ার্ড")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class GuardianRegisterVM
    {
        [Required(ErrorMessage = "Enter your full name."), StringLength(150)]
        [Display(Name = "Your full name / আপনার পূর্ণ নাম")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Enter your mobile number.")]
        [Display(Name = "Mobile number / মোবাইল নম্বর")]
        public string MobileNumber { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email address, or leave it empty."), StringLength(150)]
        [Display(Name = "Email (optional)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Choose a password."), StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters."), DataType(DataType.Password)]
        [Display(Name = "Password / পাসওয়ার্ড")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Type the password again."), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class GuardianOtpVM
    {
        [Required(ErrorMessage = "Enter the 6-digit code."), RegularExpression("^\\s*\\d{6}\\s*$", ErrorMessage = "The code has 6 digits.")]
        [Display(Name = "Code from SMS / এসএমএস কোড")]
        public string Code { get; set; }

        public string MaskedMobile { get; set; }
    }

    public class GuardianForgotPasswordVM
    {
        [Required(ErrorMessage = "Enter your mobile number.")]
        [Display(Name = "Mobile number / মোবাইল নম্বর")]
        public string MobileNumber { get; set; }
    }

    public class GuardianResetPasswordVM
    {
        [Required]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Enter the 6-digit code."), RegularExpression("^\\s*\\d{6}\\s*$", ErrorMessage = "The code has 6 digits.")]
        [Display(Name = "Code from SMS")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Choose a new password."), StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters."), DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "The two passwords do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }

    #endregion Guardian account

    #region Guardian portal

    public class GuardianDashboardVM
    {
        public AdmissionGuardianAccount Guardian { get; set; }
        public List<AdmissionApplication> Applications { get; set; } = new();
        public List<AdmissionNoticeCardVM> OpenSessions { get; set; } = new();
    }

    /// <summary>One step of the multi-step application form.</summary>
    public class ApplicationFormVM
    {
        public int Step { get; set; }
        /// <summary>Bound form fields. The manager copies only the current step's fields, so extra posted values are ignored.</summary>
        public AdmissionApplication Form { get; set; } = new();
        public AdmissionApplication Application { get; set; }
        public AdmissionSession Session { get; set; }
        public AdmissionProgram Program { get; set; }

        public List<string> MissingItems { get; set; } = new();
        public List<string> EligibilityWarnings { get; set; } = new();

        public List<SelectListItem> GenderList { get; set; } = new();
        public List<SelectListItem> ReligionList { get; set; } = new();
        public List<SelectListItem> NationalityList { get; set; } = new();
        public List<SelectListItem> BloodGroupList { get; set; } = new();
        public List<SelectListItem> DivisionList { get; set; } = new();
        /// <summary>All districts/upazilas as {id, name, parentId} for client-side cascading.</summary>
        public string DistrictsJson { get; set; } = "[]";
        public string UpazilasJson { get; set; } = "[]";

        public bool EligibilityAcknowledged { get; set; }
        public bool PolicyAcknowledged { get; set; }
    }

    public class OfflinePaymentVM
    {
        public int ApplicationId { get; set; }
        public AdmissionApplication Application { get; set; }
        public decimal Outstanding { get; set; }
        public decimal PendingAmount { get; set; }
        public string Blocker { get; set; }

        [Required(ErrorMessage = "Choose how you paid.")]
        [Display(Name = "Paid through / পরিশোধের মাধ্যম")]
        public string Channel { get; set; }

        [Required(ErrorMessage = "Enter the amount paid."), Range(1, 10000000, ErrorMessage = "Enter the amount paid.")]
        [Display(Name = "Amount (BDT) / টাকার পরিমাণ")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Enter the payment date."), DataType(DataType.Date)]
        [Display(Name = "Payment date / পরিশোধের তারিখ")]
        public DateTime PaidOn { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Enter the transaction / reference number."), StringLength(100)]
        [Display(Name = "Transaction / reference no. / ট্রানজেকশন নম্বর")]
        public string TransactionReference { get; set; }

        [Display(Name = "Proof of payment (optional photo or PDF)")]
        public IFormFile Proof { get; set; }

        public static readonly string[] Channels = { "bKash", "Nagad", "Rocket", "Bank deposit", "Other" };
    }

    #endregion Guardian portal
}
