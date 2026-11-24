using System;

namespace SMS_App.Utilities.Institutes;

/// <summary>
/// Wall-clock time at the institute (Bangladesh), independent of the server's
/// own time zone. The Linux VPS runs on UTC, so <c>DateTime.Now</c> there is six
/// hours behind the school day; anything compared with the institute's opening
/// hours must use <see cref="Now"/> instead.
/// </summary>
public static class InstituteClock
{
    /// <summary>Asia/Dhaka (IANA) on Linux, "Bangladesh Standard Time" on Windows.</summary>
    public static TimeZoneInfo Zone { get; } = Resolve();

    private static TimeZoneInfo Resolve()
    {
        foreach (var id in new[] { "Asia/Dhaka", "Bangladesh Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone)) return zone;
        }
        // UTC+6 with no DST: correct for Bangladesh even without tz data.
        return TimeZoneInfo.CreateCustomTimeZone("BST+06", TimeSpan.FromHours(6), "Bangladesh", "Bangladesh");
    }

    /// <summary>The institute's local time for <paramref name="utcNow"/> (default: now).</summary>
    public static DateTime Now(DateTime? utcNow = null) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcNow ?? DateTime.UtcNow, DateTimeKind.Utc), Zone);

    /// <summary>
    /// Whether <paramref name="now"/> falls within opening hours, or null when no
    /// hours are configured (both default to midnight). Hours that cross
    /// midnight (closing earlier than starting) are handled.
    /// </summary>
    public static bool? IsOpen(TimeSpan start, TimeSpan end, TimeSpan now)
    {
        if (start == TimeSpan.Zero && end == TimeSpan.Zero) return null;
        return start <= end
            ? now >= start && now <= end
            : now >= start || now <= end;
    }
}
