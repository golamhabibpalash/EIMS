using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// The attendance sheet shown on /AttendanceMachines, in a format-neutral shape so the PDF,
/// Excel and Word exports all carry exactly the same header, columns, rows and totals.
/// </summary>
public class AttendanceSheet
{
    public string InstituteName { get; set; }
    public string InstituteAddress { get; set; }
    public byte[] Logo { get; set; }
    public string Title { get; set; }
    /// <summary>Filter lines under the title, e.g. "Date: 20 Oct 2026", "Class: Six".</summary>
    public List<string> Filters { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    /// <summary>Relative widths for the PDF/Word layout, one per column.</summary>
    public List<float> ColumnWidths { get; set; } = new();
    public List<string[]> Rows { get; set; } = new();
    public int Present { get; set; }
    public int Absent { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public string TotalsLine => $"Total: {Rows.Count}   Present: {Present}   Absent: {Absent}";
}

public static class AttendanceSheetExport
{
    public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    #region PDF

    public static byte[] ToPdf(AttendanceSheet sheet) => new AttendanceSheetPdf(sheet).GeneratePdf();

    private class AttendanceSheetPdf : IDocument
    {
        private readonly AttendanceSheet _s;

        public AttendanceSheetPdf(AttendanceSheet sheet) => _s = sheet;

        public DocumentMetadata GetMetadata() => new() { Title = _s.Title };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(8));
                page.Header().Element(Header);
                page.Content().Element(Content);
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(_s.TotalsLine);
                    row.RelativeItem().AlignCenter().Text(x =>
                    {
                        x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
                    });
                    row.RelativeItem().AlignRight().Text($"Generated: {_s.GeneratedAt:dd MMM yyyy hh:mm tt}");
                });
            });
        }

        private void Header(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (_s.Logo != null && _s.Logo.Length > 0)
                    {
                        try { row.ConstantItem(50).Image(_s.Logo).FitArea(); } catch { /* a bad logo file must not break the export */ }
                    }
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text(_s.InstituteName ?? string.Empty).Bold().FontSize(14);
                        if (!string.IsNullOrEmpty(_s.InstituteAddress))
                            c.Item().AlignCenter().Text(_s.InstituteAddress).FontSize(8);
                        c.Item().AlignCenter().Text(_s.Title).Bold().FontSize(11);
                        c.Item().AlignCenter().Text(string.Join("   |   ", _s.Filters)).FontSize(8.5f);
                    });
                    if (_s.Logo != null && _s.Logo.Length > 0)
                    {
                        row.ConstantItem(50); // keeps the title centred on the page
                    }
                });
                col.Item().PaddingVertical(4).LineHorizontal(1);
            });
        }

        private void Content(IContainer container)
        {
            container.PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    foreach (var w in _s.ColumnWidths)
                        c.RelativeColumn(w);
                });

                static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Border(0.5f).BorderColor(Colors.Grey.Darken1).Padding(3);
                table.Header(h =>
                {
                    foreach (var col in _s.Columns)
                        h.Cell().Element(H).Text(col).Bold();
                });

                var i = 0;
                foreach (var row in _s.Rows)
                {
                    var bg = i++ % 2 == 1 ? Colors.Grey.Lighten4 : Colors.White;
                    var absent = row[^1] == "Absent";
                    foreach (var value in row)
                    {
                        var text = table.Cell().Background(bg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(value ?? string.Empty);
                        if (absent && value == "Absent")
                            text.FontColor(Colors.Red.Darken2);
                    }
                }
            });
        }
    }

    #endregion PDF

    #region Excel

    public static byte[] ToXlsx(AttendanceSheet sheet)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Attendance");
        var lastCol = sheet.Columns.Count;
        var r = 1;

        void TitleRow(string text, double size, bool bold)
        {
            var range = ws.Range(r, 1, r, lastCol).Merge();
            range.Value = text;
            range.Style.Font.FontSize = size;
            range.Style.Font.Bold = bold;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            r++;
        }

        TitleRow(sheet.InstituteName ?? string.Empty, 14, true);
        if (!string.IsNullOrEmpty(sheet.InstituteAddress))
            TitleRow(sheet.InstituteAddress, 10, false);
        TitleRow(sheet.Title, 12, true);
        TitleRow(string.Join("   |   ", sheet.Filters), 10, false);
        r++;

        var headerRow = r;
        for (var c = 0; c < lastCol; c++)
            ws.Cell(r, c + 1).Value = sheet.Columns[c];
        var header = ws.Range(r, 1, r, lastCol);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        r++;

        foreach (var row in sheet.Rows)
        {
            for (var c = 0; c < lastCol; c++)
            {
                // Text, not numbers: phone numbers and card numbers must keep their leading zeros.
                var cell = ws.Cell(r, c + 1);
                cell.SetValue(row[c] ?? string.Empty);
                cell.Style.NumberFormat.Format = "@";
            }
            if (row[^1] == "Absent")
                ws.Cell(r, lastCol).Style.Font.FontColor = XLColor.DarkRed;
            r++;
        }

        var table = ws.Range(headerRow, 1, Math.Max(headerRow, r - 1), lastCol);
        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        if (sheet.Rows.Count > 0)
            ws.Range(headerRow, 1, r - 1, lastCol).SetAutoFilter();

        r++;
        ws.Cell(r, 1).Value = sheet.TotalsLine;
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Cell(r + 1, 1).Value = $"Generated: {sheet.GeneratedAt:dd MMM yyyy hh:mm tt}";
        ws.Cell(r + 1, 1).Style.Font.Italic = true;

        ws.SheetView.FreezeRows(headerRow);
        ws.Columns(1, lastCol).AdjustToContents(headerRow, r - 2);
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0);
        ws.PageSetup.SetRowsToRepeatAtTop(headerRow, headerRow);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion Excel

    #region Word

    public static byte[] ToDocx(AttendanceSheet sheet)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            AddDefaultStyles(main);
            var body = new Body();
            main.Document = new W.Document(body);

            body.Append(Paragraph(sheet.InstituteName ?? string.Empty, 28, bold: true, center: true));
            if (!string.IsNullOrEmpty(sheet.InstituteAddress))
                body.Append(Paragraph(sheet.InstituteAddress, 18, bold: false, center: true));
            body.Append(Paragraph(sheet.Title, 24, bold: true, center: true));
            body.Append(Paragraph(string.Join("   |   ", sheet.Filters), 18, bold: false, center: true));

            body.Append(BuildTable(sheet));

            body.Append(Paragraph(sheet.TotalsLine, 18, bold: true, center: false));
            body.Append(Paragraph($"Generated: {sheet.GeneratedAt:dd MMM yyyy hh:mm tt}", 16, bold: false, center: false));

            // A4 portrait, 0.5" margins (sizes are in twentieths of a point).
            body.Append(new SectionProperties(
                new W.PageSize { Width = 11906U, Height = 16838U },
                new PageMargin { Top = 720, Bottom = 720, Left = 720U, Right = 720U, Header = 360U, Footer = 360U }));
            main.Document.Save();
        }
        return UseRelativeRelationshipTargets(stream.ToArray());
    }

    /// <summary>
    /// The packaging library writes absolute relationship targets ("/word/document.xml"). Word
    /// accepts them, but macOS readers (Pages, TextEdit, Quick Look) only accept the relative form
    /// Word itself writes, so rewrite them before the file is sent.
    /// </summary>
    private static byte[] UseRelativeRelationshipTargets(byte[] docx)
    {
        using var stream = new MemoryStream();
        stream.Write(docx, 0, docx.Length);
        using (var zip = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Update, leaveOpen: true))
        {
            foreach (var (entryName, prefix) in new[] { ("_rels/.rels", "Target=\"/"), ("word/_rels/document.xml.rels", "Target=\"/word/") })
            {
                var entry = zip.GetEntry(entryName);
                if (entry == null)
                    continue;
                string xml;
                using (var reader = new StreamReader(entry.Open()))
                    xml = reader.ReadToEnd();
                entry.Delete();
                using var writer = new StreamWriter(zip.CreateEntry(entryName).Open(), new System.Text.UTF8Encoding(false));
                writer.Write(xml.Replace(prefix, "Target=\""));
            }
        }
        return stream.ToArray();
    }

    /// <summary>A minimal styles part with a default font - some readers refuse a .docx without one.</summary>
    private static void AddDefaultStyles(MainDocumentPart main)
    {
        var stylesPart = main.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(new RunPropertiesBaseStyle(
                    new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", ComplexScript = "Calibri", EastAsia = "Calibri" },
                    new FontSize { Val = "20" },
                    new Languages { Val = "en-US" })),
                new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
                    new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }))),
            new Style(new StyleName { Val = "Normal" }, new PrimaryStyle()) { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true });
        stylesPart.Styles.Save();
    }

    private static Table BuildTable(AttendanceSheet sheet)
    {
        var border = new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4 },
            new LeftBorder { Val = BorderValues.Single, Size = 4 },
            new BottomBorder { Val = BorderValues.Single, Size = 4 },
            new RightBorder { Val = BorderValues.Single, Size = 4 },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 });

        var table = new Table(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            border,
            new TableLayout { Type = TableLayoutValues.Fixed }));

        // Usable width of A4 with 0.5" margins is ~10466 twips; split it by the relative widths.
        var total = sheet.ColumnWidths.Sum();
        var grid = new TableGrid();
        var widths = sheet.ColumnWidths.Select(w => (int)(10466 * w / total)).ToList();
        foreach (var w in widths)
            grid.Append(new GridColumn { Width = w.ToString() });
        table.Append(grid);

        var header = new TableRow(new TableRowProperties(new TableHeader())); // repeats on every page
        for (var c = 0; c < sheet.Columns.Count; c++)
            header.Append(Cell(sheet.Columns[c], widths[c], bold: true, shade: "D9D9D9"));
        table.Append(header);

        foreach (var row in sheet.Rows)
        {
            var tr = new TableRow(new TableRowProperties(new CantSplit()));
            for (var c = 0; c < row.Length; c++)
                tr.Append(Cell(row[c] ?? string.Empty, widths[c], bold: false, shade: null, color: row[c] == "Absent" ? "C00000" : null));
            table.Append(tr);
        }
        return table;
    }

    private static TableCell Cell(string text, int width, bool bold, string shade, string color = null)
    {
        var props = new TableCellProperties(new TableCellWidth { Width = width.ToString(), Type = TableWidthUnitValues.Dxa });
        if (shade != null)
            props.Append(new Shading { Val = ShadingPatternValues.Clear, Fill = shade, Color = "auto" });

        // Schema order inside rPr matters (b, color, sz) or Word reports unreadable content.
        var runProps = new RunProperties();
        if (bold) runProps.Append(new Bold());
        if (color != null) runProps.Append(new W.Color { Val = color });
        runProps.Append(new FontSize { Val = "16" });

        var paragraph = new W.Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "20", After = "20" }),
            new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        return new TableCell(props, paragraph);
    }

    private static W.Paragraph Paragraph(string text, int halfPoints, bool bold, bool center)
    {
        var runProps = new RunProperties();
        if (bold) runProps.Append(new Bold());
        runProps.Append(new FontSize { Val = halfPoints.ToString() });
        var paraProps = new ParagraphProperties(new SpacingBetweenLines { Before = "0", After = "60" });
        if (center) paraProps.Append(new Justification { Val = JustificationValues.Center });
        return new W.Paragraph(paraProps, new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    #endregion Word
}
