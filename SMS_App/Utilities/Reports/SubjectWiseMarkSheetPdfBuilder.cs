using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels.Results;

namespace SMS_App.Utilities.Reports;

public class SubjectWiseMarkSheetPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<SubjectWiseMarkSheetVM> _data;
    private readonly string _className;
    private readonly string _subjectName;
    private readonly string _examGroupName;
    private readonly string _teacherName;
    private readonly string _session;
    private readonly int _totalMarks;

    public SubjectWiseMarkSheetPdfBuilder(
        Institute institute,
        string logoBase64,
        List<SubjectWiseMarkSheetVM> data,
        string className,
        string subjectName,
        string examGroupName,
        string teacherName,
        string session,
        int totalMarks)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _data = data;
        _className = className;
        _subjectName = subjectName;
        _examGroupName = examGroupName;
        _teacherName = teacherName;
        _session = session;
        _totalMarks = totalMarks;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(9));
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
                    c.Item().AlignCenter().Text("Subject-wise Mark Sheet").Bold().FontSize(12);
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(1);

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                t.Cell().Text("Class:").SemiBold(); t.Cell().Text(_className);
                t.Cell().Text("Subject:").SemiBold(); t.Cell().Text(_subjectName);
                t.Cell().Text("Exam:").SemiBold(); t.Cell().Text(_examGroupName);
                t.Cell().Text("Session:").SemiBold(); t.Cell().Text(_session);
                t.Cell().Text("Teacher:").SemiBold(); t.Cell().Text(_teacherName);
                t.Cell().Text("Total Marks:").SemiBold(); t.Cell().Text(_totalMarks.ToString());
            });

            col.Item().PaddingTop(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(35);
                c.ConstantColumn(55);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
            });

            static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(4).AlignCenter();

            table.Header(h =>
            {
                h.Cell().Element(H).Text("SL").Bold();
                h.Cell().Element(H).Text("Roll").Bold();
                h.Cell().Element(H).Text("Student Name").Bold();
                h.Cell().Element(H).Text("Obtained").Bold();
                h.Cell().Element(H).Text("Grade").Bold();
                h.Cell().Element(H).Text("Remarks").Bold();
            });

            var ordered = _data.OrderBy(d => d.ClassRoll).ToList();
            int sl = 1;
            foreach (var d in ordered)
            {
                var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                IContainer D(IContainer c) => c.Background(bg).Padding(4);

                table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                table.Cell().Element(D).AlignCenter().Text(d.ClassRoll.ToString());
                table.Cell().Element(D).Text(d.Name ?? "");
                table.Cell().Element(D).AlignCenter().Text(d.ObtainMark.ToString("N1"));
                table.Cell().Element(D).AlignCenter().Text(d.LetterGrade ?? "");
                table.Cell().Element(D).Text(d.Remarks ?? "");
                sl++;
            }
        });
    }
}
