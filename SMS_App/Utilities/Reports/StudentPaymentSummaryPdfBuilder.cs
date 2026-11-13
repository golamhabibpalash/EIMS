using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels.StudentPayment;

namespace SMS_App.Utilities.Reports;

public class StudentPaymentSummaryPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<RptStudentsPaymentVM> _payments;
    private readonly string _reportName;
    private readonly string _fromDate;
    private readonly string _toDate;
    private readonly string _amountInWords;

    public StudentPaymentSummaryPdfBuilder(
        Institute institute,
        string logoBase64,
        List<RptStudentsPaymentVM> payments,
        string reportName,
        string fromDate,
        string toDate,
        string amountInWords)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _payments = payments;
        _reportName = reportName;
        _fromDate = fromDate;
        _toDate = toDate;
        _amountInWords = amountInWords;
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
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
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
                    try
                    {
                        var bytes = Convert.FromBase64String(_logoBase64);
                        row.ConstantItem(55).Image(bytes).FitArea();
                    }
                    catch { }
                }

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(14);
                    if (!string.IsNullOrEmpty(_institute.Address))
                        c.Item().AlignCenter().Text(_institute.Address).FontSize(8);
                    if (!string.IsNullOrEmpty(_institute.EIIN))
                        c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(8);
                    c.Item().AlignCenter().Text(_reportName).Bold().FontSize(11);
                    c.Item().AlignCenter().Text($"Period: {_fromDate}  to  {_toDate}").FontSize(8);
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(30);
                    c.ConstantColumn(55);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                static IContainer HeaderCell(IContainer c) =>
                    c.Background(Colors.Grey.Lighten1).Padding(3).AlignCenter();

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("SL").Bold();
                    h.Cell().Element(HeaderCell).Text("Roll").Bold();
                    h.Cell().Element(HeaderCell).Text("Student Name").Bold();
                    h.Cell().Element(HeaderCell).Text("Class").Bold();
                    h.Cell().Element(HeaderCell).Text("Section").Bold();
                    h.Cell().Element(HeaderCell).Text("Payment Type").Bold();
                    h.Cell().Element(HeaderCell).Text("Date").Bold();
                    h.Cell().Element(HeaderCell).Text("Amount (৳)").Bold();
                });

                int sl = 1;
                foreach (var p in _payments.OrderBy(x => x.ClassRoll))
                {
                    var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                    IContainer DataCell(IContainer c) => c.Background(bg).Padding(3);

                    table.Cell().Element(DataCell).AlignCenter().Text(sl.ToString());
                    table.Cell().Element(DataCell).AlignCenter().Text(p.ClassRoll ?? "");
                    table.Cell().Element(DataCell).Text(p.StudentName ?? "");
                    table.Cell().Element(DataCell).Text(p.AcademicClassName ?? "");
                    table.Cell().Element(DataCell).Text(p.AcademicSection ?? "");
                    table.Cell().Element(DataCell).Text(p.PaymentType ?? "");
                    table.Cell().Element(DataCell).Text(p.PaidDate ?? "");
                    table.Cell().Element(DataCell).AlignRight().Text(p.TotalPayment.ToString("N2"));
                    sl++;
                }
            });

            double total = _payments.Sum(p => p.TotalPayment);

            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(200).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).Text("Total Amount:").Bold();
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).AlignRight().Text($"৳ {total:N2}").Bold();
                });
            });

            col.Item().PaddingTop(4).Text($"In Words: {_amountInWords}").Italic().FontSize(8);
            col.Item().PaddingTop(6).Text($"Total Records: {_payments.Count}  |  Generated: {DateTime.Today:dd MMM yyyy}").FontSize(7).FontColor(Colors.Grey.Darken1);
        });
    }
}
