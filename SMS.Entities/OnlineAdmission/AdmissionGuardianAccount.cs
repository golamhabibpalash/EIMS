using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// A parent/guardian's admission-portal login. Deliberately separate from staff/student
    /// Identity users: guardians can apply for several children and never see the EIMS back office.
    /// </summary>
    public class AdmissionGuardianAccount : CommonProps
    {
        [Required, StringLength(150)]
        public string FullName { get; set; }

        /// <summary>Bangladeshi mobile, normalised to 11 digits (01XXXXXXXXX). Unique.</summary>
        [Required, StringLength(11)]
        public string MobileNumber { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsMobileVerified { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LockoutUntil { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // One active one-time code at a time; only its hash is stored.
        public string OtpHash { get; set; }
        [StringLength(20)]
        public string OtpPurpose { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public DateTime? OtpSentAt { get; set; }
        public int OtpAttempts { get; set; }
    }
}
