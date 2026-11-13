using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SchoolManagementSystem
{
    /// <summary>
    /// "What's new" for the running version, read from CHANGELOG.md (embedded into
    /// SMS_App.dll by SMS_App.csproj). Parsed once at startup.
    ///
    /// Format it understands (Keep a Changelog style):
    ///   ## [1.3.0] - 2026-11-09
    ///   ### Added
    ///   - **Title**: description, which may wrap
    ///     onto indented continuation lines.
    /// Entries starting with "CI" are internal (deploy pipeline) and are not shown to users.
    /// </summary>
    public static class ReleaseNotes
    {
        public sealed record Section(string Heading, IReadOnlyList<string> ItemsHtml);

        /// <summary>Version heading that matched, e.g. "1.3.0", or empty if none did.</summary>
        public static string Version { get; }

        /// <summary>Release date from the heading, e.g. "2026-11-09", or empty.</summary>
        public static string Date { get; }

        /// <summary>Groups such as Added / Fixed / Changed, in changelog order.</summary>
        public static IReadOnlyList<Section> Sections { get; }

        public static bool HasNotes => Sections.Count > 0;

        static ReleaseNotes()
        {
            Version = string.Empty;
            Date = string.Empty;
            Sections = Array.Empty<Section>();
            try
            {
                var text = ReadEmbeddedChangelog();
                if (string.IsNullOrEmpty(text)) return;

                // BuildInfo.Version is "1.3.0.57"; the changelog is keyed by "1.3.0".
                var product = string.Join(".", BuildInfo.Version.Split('.').Take(3));
                (Version, Date, Sections) = Parse(text, product);
            }
            catch
            {
                // Release notes are informational; never let them break page rendering.
            }
        }

        private static string ReadEmbeddedChangelog()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CHANGELOG.md");
            if (stream == null) return null;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private static readonly Regex VersionHeading =
            new(@"^##\s+\[?(?<v>[^\]\s]+)\]?(?:\s*-\s*(?<d>.+))?\s*$", RegexOptions.Compiled);

        internal static (string version, string date, IReadOnlyList<Section> sections) Parse(string text, string product)
        {
            var lines = text.Replace("\r\n", "\n").Split('\n');

            // Find "## [<product>]"; if this exact version has no entry, show nothing rather
            // than an older version's notes, which would mislead.
            int start = -1;
            string date = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                var m = VersionHeading.Match(lines[i]);
                if (m.Success && m.Groups["v"].Value == product)
                {
                    start = i + 1;
                    date = m.Groups["d"].Success ? m.Groups["d"].Value.Trim() : string.Empty;
                    break;
                }
            }
            if (start < 0) return (string.Empty, string.Empty, Array.Empty<Section>());

            var sections = new List<Section>();
            string heading = null;
            var items = new List<string>();
            StringBuilder current = null;

            void FlushItem()
            {
                if (current == null) return;
                var raw = current.ToString().Trim();
                current = null;
                if (raw.Length == 0 || raw.StartsWith("CI", StringComparison.Ordinal)) return;
                items.Add(ToHtml(raw));
            }

            void FlushSection()
            {
                FlushItem();
                if (heading != null && items.Count > 0)
                {
                    sections.Add(new Section(heading, items.ToArray()));
                }
                items = new List<string>();
            }

            for (int i = start; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("## ", StringComparison.Ordinal)) break; // next version

                if (line.StartsWith("### ", StringComparison.Ordinal))
                {
                    FlushSection();
                    heading = line.Substring(4).Trim();
                }
                else if (line.StartsWith("- ", StringComparison.Ordinal))
                {
                    FlushItem();
                    current = new StringBuilder(line.Substring(2).Trim());
                }
                else if (current != null && line.StartsWith("  ", StringComparison.Ordinal) && line.Trim().Length > 0)
                {
                    current.Append(' ').Append(line.Trim());
                }
                else if (line.Trim().Length == 0)
                {
                    FlushItem();
                }
            }
            FlushSection();

            return (product, date, sections);
        }

        // HTML-encode first, then allow only **bold**; backticks are dropped for readability.
        private static string ToHtml(string markdown)
        {
            var html = WebUtility.HtmlEncode(markdown);
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            html = html.Replace("`", string.Empty);
            return html;
        }
    }
}
