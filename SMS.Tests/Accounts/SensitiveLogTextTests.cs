using SMS_App.Utilities.LoggerService;
using Xunit;

namespace SMS.Tests.Accounts
{
    /// <summary>Passwords written to the activity log are shown to a SuperAdmin only.</summary>
    public class SensitiveLogTextTests
    {
        private static readonly string Reset = $"Password reset to {SensitiveLogText.Secret("p@ss 123")} by 2025100012";

        [Fact]
        public void SuperAdmin_sees_the_password() =>
            Assert.Equal("Password reset to p@ss 123 by 2025100012", SensitiveLogText.ForViewer(Reset, isSuperAdmin: true));

        [Fact]
        public void Others_see_it_masked()
        {
            var shown = SensitiveLogText.ForViewer(Reset, isSuperAdmin: false);
            Assert.Equal($"Password reset to {SensitiveLogText.Masked} by 2025100012", shown);
            Assert.DoesNotContain("p@ss", shown);
        }

        [Theory]
        [InlineData("Password reset to oldSecret1 by teacher@school", "Password reset to {0} by teacher@school")]
        [InlineData("User created for employee: Rahim with email: r@x.com and password: Ab12Cd", "User created for employee: Rahim with email: r@x.com and password: {0}")]
        public void Rows_written_before_the_marker_are_masked_too(string legacy, string expected) =>
            Assert.Equal(string.Format(expected, SensitiveLogText.Masked), SensitiveLogText.ForViewer(legacy, isSuperAdmin: false));

        [Fact]
        public void Ordinary_messages_are_untouched() =>
            Assert.Equal("Impersonation started: a is now testing as b.", SensitiveLogText.ForViewer("Impersonation started: a is now testing as b.", false));
    }
}
