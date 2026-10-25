using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.OnlineAdmission
{
    public class AdmissionSessionListVM
    {
        public List<AdmissionSession> Sessions { get; set; } = new();
        public bool IncludeArchived { get; set; }
        public DateTime Now { get; set; } = DateTime.Now;
    }

    public class AdmissionSessionEditVM
    {
        public AdmissionSession Session { get; set; } = new()
        {
            ApplicationStartDate = DateTime.Today,
            ApplicationEndDate = DateTime.Today.AddDays(30)
        };
        public List<SelectListItem> AcademicSessionList { get; set; } = new();
    }

    public class AdmissionSessionDetailsVM
    {
        public AdmissionSession Session { get; set; }
        public AdmissionWindowState State { get; set; }
        public Dictionary<int, AdmissionSeatUsage> SeatUsage { get; set; } = new();
        public string PublicUrl { get; set; }
    }

    public class AdmissionProgramEditVM
    {
        public AdmissionProgram Program { get; set; } = new() { IsActive = true };
        public AdmissionSession Session { get; set; }
        public List<SelectListItem> ClassList { get; set; } = new();
        public List<SelectListItem> FeeHeadList { get; set; } = new();
    }

    public class AdmissionDocumentRequirementEditVM
    {
        public AdmissionDocumentRequirement Requirement { get; set; } = new();
        public AdmissionSession Session { get; set; }
    }

    public class AdmissionApplicationListVM
    {
        public AdmissionApplicationFilter Filter { get; set; } = new();
        public List<AdmissionApplication> Applications { get; set; } = new();
        public List<SelectListItem> SessionList { get; set; } = new();
        public List<SelectListItem> ProgramList { get; set; } = new();
        public List<SelectListItem> StatusList { get; set; } = new();
        public Dictionary<AdmissionApplicationStatus, int> StatusCounts { get; set; } = new();
    }

    public class AdmissionDashboardVM
    {
        public int? AdmissionSessionId { get; set; }
        public List<SelectListItem> SessionList { get; set; } = new();
        public Dictionary<AdmissionApplicationStatus, int> StatusCounts { get; set; } = new();
        public List<AdmissionProgram> Programs { get; set; } = new();
        public Dictionary<int, AdmissionSeatUsage> SeatUsage { get; set; } = new();
        public List<AdmissionPayment> Payments { get; set; } = new();

        public int Count(params AdmissionApplicationStatus[] statuses)
        {
            var total = 0;
            foreach (var status in statuses)
                total += StatusCounts.TryGetValue(status, out var n) ? n : 0;
            return total;
        }
    }

    public class AdmissionApplicationDetailsVM
    {
        public AdmissionApplication Application { get; set; }
        public List<string> MissingItems { get; set; } = new();
        public decimal Outstanding { get; set; }
        public decimal PendingAmount { get; set; }
        public string EnrolmentBlocker { get; set; }
        public bool EnrolmentNeedsException { get; set; }
        public int SeatsHeld { get; set; }
        /// <summary>Fields changed between the last two submissions (label, before, after).</summary>
        public List<(string Field, string Before, string After)> LastCorrectionChanges { get; set; } = new();
        public Dictionary<string, string> StaffNames { get; set; } = new();
    }

    public class AdmissionEnrolVM
    {
        public AdmissionApplication Application { get; set; }
        public List<SelectListItem> SectionList { get; set; } = new();
        public bool NeedsException { get; set; }
        public string Blocker { get; set; }

        [Range(1, 999, ErrorMessage = "The roll must be between 1 and 999.")]
        [Display(Name = "Class roll (1-999)")]
        public int RollNo { get; set; }

        [Display(Name = "Section")]
        public int? AcademicSectionId { get; set; }

        [DataType(DataType.Date), Display(Name = "Admission date")]
        public DateTime AdmissionDate { get; set; } = DateTime.Today;

        [Display(Name = "Reason for enrolling before full payment")]
        public string ExceptionReason { get; set; }
    }

    public class AdmissionPaymentListVM
    {
        public AdmissionPaymentStatus? Status { get; set; }
        public int? AdmissionSessionId { get; set; }
        public List<SelectListItem> SessionList { get; set; } = new();
        public List<AdmissionPayment> Payments { get; set; } = new();
    }
}
