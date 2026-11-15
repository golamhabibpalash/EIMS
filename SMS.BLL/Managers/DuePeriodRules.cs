using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Pure rules for the current-session Due Payment list (no I/O): which fees a student is charged
    /// in a session, which month each fee falls due in, and the payable / paid / due for an optional
    /// date range. Shared by <see cref="StudentPaymentManager"/> and unit tests.
    /// </summary>
    public static class DuePeriodRules
    {
        /// <summary>Fee head serials: 0 = admission fee, 1-12 = monthly fee for that month, 13 = session fee, &gt;13 = other fees.</summary>
        public const int AdmissionFeeSl = 0;
        public const int SessionFeeSl = 13;

        public sealed class ScheduledFee
        {
            public ClassFeeList Fee { get; init; }
            /// <summary>The student's charge: their active allocation if any, otherwise the class fee.</summary>
            public double Amount { get; init; }
            /// <summary>First day of the month this fee falls due.</summary>
            public DateTime DueMonth { get; init; }
        }

        public sealed class DueAmounts
        {
            public double Payable { get; init; }
            public double Paid { get; init; }
            public double Due => Payable - Paid;
        }

        /// <summary>The session's calendar year, read from the last four characters of its name (e.g. "2026", "2025-2026").</summary>
        public static int SessionYear(AcademicSession session, DateTime today)
        {
            var name = session?.Name?.Trim();
            if (!string.IsNullOrEmpty(name) && name.Length >= 4 && int.TryParse(name[^4..], out int year) && year > 1900)
                return year;
            return today.Year;
        }

        /// <summary>
        /// Fees the student is charged this session. Same rules as the original due calculation:
        /// admission fee if admitted in the session year, otherwise the session fee; monthly fees from
        /// the admission month onward in the admission year; and every other fee. Fees are those of the
        /// student's class matching their residential status.
        /// </summary>
        public static List<ScheduledFee> ScheduledFees(Student student, AcademicSession session,
            IEnumerable<ClassFeeList> classFees, IEnumerable<StudentFeeAllocation> allocations, DateTime today)
        {
            int year = SessionYear(session, today);
            bool admittedThisSession = student.AdmissionDate.Year == year;
            var allocationByFee = allocations
                .Where(a => a.ClassFeeListId.HasValue)
                .GroupBy(a => a.ClassFeeListId.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var fees = classFees
                .Where(c => c.AcademicClassId == student.AcademicClassId && c.StudentFeeHead != null
                            && c.StudentFeeHead.IsResidential == student.IsResidential)
                .ToList();

            var result = new List<ScheduledFee>();

            // One-off fee for the year: admission fee for new students, session fee for the rest.
            int oneOffSl = admittedThisSession ? AdmissionFeeSl : SessionFeeSl;
            var oneOff = fees.FirstOrDefault(f => f.StudentFeeHead.SL == oneOffSl);
            if (oneOff != null)
            {
                var month = admittedThisSession
                    ? new DateTime(year, student.AdmissionDate.Month, 1)
                    : new DateTime(year, 1, 1);
                result.Add(Schedule(oneOff, allocationByFee, month));
            }

            foreach (var fee in fees.Where(f => f.StudentFeeHead.SL is >= 1 and <= 12))
            {
                // New students are not charged for months before they were admitted.
                // (A class fee with no serial is skipped for them too, as the original rule did.)
                if (admittedThisSession && !(fee.SL >= student.AdmissionDate.Month)) continue;
                result.Add(Schedule(fee, allocationByFee, new DateTime(year, fee.StudentFeeHead.SL.Value, 1)));
            }

            foreach (var fee in fees.Where(f => f.StudentFeeHead.SL > SessionFeeSl))
            {
                // Other fees fall due on their start date when one was set, else at the start of the session.
                var month = fee.StartDate.Year >= 2000
                    ? new DateTime(fee.StartDate.Year, fee.StartDate.Month, 1)
                    : new DateTime(year, 1, 1);
                result.Add(Schedule(fee, allocationByFee, month));
            }

            return result;
        }

        private static ScheduledFee Schedule(ClassFeeList fee, IReadOnlyDictionary<int, StudentFeeAllocation> allocationByFee, DateTime month) => new()
        {
            Fee = fee,
            Amount = allocationByFee.TryGetValue(fee.Id, out var allocation) ? allocation.AllocatedAmount : fee.Amount,
            DueMonth = month
        };

        /// <summary>
        /// Payable, paid and due for a student.
        /// <para>No date range: the whole session, with paid = every payment the student made this
        /// session (unchanged from the original calculation).</para>
        /// <para>With a date range: only fees falling due in a month the range touches, and only the
        /// payments made against those fees (matched by class fee, or by fee head for older payments
        /// saved without one), the same way the Payment screen shows each fee's balance.</para>
        /// </summary>
        public static DueAmounts Calculate(IReadOnlyList<ScheduledFee> schedule, IEnumerable<StudentPayment> sessionPayments,
            DateTime? from, DateTime? to)
        {
            var payments = sessionPayments?.ToList() ?? new List<StudentPayment>();

            if (from == null && to == null)
            {
                return new DueAmounts
                {
                    Payable = schedule.Sum(f => f.Amount),
                    Paid = payments.Sum(p => p.TotalPayment)
                };
            }

            var inRange = schedule.Where(f => InRange(f.DueMonth, from, to)).ToList();
            var details = payments.SelectMany(p => p.StudentPaymentDetails ?? new List<StudentPaymentDetails>()).ToList();

            return new DueAmounts
            {
                Payable = inRange.Sum(f => f.Amount),
                Paid = inRange.Sum(f => details
                    .Where(d => d.ClassFeeId == f.Fee.Id || (d.ClassFeeId == 0 && d.StudentFeeHeadId == f.Fee.StudentFeeHeadId))
                    .Sum(d => d.PaidAmount))
            };
        }

        /// <summary>Month-level overlap: a fee for March is in range for any range that touches March.</summary>
        public static bool InRange(DateTime dueMonth, DateTime? from, DateTime? to)
        {
            if (from.HasValue && dueMonth < new DateTime(from.Value.Year, from.Value.Month, 1)) return false;
            if (to.HasValue && dueMonth > new DateTime(to.Value.Year, to.Value.Month, 1)) return false;
            return true;
        }
    }
}
