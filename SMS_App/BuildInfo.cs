using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace SchoolManagementSystem
{
    /// <summary>
    /// Build and release details for the running application, read once from the
    /// assembly attributes stamped at publish time by Directory.Build.props.
    /// Bump the version in Directory.Build.props, never here.
    /// </summary>
    public static class BuildInfo
    {
        /// <summary>Product version including build number, e.g. "1.2.3.57".</summary>
        public static string Version { get; }

        /// <summary>Short commit the build came from, or "local" for a hand built publish.</summary>
        public static string Commit { get; }

        /// <summary>UTC build time, e.g. "2026-08-06 10:30:00", or empty if unknown.</summary>
        public static string BuiltUtc { get; }

        /// <summary>True when produced by the deploy workflow rather than a local publish.</summary>
        public static bool IsOfficialBuild => !string.Equals(Commit, "local", StringComparison.OrdinalIgnoreCase);

        /// <summary>Compact label for the footer / sidebar, e.g. "1.2.3.57" or "1.2.3.0 (local)".</summary>
        public static string ShortDisplay =>
            IsOfficialBuild ? Version : $"{Version} (local)";

        /// <summary>
        /// Per-assembly build stamps. The footer commit only reflects SMS_App.dll,
        /// so a stale SMS.DAL.dll (where most data-access fixes live) used to be
        /// undetectable. The Version dialog lists every layer; all commits must
        /// match after a full deploy.
        /// </summary>
        public static IReadOnlyList<(string Name, string Commit)> ComponentCommits()
        {
            return new[]
            {
                ("SMS_App", Commit),
                ("SMS.BLL", CommitOf(typeof(SMS.BLL.Contracts.IPhoneSMSManager))),
                ("SMS.DAL", CommitOf(typeof(SMS.DAL.Contracts.IStudentPaymentRepository))),
                ("SMS.DB", CommitOf(typeof(SMS.DB.ApplicationDbContext))),
                ("SMS.Entities", CommitOf(typeof(SMS.Entities.Student))),
            };
        }

        private static string CommitOf(Type typeInAssembly)
        {
            var informational = typeInAssembly.Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var parts = (informational ?? string.Empty).Split('+');
            return parts.Length > 1 && parts[0].Length > 0 ? parts[1] : "unknown";
        }

        static BuildInfo()
        {
            // Directory.Build.props writes "<version>+<commit>+<utc timestamp>".
            var informational = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            var parts = (informational ?? string.Empty).Split('+');

            Version = parts.Length > 0 && parts[0].Length > 0
                ? parts[0]
                : Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

            Commit = parts.Length > 1 ? parts[1] : "local";

            // Timestamp may itself contain no '+', but rejoin defensively in case the
            // format ever changes to include one.
            BuiltUtc = parts.Length > 2 ? string.Join("+", parts, 2, parts.Length - 2) : string.Empty;
        }

        /// <summary>Build time rendered in the given time zone, falling back to the raw UTC text.</summary>
        public static string BuiltLocal(TimeZoneInfo timeZone)
        {
            if (string.IsNullOrWhiteSpace(BuiltUtc))
                return string.Empty;

            if (!DateTime.TryParseExact(BuiltUtc, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var utc))
            {
                return BuiltUtc;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone).ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture);
        }
    }
}
