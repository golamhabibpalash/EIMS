using System.Globalization;

namespace SMS.Entities.Utilities
{
    /// <summary>
    /// Compares the free-text PIN/card columns used to tie an attendance machine punch to a
    /// person: Tran_MachineRawPunch.CardNo against Student.UniqueId / Employee.MachineUserId
    /// (or a roll number / employee id for legacy punches recorded before enrolment).
    ///
    /// All of these are plain string columns, and devices/admins are inconsistent about leading
    /// zeros - "0123" and "123" are the same PIN to the machine but different strings. A plain
    /// string compare treats them as different people, which showed up as the dashboard marking
    /// a punched-in student absent (while the monthly report, which happened to compare
    /// numerically, showed the same student present). Every place that matches a punch to a
    /// person should go through here so the two never drift apart again.
    /// </summary>
    public static class AttendancePinMatcher
    {
        /// <summary>
        /// Canonical form for equality/lookup: a value that parses as a whole number is
        /// normalized to its decimal form (so "0123", "123" and "00123" all become "123");
        /// anything else is trimmed and upper-invariant. Returns null for a blank value so
        /// callers can't accidentally match two absent PINs against each other.
        /// </summary>
        public static string Normalize(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return null;
            }

            pin = pin.Trim();

            return long.TryParse(pin, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
                ? numeric.ToString(CultureInfo.InvariantCulture)
                : pin.ToUpperInvariant();
        }

        /// <summary>True when both values normalize to the same, non-blank PIN.</summary>
        public static bool Matches(string a, string b)
        {
            var normalizedA = Normalize(a);
            return normalizedA != null && normalizedA == Normalize(b);
        }
    }
}
