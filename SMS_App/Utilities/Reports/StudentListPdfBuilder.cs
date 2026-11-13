using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels;

namespace SMS_App.Utilities.Reports;

public class StudentListPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<RptStudentVM> _students;

    public StudentListPdfBuilder(Institute institute, string logoBase64, List<RptStudentVM> students)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _students = students;
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
                    if (!string.IsNullOrEmpty(_institute.EIIN))
                        c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(8);
                    c.Item().AlignCenter().Text("Student List").Bold().FontSize(11);
                    c.Item().AlignCenter().Text($"Generated: {DateTime.Today:dd MMM yyyy}").FontSize(8);
                });
            });
            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(25);
                c.ConstantColumn(50);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
                c.RelativeColumn(3);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
            });

            static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(3).AlignCenter();

            table.Header(h =>
            {
                h.Cell().Element(H).Text("SL").Bold();
                h.Cell().Element(H).Text("Roll").Bold();
                h.Cell().Element(H).Text("Student Name").Bold();
                h.Cell().Element(H).Text("Class").Bold();
                h.Cell().Element(H).Text("Section").Bold();
                h.Cell().Element(H).Text("Father's Name").Bold();
                h.Cell().Element(H).Text("Mother's Name").Bold();
                h.Cell().Element(H).Text("Phone").Bold();
                h.Cell().Element(H).Text("Religion").Bold();
            });

            int sl = 1;
            foreach (var s in _students)
            {
                var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                IContainer D(IContainer c) => c.Background(bg).Padding(3);

                table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                table.Cell().Element(D).AlignCenter().Text(s.ClassRoll ?? "");
                table.Cell().Element(D).Text(s.StudentName ?? "");
                table.Cell().Element(D).Text(s.ClassName ?? "");
                table.Cell().Element(D).Text(s.SectionName ?? "");
                table.Cell().Element(D).Text(s.FatherName ?? "");
                table.Cell().Element(D).Text(s.MotherName ?? "");
                table.Cell().Element(D).Text(s.PhoneNo ?? s.GuardianPhone ?? "");
                table.Cell().Element(D).Text(s.Religion ?? "");
                sl++;
            }
        });
    }
}
