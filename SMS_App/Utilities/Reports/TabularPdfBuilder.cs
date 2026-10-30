using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// A generic landscape table PDF built from a title, subtitle, header row and string rows.
/// Columns are driven entirely by the supplied headers, so any column selection (e.g. the student
/// export picker) renders without a bespoke builder. Zebra striping, a repeating header row and a
/// page/total footer come for free.
/// </summary>
public class TabularPdfBuilder : IDocument
{
    private readonly string _title;
    private readonly string _subtitle;
    private readonly IReadOnlyList<string> _headers;
    private readonly IReadOnlyList<IReadOnlyList<string>> _rows;

    public TabularPdfBuilder(string title, string subtitle,
        IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        _title = title;
        _subtitle = subtitle;
        _headers = headers;
        _rows = rows;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            // Shrink the font as the column count grows so wide selections still fit the page.
            page.DefaultTextStyle(x => x.FontSize(_headers.Count > 10 ? 7 : 8));
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Total: {_rows.Count}");
                row.RelativeItem().AlignCenter().Text(x =>
                {
                    x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
                });
                row.RelativeItem().AlignRight().Text($"Generated: {DateTime.Now:dd MMM yyyy hh:mm tt}");
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            if (!string.IsNullOrEmpty(_title))
                col.Item().AlignCenter().Text(_title).Bold().FontSize(14);
            if (!string.IsNullOrEmpty(_subtitle))
                col.Item().AlignCenter().Text(_subtitle).FontSize(10);
            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                foreach (var _ in _headers) cd.RelativeColumn();
            });

            static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(3);
            table.Header(h =>
            {
                foreach (var head in _headers)
                    h.Cell().Element(H).Text(head ?? string.Empty).Bold();
            });

            int sl = 1;
            foreach (var row in _rows)
            {
                var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                IContainer D(IContainer c) => c.Background(bg).Padding(3);
                foreach (var cell in row)
                    table.Cell().Element(D).Text(cell ?? string.Empty);
                sl++;
            }
        });
    }
}
