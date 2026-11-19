using System;

namespace SMS_App.Utilities.Employees;

/// <summary>
/// Today's attendance of the signed-in employee, from their first machine punch.
/// <see cref="Status"/> is a stable code the mobile app localizes and colors;
/// <see cref="Message"/> is the legacy English sentence older app builds display.
/// </summary>
public sealed record EmployeePresence(string Status, string Time, string Message)
{
    public const string Present = "present";
    public const string Late = "late";
    public const string Absent = "absent";
    public const string Unavailable = "unavailable";

    public static EmployeePresence NotAvailable { get; } = new(Unavailable, null, "Presence unavailable");

    /// <param name="firstPunch">Earliest punch today, or null when none.</param>
    /// <param name="lateAfter">Institute late threshold (time of day), or null when not configured.</param>
    public static EmployeePresence Classify(DateTime? firstPunch, TimeSpan? lateAfter)
    {
        if (firstPunch == null) return new(Absent, null, "You are absent today");

        var time = firstPunch.Value.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        var isLate = lateAfter != null && firstPunch.Value.TimeOfDay > lateAfter.Value;
        return isLate
            ? new(Late, time, $"You are late today ({time})")
            : new(Present, time, $"You are attended ({time}) today");
    }
}
