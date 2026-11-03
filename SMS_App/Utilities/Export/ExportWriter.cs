using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace SMS_App.Utilities.Export
{
    /// <summary>
    /// Builds tabular CSV / XLSX byte payloads from a header row and string rows. Centralises the
    /// CSV quoting (RFC 4180) and the UTF-8 BOM so every export that uses it opens cleanly in Excel
    /// (including Bangla text), and gives a one-call ClosedXML worksheet for real .xlsx files.
    /// </summary>
    public static class ExportWriter
    {
        public const string CsvContentType = "text/csv";
        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static byte[] ToCsv(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(Field)));
            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(Field)));

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        public static byte[] ToXlsx(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName);

            for (int c = 0; c < headers.Count; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            ws.Row(1).Style.Font.Bold = true;

            int r = 2;
            foreach (var row in rows)
            {
                for (int c = 0; c < row.Count; c++)
                    ws.Cell(r, c + 1).Value = row[c] ?? string.Empty;
                r++;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        /// <summary>Quotes a CSV field per RFC 4180 (wrap in quotes, double any embedded quote).</summary>
        private static string Field(string value)
        {
            value ??= string.Empty;
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
