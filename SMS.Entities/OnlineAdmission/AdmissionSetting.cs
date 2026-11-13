using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// Institute-wide Online Admission settings (single row). Each deployment is one school,
    /// so this is the school's own configuration.
    /// </summary>
    public class AdmissionSetting : CommonProps
    {
        [Display(Name = "Helpline phone"), StringLength(20)]
        public string HelplinePhone { get; set; }

        [Display(Name = "WhatsApp number"), StringLength(20)]
        public string WhatsAppNumber { get; set; }

        [Display(Name = "Support hours"), StringLength(200)]
        public string SupportHours { get; set; }

        [Display(Name = "Support message"), StringLength(500)]
        public string SupportMessage { get; set; }

        [Display(Name = "Application number prefix"), Required, StringLength(10)]
        public string ApplicationNoPrefix { get; set; } = "ADM";

        /// <summary>Guardians must confirm their mobile with an SMS code before they can apply.</summary>
        [Display(Name = "Verify guardian mobile by SMS code")]
        public bool RequireOtpVerification { get; set; }

        [Display(Name = "Send SMS updates to guardians")]
        public bool SendSmsNotifications { get; set; } = true;

        [Display(Name = "Max upload size (KB)")]
        public int MaxUploadSizeKb { get; set; } = 2048;

        /// <summary>Optional school-specific text appended to the built-in first-time applicant manual.</summary>
        [Display(Name = "Additional user manual notes")]
        public string UserManualNotes { get; set; }
    }
}
