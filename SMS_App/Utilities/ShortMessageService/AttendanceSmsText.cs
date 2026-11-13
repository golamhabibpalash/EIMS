using SMS.Entities;

namespace SMS_App.Utilities.ShortMessageService
{
    /// <summary>
    /// Single source of truth for attendance SMS wording. Hangfire jobs,
    /// the real-time punch notifier, manual entry and the dry-run test page
    /// all compose through here, so the on-screen preview is byte-identical
    /// to what guardians receive. Each instance signs with its own
    /// Institute ShortName (fallback: Name, then no suffix) - never hard-code
    /// an institute name here.
    /// </summary>
    public static class AttendanceSmsText
    {
        public static string DisplayName(Institute institute)
        {
            if (institute == null)
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(institute.ShortName))
                return institute.ShortName.Trim();
            return institute.Name?.Trim() ?? string.Empty;
        }

        public static string Suffix(string instituteShortName) =>
            string.IsNullOrWhiteSpace(instituteShortName) ? string.Empty : " -" + instituteShortName.Trim();

        public static string CheckIn(string name, string attendanceTime, string instituteShortName)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(attendanceTime))
                return string.Empty;
            return name + " আজ " + attendanceTime + " মিনিটে স্কুলে উপস্থিত হয়েছে।" + Suffix(instituteShortName) + " ।";
        }

        public static string CheckOut(string name, string attendanceTime, string instituteShortName)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(attendanceTime))
                return string.Empty;
            return name + " স্কুল থেকে " + attendanceTime + " মিনিটে প্রস্থান করেছে।" + Suffix(instituteShortName) + " ।";
        }

        public static string AbsentToday(string name, string date, string instituteShortName)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return name + " আজ (" + date + ") স্কুলে আসেনি ।" + Suffix(instituteShortName) + "।";
        }

        public static string AbsentOngoing(string name, int absentDayCount, string instituteShortName)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return name + "গত " + absentDayCount + " দিন থেকে স্কুলে আসছে না ।" + Suffix(instituteShortName) + "।";
        }
    }
}
