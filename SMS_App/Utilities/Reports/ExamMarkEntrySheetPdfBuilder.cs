using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using System;
using System.Collections.Generic;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// The physical mark-entry sheet printed from /AcademicExams/Details/{id}.
/// It is intentionally blank in the Marks/Remarks columns so the exam teacher can
/// carry it to the classroom, fill it by hand and later enter the data in EIMS.
/// </summary>
public class ExamMarkEntrySheetData
{
    public Institute Institute { get; set; }
    public byte[] Logo { get; set; }
    public byte[] Signature { get; set; }
    public string ExamName { get; set; }
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public string ExamCategory { get; set; }
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public string SessionName { get; set; }
    public string MonthName { get; set; }
    public int TotalMarks { get; set; }
    public string TeacherName { get; set; }
    public List<ExamMarkEntryStudent> Students { get; set; } = new();
}

public class ExamMarkEntryStudent
{
    public int ClassRoll { get; set; }
    public string Name { get; set; }
}

public class ExamMarkEntrySheetPdfBuilder
{
    private readonly ExamMarkEntrySheetData _s;

    public ExamMarkEntrySheetPdfBuilder(ExamMarkEntrySheetData sheet) => _s = sheet;

    public byte[] GeneratePdf() => Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(9));
            page.Header().Element(Header);
            page.Content().Element(Content);
            page.Footer().Element(Footer);
        });
    }).GeneratePdf();

    private void Header(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (_s.Logo != null && _s.Logo.Length > 0)
                {
                    try { row.ConstantItem(55).AlignMiddle().Image(_s.Logo).FitArea(); }
                    catch { /* a bad logo file must not break the export */ }
                }

                row.RelativeItem().PaddingHorizontal(8).Column(c =>
                {
                    c.Item().AlignCenter().Text(_s.Institute?.Name ?? string.Empty).Bold().FontSize(16);
                    if (!string.IsNullOrWhiteSpace(_s.Institute?.Address))
                        c.Item().AlignCenter().Text(_s.Institute.Address).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(_s.Institute?.EIIN))
                        c.Item().AlignCenter().Text($"EIIN: {_s.Institute.EIIN}").FontSize(9);
                });

                if (_s.Logo != null && _s.Logo.Length > 0)
                    row.ConstantItem(55); // keeps the school name centred on the page
            });

            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
            col.Item().AlignCenter().Text("EXAM MARK ENTRY SHEET").Bold().FontSize(14);
            col.Item().AlignCenter().Text("(To be filled in by the exam teacher)").FontSize(8).Italic();
            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Black);
            col.Item().PaddingTop(6).Element(InfoBlock);
        });
    }

    private void InfoBlock(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(90);
                c.RelativeColumn();
                c.ConstantColumn(85);
                c.RelativeColumn();
            });

            void Info(string label1, string value1, string label2, string value2)
            {
                table.Cell().PaddingBottom(3).Text(label1).Bold();
                table.Cell().PaddingBottom(3).Text(value1 ?? string.Empty);
                table.Cell().PaddingBottom(3).Text(label2).Bold();
                table.Cell().PaddingBottom(3).Text(value2 ?? string.Empty);
            }

            Info("Exam Name", _s.ExamName,
                 "Subject", string.IsNullOrWhiteSpace(_s.SubjectCode)
                    ? _s.SubjectName
                    : $"{_s.SubjectName} ({_s.SubjectCode})");

            Info("Exam Category", _s.ExamCategory,
                 "Class", $"{_s.ClassName} ({_s.SectionName})");

            Info("Session", _s.SessionName,
                 "Month", _s.MonthName);

            Info("Exam Teacher", _s.TeacherName,
                 "Total Marks", _s.TotalMarks.ToString());
        });
    }

    private void Content(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(30);
                c.ConstantColumn(60);
                c.RelativeColumn(4);
                c.ConstantColumn(70);
                c.RelativeColumn(3);
            });

            static IContainer Head(IContainer c) => c
                .Background(Colors.Grey.Lighten2)
                .Border(0.7f)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(5)
                .PaddingHorizontal(4);

            table.Header(h =>
            {
                h.Cell().Element(Head).AlignCenter().Text("SL").Bold();
                h.Cell().Element(Head).AlignCenter().Text("Class Roll").Bold();
                h.Cell().Element(Head).Text("Student Name").Bold();
                h.Cell().Element(Head).AlignCenter().Text("Marks").Bold();
                h.Cell().Element(Head).Text("Remarks").Bold();
            });

            static IContainer Body(IContainer c) => c
                .Border(0.7f)
                .BorderColor(Colors.Grey.Darken1)
                .MinHeight(24)
                .PaddingVertical(5)
                .PaddingHorizontal(4);

            var i = 0;
            foreach (var student in _s.Students)
            {
                i++;
                var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                table.Cell().Background(bg).Element(Body).AlignCenter().Text(i.ToString());
                table.Cell().Background(bg).Element(Body).AlignCenter().Text(student.ClassRoll.ToString());
                table.Cell().Background(bg).Element(Body).Text(student.Name ?? string.Empty);
                table.Cell().Background(bg).Element(Body).Text(string.Empty);
                table.Cell().Background(bg).Element(Body).Text(string.Empty);
            }
        });
    }

    private void Footer(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(18).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().PaddingTop(14).LineHorizontal(1).LineColor(Colors.Black);
                    c.Item().PaddingTop(2).Text("Signature of Exam Teacher").FontSize(9);
                    if (!string.IsNullOrWhiteSpace(_s.TeacherName))
                        c.Item().Text($"({_s.TeacherName})").FontSize(8);
                });

                row.ConstantItem(20);

                row.RelativeItem().Column(c =>
                {
                    if (_s.Signature != null && _s.Signature.Length > 0)
                    {
                        try { c.Item().AlignCenter().Height(28).Image(_s.Signature).FitArea(); }
                        catch { /* ignore bad signature */ }
                    }
                    else
                    {
                        c.Item().PaddingTop(14).LineHorizontal(1).LineColor(Colors.Black);
                    }
                    c.Item().PaddingTop(2).Text("Signature of Head Teacher").FontSize(9);
                });

                row.ConstantItem(20);

                row.RelativeItem().Column(c =>
                {
                    c.Item().PaddingTop(14).LineHorizontal(1).LineColor(Colors.Black);
                    c.Item().PaddingTop(2).Text("Date").FontSize(9);
                });
            });

            col.Item().PaddingTop(10).AlignRight().Text(x =>
            {
                x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
                x.Span("   |   Generated: ").Light();
                x.Span(DateTime.Now.ToString("dd MMM yyyy hh:mm tt")).Light();
            });
        });
    }
}
