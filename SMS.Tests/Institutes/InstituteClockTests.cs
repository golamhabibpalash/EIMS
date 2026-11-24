using System;
using SMS_App.Utilities.Institutes;
using Xunit;

namespace SMS.Tests.Institutes
{
    /// <summary>"Institute Status: Open/Closed" must follow Bangladesh time, not the server's (UTC) clock.</summary>
    public class InstituteClockTests
    {
        private static readonly TimeSpan Eight = new(8, 0, 0);
        private static readonly TimeSpan Two = new(14, 0, 0);

        [Fact]
        public void Converts_utc_to_bangladesh_time()
        {
            // 06:55 UTC on the VPS is 12:55 in Dhaka — inside school hours.
            var local = InstituteClock.Now(new DateTime(2026, 11, 24, 6, 55, 0, DateTimeKind.Utc));
            Assert.Equal(new DateTime(2026, 11, 24, 12, 55, 0), local);
            Assert.True(InstituteClock.IsOpen(Eight, Two, local.TimeOfDay));
        }

        [Theory]
        [InlineData(7, 59, false)]
        [InlineData(8, 0, true)]
        [InlineData(14, 0, true)]
        [InlineData(14, 1, false)]
        public void Open_between_starting_and_closing_time(int h, int m, bool open)
        {
            Assert.Equal(open, InstituteClock.IsOpen(Eight, Two, new TimeSpan(h, m, 0)));
        }

        [Fact]
        public void Unset_hours_are_unknown()
        {
            Assert.Null(InstituteClock.IsOpen(TimeSpan.Zero, TimeSpan.Zero, Eight));
        }

        [Fact]
        public void Hours_crossing_midnight()
        {
            var night = new TimeSpan(22, 0, 0);
            var six = new TimeSpan(6, 0, 0);
            Assert.True(InstituteClock.IsOpen(night, six, new TimeSpan(23, 0, 0)));
            Assert.True(InstituteClock.IsOpen(night, six, new TimeSpan(5, 0, 0)));
            Assert.False(InstituteClock.IsOpen(night, six, Eight));
        }
    }
}
