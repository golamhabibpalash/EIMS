using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS.Entities.AdditionalModels.Finance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SMS_App.ViewModels.PaymentVM
{
    /// <summary>
    /// "Due Payment" (one session) page: the filters posted by the form / sent to the export, and
    /// what the view needs to redisplay them.
    /// </summary>
    public class DuePaymentVM
    {
        #region Filters
        [Display(Name = "Session")]
        public int AcademicSessionId { get; set; }
        [Display(Name = "Class")]
        public int AcademicClassId { get; set; }
        [Display(Name = "Section")]
        public int AcademicSectionId { get; set; }
        /// <summary>"1" = active, "0" = inactive, "2" = all.</summary>
        public string Status { get; set; } = "2";
        /// <summary>"all", "residential" or "nonResidential".</summary>
        public string Category { get; set; } = "all";

        /// <summary>
        /// Optional fee period. When set, only fees falling due in the months this range touches
        /// (and payments made against them) count. Both empty = the whole session.
        /// </summary>
        [Display(Name = "From"), DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }
        [Display(Name = "To"), DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        /// <summary>When true, students with nothing left to pay are left out.</summary>
        public bool DueOnly { get; set; } = true;
        #endregion Filters

        #region Display
        public AcademicSession AcademicSession { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public AcademicSection AcademicSection { get; set; }
        public Institute Institute { get; set; }
        /// <summary>Calendar year of the selected session; bounds the date pickers.</summary>
        public int SessionYear { get; set; }
        public List<DuePayment> DuePayments { get; set; } = new();

        public List<SelectListItem> AcademicSessionList { get; set; } = new();
        public List<SelectListItem> AcademicClassList { get; set; } = new();
        public List<SelectListItem> AcademicSectionList { get; set; } = new();
        #endregion Display

        public bool HasPeriod => FromDate.HasValue || ToDate.HasValue;

        /// <summary>Human-readable filter lines for the report heading and the PDF subtitle.</summary>
        public List<string> DescribeFilters()
        {
            var lines = new List<string>
            {
                $"Session {AcademicSession?.Name} · {AcademicClass?.Name ?? "All classes"} · "
                    + (AcademicSection != null ? $"Section {AcademicSection.Name}" : "All sections"),
                PeriodLabel(FromDate, ToDate)
            };
            var status = Status switch { "1" => "Active students", "0" => "Inactive students", _ => "Active and inactive students" };
            var category = Category switch { "residential" => ", residential", "nonResidential" => ", non-residential", _ => "" };
            lines.Add(status + category + (DueOnly ? ", with a due" : ""));
            return lines;
        }

        public static string PeriodLabel(DateTime? from, DateTime? to)
        {
            if (from == null && to == null) return "Whole session";
            static string Month(DateTime d) => d.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            if (from != null && to != null)
                return Month(from.Value) == Month(to.Value)
                    ? $"Fees for {Month(from.Value)}"
                    : $"Fees for {Month(from.Value)} to {Month(to.Value)}";
            return from != null ? $"Fees from {Month(from.Value)}" : $"Fees up to {Month(to.Value)}";
        }
    }
}
