using SMS.Entities;
using SMS.Entities.AdditionalModels.Finance;
using System.Collections.Generic;

namespace SMS_App.ViewModels.PaymentVM
{
    /// <summary>
    /// Model for <c>Views/StudentPayments/_DueListResults.cshtml</c>, the summary cards + sortable
    /// table shared by the "Due Payment" and "Due Payment (Previous)" pages.
    /// </summary>
    public class DueListResultsVM
    {
        public List<DuePayment> Rows { get; set; } = new();
        public Institute Institute { get; set; }
        /// <summary>Report name under the institute name, e.g. "Due Payments (current session)".</summary>
        public string ReportTitle { get; set; }
        /// <summary>Filter lines shown on the right of the report heading (class, period, status...).</summary>
        public List<string> ScopeLines { get; set; } = new();
        public bool IsFromPost { get; set; }
        public bool DueOnly { get; set; }
        /// <summary>Shown before the first search, under "Choose filters and select Get list".</summary>
        public string EmptyHint { get; set; }
    }
}
