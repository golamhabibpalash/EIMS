using System.Text.RegularExpressions;

namespace SMS_App.Utilities.LoggerService;

/// <summary>
/// Marks secrets (passwords) inside activity-log messages so the Logs screen shows them only to
/// a SuperAdmin. Writers wrap the value with <see cref="Secret"/>; readers call
/// <see cref="ForViewer"/>. Older rows written before the marker existed are recognised by their
/// wording and masked the same way.
/// </summary>
public static class SensitiveLogText
{
    public const string Masked = "•••••• (SuperAdmin only)";

    private const string Open = "[[secret:";
    private const string Close = "]]";

    private static readonly Regex Marker = new(@"\[\[secret:(.*?)\]\]", RegexOptions.Compiled | RegexOptions.Singleline);

    // Wording used before the marker: "...and password: abc123" and "Password reset to abc123 by x".
    private static readonly Regex LegacyEmployee = new(@"(and password:\s*)(\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex LegacyReset = new(@"(Password reset to )(.*?)( by )", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>Wraps a secret so it is hidden from everyone but a SuperAdmin.</summary>
    public static string Secret(string value) => Open + (value ?? string.Empty).Replace(Close, "] ]") + Close;

    /// <summary>The text as the viewer may see it: secrets revealed for a SuperAdmin, masked otherwise.</summary>
    public static string ForViewer(string text, bool isSuperAdmin)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        if (isSuperAdmin)
            return Marker.Replace(text, m => m.Groups[1].Value);

        text = Marker.Replace(text, Masked);
        text = LegacyEmployee.Replace(text, m => m.Groups[1].Value + Masked);
        return LegacyReset.Replace(text, m => m.Groups[1].Value + Masked + m.Groups[3].Value);
    }
}
