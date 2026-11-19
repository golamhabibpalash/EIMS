using System;
using SMS_App.Utilities.Employees;
using Xunit;

namespace SMS.Tests.Employees
{
    /// <summary>Staff dashboard "today's attendance": a stable status code plus the first punch time.</summary>
    public class EmployeePresenceTests
    {
        private static readonly TimeSpan NineAm = new(9, 0, 0);

        [Fact]
        public void No_punch_is_absent()
        {
            var p = EmployeePresence.Classify(null, NineAm);
            Assert.Equal(EmployeePresence.Absent, p.Status);
            Assert.Null(p.Time);
        }

        [Fact]
        public void Punch_on_or_before_threshold_is_present()
        {
            var p = EmployeePresence.Classify(new DateTime(2026, 11, 19, 9, 0, 0), NineAm);
            Assert.Equal(EmployeePresence.Present, p.Status);
            Assert.Equal("09:00 AM", p.Time);
        }

        [Fact]
        public void Punch_after_threshold_is_late()
        {
            var p = EmployeePresence.Classify(new DateTime(2026, 11, 19, 9, 15, 0), NineAm);
            Assert.Equal(EmployeePresence.Late, p.Status);
            Assert.Equal("You are late today (09:15 AM)", p.Message);
        }

        [Fact]
        public void Without_a_threshold_any_punch_is_present()
        {
            var p = EmployeePresence.Classify(new DateTime(2026, 11, 19, 11, 0, 0), null);
            Assert.Equal(EmployeePresence.Present, p.Status);
        }
    }
}
