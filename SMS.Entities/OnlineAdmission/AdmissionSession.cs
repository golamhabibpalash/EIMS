using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// One admission round (e.g. "Admission 2027"). Carries its public notice, eligibility,
    /// policy/guidelines, application window and payment rules. Enrolled students are placed in
    /// <see cref="AcademicSessionId"/>.
    /// </summary>
    public class AdmissionSession : CommonProps
    {
        [Required, StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Academic session")]
        public int AcademicSessionId { get; set; }
        public AcademicSession AcademicSession { get; set; }

        public AdmissionSessionStatus Status { get; set; } = AdmissionSessionStatus.Draft;

        /// <summary>Optional scheduled publication: a published session stays hidden until this date.</summary>
        [Display(Name = "Publish from")]
        public DateTime? PublishFrom { get; set; }

        [Display(Name = "Notice summary"), StringLength(500)]
        public string NoticeSummary { get; set; }

        [Display(Name = "Notice details")]
        public string NoticeDetails { get; set; }

        [Display(Name = "Application start date")]
        public DateTime ApplicationStartDate { get; set; }

        [Display(Name = "Application deadline")]
        public DateTime ApplicationEndDate { get; set; }

        [Display(Name = "Show \"Closing soon\" this many days before the deadline")]
        public int ClosingSoonDays { get; set; } = 3;

        [Display(Name = "Important dates (exam, interview, results...)")]
        public string ImportantDates { get; set; }

        [Display(Name = "Admission eligibility")]
        public string EligibilityContent { get; set; }

        [Display(Name = "Admission policy / guidelines")]
        public string PolicyContent { get; set; }

        /// <summary>Recorded against each application at submission, so it is known which wording the guardian accepted.</summary>
        [Display(Name = "Policy version"), StringLength(30)]
        public string PolicyVersion { get; set; } = "1.0";

        [Display(Name = "Fee payable")]
        public AdmissionPaymentStage PaymentStage { get; set; } = AdmissionPaymentStage.AfterApproval;

        [Display(Name = "Allow online payment")]
        public bool AllowOnlinePayment { get; set; }

        [Display(Name = "Allow offline payment")]
        public bool AllowOfflinePayment { get; set; } = true;

        [Display(Name = "Allow partial payment")]
        public bool AllowPartialPayment { get; set; }

        [Display(Name = "Offline payment instructions")]
        public string OfflinePaymentInstructions { get; set; }

        /// <summary>Days an approved applicant has to pay. Empty = no offer deadline.</summary>
        [Display(Name = "Offer valid for (days)")]
        public int? OfferValidityDays { get; set; }

        public List<AdmissionProgram> Programs { get; set; } = new();
        public List<AdmissionDocumentRequirement> DocumentRequirements { get; set; } = new();
    }
}
