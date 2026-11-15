using System;
using System.Collections.Generic;
using System.Linq;
using SMS.BLL.Managers;
using SMS.Entities;
using Xunit;

namespace SMS.Tests.StudentPayments
{
    public class DuePeriodRulesTests
    {
        private static readonly DateTime Today = new(2026, 11, 15);
        private static readonly AcademicSession Session = new() { Id = 1, Name = "2026" };
        private const int ClassId = 6;

        private static ClassFeeList Fee(int id, int headSl, double amount, bool residential = false, DateTime? start = null) => new()
        {
            Id = id,
            AcademicClassId = ClassId,
            AcademicSessionId = Session.Id,
            AcademicSession = Session,
            StudentFeeHeadId = 100 + id,
            StudentFeeHead = new StudentFeeHead { Id = 100 + id, SL = headSl, IsResidential = residential },
            SL = headSl,
            Amount = amount,
            StartDate = start ?? default
        };

        /// <summary>Admission (SL 0) 2000, Jan-Dec monthly (SL 1-12) 500 each, session fee (SL 13) 1500, exam fee (SL 14) 800.</summary>
        private static List<ClassFeeList> ClassFees()
        {
            var fees = new List<ClassFeeList> { Fee(1, 0, 2000), Fee(2, 13, 1500), Fee(3, 14, 800) };
            fees.AddRange(Enumerable.Range(1, 12).Select(m => Fee(10 + m, m, 500)));
            return fees;
        }

        private static Student Student(DateTime admitted, bool residential = false) => new()
        {
            Id = 1, UniqueId = "U1", AcademicClassId = ClassId, AdmissionDate = admitted, IsResidential = residential, Status = true
        };

        private static StudentPayment Payment(double total, params (int classFeeId, int headId, double amount)[] details) => new()
        {
            UniqueId = "U1",
            AcademicSessionId = Session.Id,
            TotalPayment = total,
            StudentPaymentDetails = details.Select(d => new StudentPaymentDetails { ClassFeeId = d.classFeeId, StudentFeeHeadId = d.headId, PaidAmount = d.amount }).ToList()
        };

        private static IReadOnlyList<DuePeriodRules.ScheduledFee> Schedule(Student st, params StudentFeeAllocation[] allocations) =>
            DuePeriodRules.ScheduledFees(st, Session, ClassFees(), allocations, Today);

        [Fact]
        public void Returning_student_pays_session_fee_all_months_and_other_fees()
        {
            var schedule = Schedule(Student(new DateTime(2024, 1, 10)));

            Assert.DoesNotContain(schedule, f => f.Fee.StudentFeeHead.SL == 0);
            Assert.Single(schedule, f => f.Fee.StudentFeeHead.SL == 13);
            Assert.Equal(12, schedule.Count(f => f.Fee.StudentFeeHead.SL is >= 1 and <= 12));
            Assert.Equal(1500 + 12 * 500 + 800, schedule.Sum(f => f.Amount));
        }

        [Fact]
        public void New_student_pays_admission_fee_and_months_from_admission()
        {
            var schedule = Schedule(Student(new DateTime(2026, 4, 20)));

            var admission = Assert.Single(schedule, f => f.Fee.StudentFeeHead.SL == 0);
            Assert.Equal(new DateTime(2026, 4, 1), admission.DueMonth);
            Assert.DoesNotContain(schedule, f => f.Fee.StudentFeeHead.SL == 13);
            Assert.Equal(Enumerable.Range(4, 9), schedule.Where(f => f.Fee.StudentFeeHead.SL is >= 1 and <= 12).Select(f => f.DueMonth.Month));
        }

        [Fact]
        public void Fees_for_the_other_residential_status_are_not_charged()
        {
            var fees = ClassFees();
            fees.Add(Fee(99, 14, 3000, residential: true));
            var schedule = DuePeriodRules.ScheduledFees(Student(new DateTime(2024, 1, 1)), Session, fees, Array.Empty<StudentFeeAllocation>(), Today);
            Assert.DoesNotContain(schedule, f => f.Fee.Id == 99);
        }

        [Fact]
        public void Allocation_replaces_class_fee_amount()
        {
            var allocation = new StudentFeeAllocation { ClassFeeListId = 13, AllocatedAmount = 0, UniqueId = "U1", IsActive = true }; // March free
            var schedule = Schedule(Student(new DateTime(2024, 1, 1)), allocation);
            Assert.Equal(0, schedule.Single(f => f.Fee.Id == 13).Amount);
        }

        [Fact]
        public void Other_fee_uses_its_start_date_when_set()
        {
            var fees = ClassFees();
            fees.Add(Fee(50, 15, 600, start: new DateTime(2026, 6, 5)));
            var schedule = DuePeriodRules.ScheduledFees(Student(new DateTime(2024, 1, 1)), Session, fees, Array.Empty<StudentFeeAllocation>(), Today);

            Assert.Equal(new DateTime(2026, 6, 1), schedule.Single(f => f.Fee.Id == 50).DueMonth);
            Assert.Equal(new DateTime(2026, 1, 1), schedule.Single(f => f.Fee.Id == 3).DueMonth); // no start date: session start
        }

        [Fact]
        public void No_period_matches_the_original_whole_session_calculation()
        {
            var schedule = Schedule(Student(new DateTime(2024, 1, 1)));
            // Paid total includes a payment with no fee link, which the session view has always counted.
            var payments = new[] { Payment(2000, (11, 111, 500), (12, 112, 500)), Payment(700) };

            var result = DuePeriodRules.Calculate(schedule, payments, null, null);

            Assert.Equal(8300, result.Payable);
            Assert.Equal(2700, result.Paid);
            Assert.Equal(5600, result.Due);
        }

        [Fact]
        public void Period_counts_only_fees_in_range_and_payments_against_them()
        {
            var schedule = Schedule(Student(new DateTime(2024, 1, 1)));
            var payments = new[]
            {
                Payment(1000, (11, 111, 500), (12, 112, 500)), // Jan + Feb paid
                Payment(300, (13, 113, 300)),                  // part of March
                Payment(500, (17, 117, 500)),                  // July: outside the range
            };

            // 10 Feb - 20 Mar touches February and March.
            var result = DuePeriodRules.Calculate(schedule, payments, new DateTime(2026, 2, 10), new DateTime(2026, 3, 20));

            Assert.Equal(1000, result.Payable);
            Assert.Equal(800, result.Paid);
            Assert.Equal(200, result.Due);
        }

        [Fact]
        public void Up_to_a_month_includes_session_and_other_fees_from_session_start()
        {
            var schedule = Schedule(Student(new DateTime(2024, 1, 1)));
            var result = DuePeriodRules.Calculate(schedule, Array.Empty<StudentPayment>(), null, new DateTime(2026, 2, 28));
            Assert.Equal(1500 + 800 + 2 * 500, result.Payable);
        }

        [Fact]
        public void Older_payments_without_a_fee_link_match_by_fee_head()
        {
            var schedule = Schedule(Student(new DateTime(2024, 1, 1)));
            var payments = new[] { Payment(500, (0, 111, 500)) }; // January fee head, saved before fee links existed

            var result = DuePeriodRules.Calculate(schedule, payments, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            Assert.Equal(500, result.Paid);
            Assert.Equal(500 + 1500 + 800, result.Payable); // January also holds the session and exam fees
        }

        [Fact]
        public void Single_open_ended_bounds_work()
        {
            Assert.True(DuePeriodRules.InRange(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), null));
            Assert.False(DuePeriodRules.InRange(new DateTime(2026, 4, 1), new DateTime(2026, 5, 31), null));
            Assert.True(DuePeriodRules.InRange(new DateTime(2026, 5, 1), null, new DateTime(2026, 5, 1)));
            Assert.False(DuePeriodRules.InRange(new DateTime(2026, 6, 1), null, new DateTime(2026, 5, 31)));
        }

        [Theory]
        [InlineData("2026", 2026)]
        [InlineData("2025-2026", 2026)]
        [InlineData("Session 2027", 2027)]
        [InlineData("abc", 2026)]   // unreadable: fall back to the current year
        [InlineData(null, 2026)]
        public void Session_year_comes_from_the_name(string name, int expected)
        {
            Assert.Equal(expected, DuePeriodRules.SessionYear(new AcademicSession { Name = name }, Today));
        }
    }
}
