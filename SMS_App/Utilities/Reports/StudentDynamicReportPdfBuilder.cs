using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

public class StudentDynamicReportPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly DataTable _data;
    private readonly List<string> _columns;
    private readonly Dictionary<string, string> _columnLabels;

    public StudentDynamicReportPdfBuilder(
        Institute institute,
        string logoBase64,
        DataTable data,
        List<string> columns,
        Dictionary<string, string> columnLabels)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _data = data;
        _columns = columns;
        _columnLabels = columnLabels;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(8));
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (!string.IsNullOrEmpty(_logoBase64))
                {
                    try { row.ConstantItem(55).Image(Convert.FromBase64String(_logoBase64)).FitArea(); } catch { }
                }
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(14);
                    if (!string.IsNullOrEmpty(_institute.Address))
                        c.Item().AlignCenter().Text(_institute.Address).FontSize(8);
                    c.Item().AlignCenter().Text("Student Report").Bold().FontSize(11);
                    c.Item().AlignCenter().Text($"Generated: {DateTime.Today:dd MMM yyyy}").FontSize(8);
                });
            });
            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        var visibleCols = _data.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .Where(name => !name.StartsWith("Col") || _columns.Contains(name))
            .ToList();

        container.PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(28);
                foreach (var _ in visibleCols) c.RelativeColumn();
            });

            static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(3).AlignCenter();

            table.Header(h =>
            {
                h.Cell().Element(H).Text("SL").Bold();
                foreach (var col in visibleCols)
                {
                    var label = _columnLabels.TryGetValue(col, out var l) ? l : col;
                    h.Cell().Element(H).Text(label).Bold();
                }
            });

            int sl = 1;
            foreach (DataRow row in _data.Rows)
            {
                var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                IContainer D(IContainer c) => c.Background(bg).Padding(3);

                table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                foreach (var col in visibleCols)
                    table.Cell().Element(D).Text(row[col]?.ToString() ?? "");
                sl++;
            }
        });
    }
}
