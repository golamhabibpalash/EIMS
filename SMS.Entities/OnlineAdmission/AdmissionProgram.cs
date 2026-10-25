using SMS.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>A class offered in an admission session, with its seats, eligibility rules and fee.</summary>
    public class AdmissionProgram : CommonProps
    {
        public int AdmissionSessionId { get; set; }
        public AdmissionSession AdmissionSession { get; set; }

        [Display(Name = "Class")]
        public int AcademicClassId { get; set; }
        public AcademicClass AcademicClass { get; set; }

        /// <summary>0 = unlimited.</summary>
        [Display(Name = "Seats")]
        public int SeatCapacity { get; set; }

        [Display(Name = "When seats are full")]
        public SeatCapacityPolicy CapacityPolicy { get; set; } = SeatCapacityPolicy.Waitlist;

        [Display(Name = "Minimum age (years)")]
        public int? MinAgeYears { get; set; }

        [Display(Name = "Maximum age (years)")]
        public int? MaxAgeYears { get; set; }

        /// <summary>Age is measured on this date; defaults to the application deadline when empty.</summary>
        [Display(Name = "Age calculated on")]
        public DateTime? AgeCalculationDate { get; set; }

        [Display(Name = "Previous result required")]
        public bool RequirePreviousResult { get; set; }

        [Display(Name = "Minimum previous GPA")]
        public decimal? MinPreviousGpa { get; set; }

        [Display(Name = "Eligibility notes")]
        public string EligibilityNotes { get; set; }

        [Display(Name = "Admission fee (BDT)")]
        public decimal AdmissionFeeAmount { get; set; }

        /// <summary>Fee head used to post the verified admission fee into the student's ledger at enrolment.</summary>
        [Display(Name = "Post fee to fee head")]
        public int? AdmissionFeeHeadId { get; set; }
        public StudentFeeHead AdmissionFeeHead { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
