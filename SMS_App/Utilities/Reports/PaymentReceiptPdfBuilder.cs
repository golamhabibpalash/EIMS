using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels.StudentPayment;

namespace SMS_App.Utilities.Reports;

public class PaymentReceiptPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<RptPaymentReceiptVM> _data;
    private readonly string _amountInWords;

    public PaymentReceiptPdfBuilder(
        Institute institute,
        string logoBase64,
        List<RptPaymentReceiptVM> data,
        string amountInWords)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _data = data;
        _amountInWords = amountInWords;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5.Landscape());
            page.Margin(20);
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
        var first = _data.FirstOrDefault();

        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (!string.IsNullOrEmpty(_logoBase64))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(_logoBase64);
                        row.ConstantItem(50).Image(bytes).FitArea();
                    }
                    catch { }
                }

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(13);
                    if (!string.IsNullOrEmpty(_institute.Address))
                        c.Item().AlignCenter().Text(_institute.Address).FontSize(8);
                    if (!string.IsNullOrEmpty(_institute.EIIN))
                        c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(8);
                    c.Item().AlignCenter().Text("Payment Receipt").Bold().FontSize(11);
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(1);

            if (first != null)
            {
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    t.Cell().Text("Student Name:").SemiBold();
                    t.Cell().Text(first.Student_Name ?? "");
                    t.Cell().Text("Receipt No:").SemiBold();
                    t.Cell().Text(first.ReceiptNo ?? "");

                    t.Cell().Text("Class:").SemiBold();
                    t.Cell().Text(first.Class_Name ?? "");
                    t.Cell().Text("Section:").SemiBold();
                    t.Cell().Text(first.Section_Name ?? "");

                    t.Cell().Text("Roll:").SemiBold();
                    t.Cell().Text(first.ClassRoll.ToString());
                    t.Cell().Text("Date:").SemiBold();
                    t.Cell().Text(first.PaidDate.ToString("dd MMM yyyy"));
                });
            }

            col.Item().PaddingTop(4).LineHorizontal(1);
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
                    c.RelativeColumn(4);
                    c.RelativeColumn(2);
                });

                static IContainer HeaderCell(IContainer c) =>
                    c.Background(Colors.Grey.Lighten1).Padding(4).AlignCenter();

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("SL").Bold();
                    h.Cell().Element(HeaderCell).Text("Fee Head").Bold();
                    h.Cell().Element(HeaderCell).Text("Amount (৳)").Bold();
                });

                int sl = 1;
                foreach (var row in _data)
                {
                    var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                    IContainer DataCell(IContainer c) => c.Background(bg).Padding(4);

                    table.Cell().Element(DataCell).AlignCenter().Text(sl.ToString());
                    table.Cell().Element(DataCell).Text(row.Fee_Head ?? "");
                    table.Cell().Element(DataCell).AlignRight().Text(row.PaidAmount.ToString("N2"));
                    sl++;
                }
            });

            double total = _data.Sum(d => d.PaidAmount);

            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(180).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).Text("Total:").Bold();
                    t.Cell().Background(Colors.Grey.Lighten1).Padding(4).AlignRight().Text($"৳ {total:N2}").Bold();
                });
            });

            col.Item().PaddingTop(4).Text($"In Words: {_amountInWords}").Italic().FontSize(8);

            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().LineHorizontal(0.5f);
                    c.Item().AlignCenter().Text("Received By").FontSize(8);
                });
                row.ConstantItem(40);
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().LineHorizontal(0.5f);
                    c.Item().AlignCenter().Text("Authorized Signature").FontSize(8);
                });
            });
        });
    }
}
