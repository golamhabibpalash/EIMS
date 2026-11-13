using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// An application for one child. Kept separate from <see cref="Student"/>: a Student row is
    /// only created at enrolment, once every admission condition is met.
    /// </summary>
    public class AdmissionApplication : CommonProps
    {
        /// <summary>Human-readable reference, assigned on first submission (null while a draft).</summary>
        [StringLength(30)]
        public string ApplicationNo { get; set; }

        public int AdmissionSessionId { get; set; }
        public AdmissionSession AdmissionSession { get; set; }

        public int AdmissionProgramId { get; set; }
        public AdmissionProgram AdmissionProgram { get; set; }

        public int GuardianAccountId { get; set; }
        public AdmissionGuardianAccount GuardianAccount { get; set; }

        public AdmissionApplicationStatus Status { get; set; } = AdmissionApplicationStatus.Draft;

        /// <summary>Furthest form step reached (1-5), so a draft resumes where it was left.</summary>
        public int CurrentStep { get; set; } = 1;

        #region Student
        [StringLength(150)] public string Name { get; set; }
        [StringLength(150)] public string NameBangla { get; set; }
        public DateTime? DOB { get; set; }
        public int? GenderId { get; set; }
        public Gender Gender { get; set; }
        public int? ReligionId { get; set; }
        public Religion Religion { get; set; }
        public int? NationalityId { get; set; }
        public Nationality Nationality { get; set; }
        public int? BloodGroupId { get; set; }
        public BloodGroup BloodGroup { get; set; }
        [StringLength(17)] public string BirthCertificateNo { get; set; }
        /// <summary>Stored file name of the student photo (private storage).</summary>
        [StringLength(200)] public string PhotoFileName { get; set; }
        #endregion

        #region Guardian
        [StringLength(150)] public string FatherName { get; set; }
        [StringLength(150)] public string FatherNameBangla { get; set; }
        [StringLength(100)] public string FatherOccupation { get; set; }
        [StringLength(11)] public string FatherPhone { get; set; }
        [StringLength(17)] public string FatherNID { get; set; }
        [StringLength(150)] public string MotherName { get; set; }
        [StringLength(150)] public string MotherNameBangla { get; set; }
        [StringLength(100)] public string MotherOccupation { get; set; }
        [StringLength(11)] public string MotherPhone { get; set; }
        [StringLength(17)] public string MotherNID { get; set; }
        /// <summary>Contact mobile used for the student record and SMS updates.</summary>
        [StringLength(11)] public string GuardianPhone { get; set; }
        [StringLength(150)] public string Email { get; set; }

        [StringLength(200)] public string PresentAddressArea { get; set; }
        [StringLength(100)] public string PresentAddressPO { get; set; }
        public int? PresentDivisionId { get; set; }
        public Division PresentDivision { get; set; }
        public int? PresentDistrictId { get; set; }
        public District PresentDistrict { get; set; }
        public int? PresentUpazilaId { get; set; }
        public Upazila PresentUpazila { get; set; }

        [StringLength(200)] public string PermanentAddressArea { get; set; }
        [StringLength(100)] public string PermanentAddressPO { get; set; }
        public int? PermanentDivisionId { get; set; }
        public Division PermanentDivision { get; set; }
        public int? PermanentDistrictId { get; set; }
        public District PermanentDistrict { get; set; }
        public int? PermanentUpazilaId { get; set; }
        public Upazila PermanentUpazila { get; set; }
        #endregion

        #region Previous academics
        [StringLength(200)] public string PreviousSchool { get; set; }
        [StringLength(50)] public string PreviousClass { get; set; }
        [StringLength(100)] public string PreviousResult { get; set; }
        public decimal? PreviousGpa { get; set; }
        #endregion

        #region Submission & eligibility
        public DateTime? SubmittedAt { get; set; }
        public DateTime? LastSubmittedAt { get; set; }
        public bool EligibilityAcknowledged { get; set; }
        [StringLength(30)] public string PolicyVersionAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        /// <summary>Eligibility criteria not met at submission (one per line). Empty = none.</summary>
        [StringLength(2000)] public string EligibilityWarnings { get; set; }
        public bool EligibilityExceptionApproved { get; set; }
        [StringLength(1000)] public string EligibilityExceptionReason { get; set; }
        #endregion

        #region Review & decision
        [StringLength(2000)] public string CorrectionReason { get; set; }
        public DateTime? CorrectionDeadline { get; set; }
        [StringLength(2000)] public string DecisionReason { get; set; }
        [StringLength(450)] public string DecisionBy { get; set; }
        public DateTime? DecisionAt { get; set; }
        public DateTime? OfferExpiresAt { get; set; }
        #endregion

        #region Payment & enrolment
        public decimal AmountDue { get; set; }
        /// <summary>Sum of verified payments only.</summary>
        public decimal AmountPaid { get; set; }
        /// <summary>Set once, at enrolment. Unique - guards against a retried enrolment creating a second student.</summary>
        public int? StudentId { get; set; }
        public Student Student { get; set; }
        public DateTime? EnrolledAt { get; set; }
        [StringLength(450)] public string EnrolledBy { get; set; }
        [StringLength(1000)] public string EnrolmentExceptionReason { get; set; }
        #endregion

        public List<AdmissionApplicationDocument> Documents { get; set; } = new();
        public List<AdmissionPayment> Payments { get; set; } = new();
        public List<AdmissionApplicationHistory> History { get; set; } = new();
    }
}
