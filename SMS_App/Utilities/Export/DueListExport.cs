using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SMS.Entities.AdditionalModels.Finance;

namespace SMS_App.Utilities.Export
{
    /// <summary>
    /// Columns, sort keys and search for the due lists ("Due Payment" and "Due Payment (Previous)").
    /// Each page's grid and its CSV / Excel / PDF export both read this catalog, so a download always
    /// has the same columns, order and rows the user is looking at. Sort keys match the
    /// <c>data-sort-key</c> attributes in <c>Views/StudentPayments/_DueListResults.cshtml</c>.
    /// </summary>
    public static class DueListExport
    {
        public sealed class Column
        {
            public string Key { get; init; }
            public string Header { get; init; }
            public Func<DuePayment, string> Text { get; init; }
            /// <summary>Raw value used for sorting; numbers sort numerically, everything else as text.</summary>
            public Func<DuePayment, IComparable> SortValue { get; init; }
            public bool IsAmount { get; init; }
        }

        public const string DefaultSortKey = "due";
        public const string DefaultSortDirection = "desc";

        public static string Amount(double value) => value.ToString("#,##0.##", CultureInfo.InvariantCulture);

        private static string Phone(DuePayment d)
        {
            var phone = string.IsNullOrWhiteSpace(d.Student?.GuardianPhone) ? d.Student?.PhoneNo : d.Student.GuardianPhone;
            return string.IsNullOrWhiteSpace(phone) ? string.Empty : phone.Trim().PadLeft(11, '0');
        }

        public static readonly IReadOnlyList<Column> All = new List<Column>
        {
            new() { Key = "roll",     Header = "Class Roll",   Text = d => d.Student?.ClassRoll.ToString(),            SortValue = d => d.Student?.ClassRoll ?? 0 },
            new() { Key = "uniqueId", Header = "Unique ID",    Text = d => d.Student?.UniqueId,                        SortValue = d => d.Student?.UniqueId ?? string.Empty },
            new() { Key = "name",     Header = "Student Name", Text = d => d.Student?.Name,                            SortValue = d => d.Student?.Name ?? string.Empty },
            new() { Key = "class",    Header = "Class",        Text = d => d.Student?.AcademicClass?.Name ?? "N/A",    SortValue = d => d.Student?.AcademicClass?.Name ?? string.Empty },
            new() { Key = "section",  Header = "Section",      Text = d => d.Student?.AcademicSection?.Name ?? "N/A",  SortValue = d => d.Student?.AcademicSection?.Name ?? string.Empty },
            new() { Key = "phone",    Header = "Contact No",   Text = Phone,                                           SortValue = d => Phone(d) },
            new() { Key = "status",   Header = "Status",       Text = d => d.Student?.Status == true ? "Active" : "Inactive", SortValue = d => d.Student?.Status == true ? 0 : 1 },
            new() { Key = "payable",  Header = "Payable",      Text = d => Amount(d.PayableAmount), SortValue = d => d.PayableAmount, IsAmount = true },
            new() { Key = "paid",     Header = "Paid",         Text = d => Amount(d.PaidAmount),    SortValue = d => d.PaidAmount,    IsAmount = true },
            new() { Key = "due",      Header = "Due",          Text = d => Amount(d.TotalDue),      SortValue = d => d.TotalDue,      IsAmount = true },
        };

        /// <summary>
        /// Keeps rows where any column's text contains <paramref name="term"/> (case-insensitive),
        /// mirroring the grid's quick search so the export matches what is on screen.
        /// </summary>
        public static IEnumerable<DuePayment> Search(IEnumerable<DuePayment> rows, string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return rows;
            var t = term.Trim();
            return rows.Where(r => All.Any(c => (c.Text(r) ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Sorts by a column key (unknown keys fall back to the default: highest due first). Ties are
        /// broken by class roll so the order is stable between the page and the export.
        /// </summary>
        public static List<DuePayment> Sort(IEnumerable<DuePayment> rows, string sortKey, string direction)
        {
            var column = All.FirstOrDefault(c => string.Equals(c.Key, sortKey, StringComparison.OrdinalIgnoreCase))
                         ?? All.First(c => c.Key == DefaultSortKey);
            bool descending = string.Equals(direction ?? (column.Key == DefaultSortKey ? DefaultSortDirection : "asc"),
                                            "desc", StringComparison.OrdinalIgnoreCase);
            var comparer = Comparer<IComparable>.Create(CompareValues);

            var ordered = descending
                ? rows.OrderByDescending(column.SortValue, comparer)
                : rows.OrderBy(column.SortValue, comparer);
            return ordered.ThenBy(r => r.Student?.ClassRoll ?? 0).ToList();
        }

        private static int CompareValues(IComparable a, IComparable b)
        {
            if (a is string sa && b is string sb) return string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
            if (a == null) return b == null ? 0 : -1;
            return b == null ? 1 : a.CompareTo(b);
        }

        public static IReadOnlyList<string> Headers => All.Select(c => c.Header).ToList();

        public static IReadOnlyList<IReadOnlyList<string>> Rows(IEnumerable<DuePayment> rows) =>
            rows.Select(r => (IReadOnlyList<string>)All.Select(c => c.Text(r) ?? string.Empty).ToList()).ToList();

        /// <summary>Zero-based indexes of the amount columns, written as numbers in Excel.</summary>
        public static ISet<int> AmountColumnIndexes =>
            All.Select((c, i) => (c, i)).Where(x => x.c.IsAmount).Select(x => x.i).ToHashSet();
    }
}
