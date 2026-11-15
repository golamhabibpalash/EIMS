using System.Collections.Generic;
using System.Linq;
using SMS.Entities;
using SMS.Entities.AdditionalModels.Finance;
using SMS_App.Utilities.Export;
using Xunit;

namespace SMS.Tests.StudentPayments
{
    public class PreviousDueExportTests
    {
        private static DuePayment Due(int roll, string name, double payable, double paid, string section = null, bool active = true) => new()
        {
            Student = new Student
            {
                ClassRoll = roll,
                Name = name,
                UniqueId = "26" + roll,
                Status = active,
                AcademicSection = section == null ? null : new AcademicSection { Name = section }
            },
            PayableAmount = payable,
            PaidAmount = paid,
            TotalDue = payable - paid
        };

        private static readonly List<DuePayment> Rows = new()
        {
            Due(3, "Charlie", 5000, 1000, "B"),
            Due(1, "alice", 3000, 2500, "A"),
            Due(2, "Bob", 9000, 1000, "A", active: false),
        };

        [Fact]
        public void Default_sort_is_highest_due_first()
        {
            var sorted = PreviousDueExport.Sort(Rows, null, null);
            Assert.Equal(new[] { 2, 3, 1 }, sorted.Select(r => r.Student.ClassRoll));
        }

        [Fact]
        public void Text_columns_sort_case_insensitively()
        {
            var sorted = PreviousDueExport.Sort(Rows, "name", "asc");
            Assert.Equal(new[] { "alice", "Bob", "Charlie" }, sorted.Select(r => r.Student.Name));
        }

        [Fact]
        public void Numeric_columns_sort_by_value_in_requested_direction()
        {
            var sorted = PreviousDueExport.Sort(Rows, "paid", "desc");
            Assert.Equal(2500, sorted.First().PaidAmount);
            Assert.Equal(new[] { 2, 3 }, sorted.Skip(1).Select(r => r.Student.ClassRoll)); // tie broken by roll
        }

        [Fact]
        public void Unknown_sort_key_falls_back_to_default()
        {
            var sorted = PreviousDueExport.Sort(Rows, "nonsense", "asc");
            Assert.Equal(new[] { 1, 3, 2 }, sorted.Select(r => r.Student.ClassRoll)); // due, ascending
        }

        [Fact]
        public void Search_matches_any_column_ignoring_case()
        {
            Assert.Equal(new[] { "Bob" }, PreviousDueExport.Search(Rows, "BOB").Select(r => r.Student.Name));
            Assert.Equal(new[] { 1 }, PreviousDueExport.Search(Rows, "500").Select(r => r.Student.ClassRoll)); // due 500
            Assert.Single(PreviousDueExport.Search(Rows, "inactive"));
            Assert.Equal(3, PreviousDueExport.Search(Rows, "  ").Count());
        }

        [Fact]
        public void Rows_follow_header_order_and_format_amounts()
        {
            var headers = PreviousDueExport.Headers;
            var row = PreviousDueExport.Rows(new[] { Due(7, "Dina", 12500.5, 500) }).Single();

            Assert.Equal(headers.Count, row.Count);
            Assert.Equal("7", row[headers.ToList().IndexOf("Class Roll")]);
            Assert.Equal("N/A", row[headers.ToList().IndexOf("Section")]);
            Assert.Equal("12,000.5", row[headers.ToList().IndexOf("Due")]);
            Assert.Equal(3, PreviousDueExport.AmountColumnIndexes.Count);
        }
    }
}
