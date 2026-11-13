using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels;

namespace SMS_App.Utilities.Reports;

public class StudentPaymentInfoPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly Student _student;
    private readonly List<rptStudentPaymentsVM> _payments;
    private readonly string _fromDate;
    private readonly string _toDate;
    private readonly string _amountInWords;

    public StudentPaymentInfoPdfBuilder(
        Institute institute,
        string logoBase64,
        Student student,
        List<rptStudentPaymentsVM> payments,
        string fromDate,
        string toDate,
        string amountInWords)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _student = student;
        _payments = payments;
        _fromDate = fromDate;
        _toDate = toDate;
        _amountInWords = amountInWords;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(9));

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
                        var imageBytes = Convert.FromBase64String(_logoBase64);
                        row.ConstantItem(60).Image(imageBytes).FitArea();
                    }
                    catch { }
                }

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(14);
                    if (!string.IsNullOrEmpty(_institute.Address))
                        c.Item().AlignCenter().Text(_institute.Address).FontSize(9);
                    if (!string.IsNullOrEmpty(_institute.EIIN))
                        c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(9);
                    c.Item().AlignCenter().Text("Payments Summary Report").Bold().FontSize(11);
                });
            });

            col.Item().PaddingVertical(5).LineHorizontal(1);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Cell().Text("Student Name:").SemiBold();
                table.Cell().Text(_student.Name ?? "");
                table.Cell().Text("Class:").SemiBold();
                table.Cell().Text(_student.AcademicClass?.Name ?? "");

                table.Cell().Text("Class Roll:").SemiBold();
                table.Cell().Text(_student.ClassRoll.ToString());
                table.Cell().Text("Session:").SemiBold();
                table.Cell().Text(_student.AcademicSession?.Name ?? "");

                table.Cell().Text("Date Range:").SemiBold();
                table.Cell().ColumnSpan(3).Text($"{_fromDate}  to  {_toDate}");
            });

            col.Item().PaddingTop(5).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(30);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                });

                // Header row
                static IContainer HeaderCell(IContainer c) => c
                    .Background(Colors.Grey.Lighten1)
                    .Padding(4)
                    .AlignCenter();

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("SL").Bold();
                    header.Cell().Element(HeaderCell).Text("Receipt No").Bold();
                    header.Cell().Element(HeaderCell).Text("Date").Bold();
                    header.Cell().Element(HeaderCell).Text("Payment Type").Bold();
                    header.Cell().Element(HeaderCell).Text("Amount (৳)").Bold();
                    header.Cell().Element(HeaderCell).Text("Remarks").Bold();
                });

                // Data rows
                int sl = 1;
                foreach (var p in _payments)
                {
                    var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                    IContainer DataCell(IContainer c) => c.Background(bg).Padding(4);

                    table.Cell().Element(DataCell).AlignCenter().Text(sl.ToString());
                    table.Cell().Element(DataCell).Text(p.ReceiptNo ?? "");
                    table.Cell().Element(DataCell).Text(p.PaidDate ?? "");
                    table.Cell().Element(DataCell).Text(p.PaymentTypeName ?? "");
                    table.Cell().Element(DataCell).AlignRight().Text(p.TotalPayment.ToString("N2"));
                    table.Cell().Element(DataCell).Text(p.Remarks ?? "");
                    sl++;
                }
            });

            double total = _payments.Sum(p => p.TotalPayment);

            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(200).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).Text("Total Amount:").Bold();
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).AlignRight().Text($"৳ {total:N2}").Bold();
                });
            });

            col.Item().PaddingTop(5).Text($"In Words: {_amountInWords}").Italic().FontSize(9);

            col.Item().PaddingTop(10).Text($"Report Generated: {DateTime.Today:dd MMM yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }
}
