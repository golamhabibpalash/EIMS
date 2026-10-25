using System.Globalization;
using System.Linq;

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

        /// <summary>
        /// All string forms worth matching in the database for one id: the trimmed
        /// value plus its canonical form (so "02607009" also finds "2607009" and
        /// vice versa). For use as <c>keys.Contains(column)</c> in EF queries,
        /// which translates to SQL IN - the canonicalizer itself cannot run in SQL.
        /// </summary>
        public static string[] KeyVariants(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return System.Array.Empty<string>();
            }

            var trimmed = pin.Trim();
            var normalized = Normalize(trimmed);
            var variants = new System.Collections.Generic.List<string> { trimmed, normalized };

            // A numeric PIN may be stored with leading zeros the punch doesn't carry (or the other
            // way round): "27826618" must also find a row holding "027826618" or "0027826618".
            if (normalized.All(char.IsDigit))
            {
                for (var zeros = 1; zeros <= 3; zeros++)
                {
                    variants.Add(new string('0', zeros) + normalized);
                }
            }

            return variants.Distinct().ToArray();
        }

        /// <summary>
        /// Every PIN a student's punches may arrive under, in priority order: the PIN set on the
        /// Machine Enrollment screen (MachineUserId - what the device sync enrolls), then the
        /// UniqueId, then the class roll (legacy punches recorded before UniqueId enrolment).
        /// Every place that ties a punch to a student must use this list, so a student enrolled
        /// under a custom PIN is never shown absent on one screen and present on another.
        /// </summary>
        public static string[] StudentPins(string machineUserId, string uniqueId, int classRoll)
        {
            return new[] { machineUserId, uniqueId, classRoll > 0 ? classRoll.ToString(CultureInfo.InvariantCulture) : null }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToArray();
        }

        /// <summary>True when the punch's PIN matches any of the student's PINs (see <see cref="StudentPins"/>).</summary>
        public static bool MatchesStudent(string cardNo, string machineUserId, string uniqueId, int classRoll)
        {
            var normalizedCard = Normalize(cardNo);
            return normalizedCard != null
                && StudentPins(machineUserId, uniqueId, classRoll).Any(p => Normalize(p) == normalizedCard);
        }
    }
}
