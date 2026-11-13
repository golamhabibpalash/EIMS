using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels.AttendanceVM;

namespace SMS_App.Utilities.Reports;

public class MonthlyAttendancePdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<RptMonthlyAttendanceVM> _data;
    private readonly string _monthName;
    private readonly string _className;
    private readonly int _totalDays;

    public MonthlyAttendancePdfBuilder(
        Institute institute,
        string logoBase64,
        List<RptMonthlyAttendanceVM> data,
        string monthName,
        string className,
        int totalDays)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _data = data;
        _monthName = monthName;
        _className = className;
        _totalDays = totalDays;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A3.Landscape());
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(7));
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
                    try { row.ConstantItem(50).Image(Convert.FromBase64String(_logoBase64)).FitArea(); } catch { }
                }
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(13);
                    if (!string.IsNullOrEmpty(_institute.Address))
                        c.Item().AlignCenter().Text(_institute.Address).FontSize(8);
                    c.Item().AlignCenter().Text("Monthly Attendance Report").Bold().FontSize(11);
                    c.Item().AlignCenter().Text($"{_monthName}  |  Class: {_className}").FontSize(9);
                });
            });
            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        var days = Enumerable.Range(1, _totalDays).ToList();

        container.PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(25);
                c.ConstantColumn(40);
                c.RelativeColumn(3);
                foreach (var _ in days) c.ConstantColumn(14);
                c.ConstantColumn(25);
            });

            static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(2).AlignCenter();

            table.Header(h =>
            {
                h.Cell().Element(H).Text("SL").Bold();
                h.Cell().Element(H).Text("Roll").Bold();
                h.Cell().Element(H).Text("Name").Bold();
                foreach (var d in days)
                    h.Cell().Element(H).Text(d.ToString()).Bold();
                h.Cell().Element(H).Text("Total").Bold();
            });

            int sl = 1;
            foreach (var s in _data)
            {
                var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                IContainer D(IContainer c) => c.Background(bg).Padding(2);

                var dayValues = new[]
                {
                    s.Day1,s.Day2,s.Day3,s.Day4,s.Day5,s.Day6,s.Day7,
                    s.Day8,s.Day9,s.Day10,s.Day11,s.Day12,s.Day13,s.Day14,
                    s.Day15,s.Day16,s.Day17,s.Day18,s.Day19,s.Day20,s.Day21,
                    s.Day22,s.Day23,s.Day24,s.Day25,s.Day26,s.Day27,s.Day28,
                    s.Day29,s.Day30,s.Day31
                };

                int present = dayValues.Take(_totalDays).Count(v => v == "P");

                table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                table.Cell().Element(D).AlignCenter().Text(s.ClassRoll ?? "");
                table.Cell().Element(D).Text(s.StudentName ?? "");
                foreach (var d in days)
                {
                    var val = dayValues[d - 1];
                    var isPresent = val == "P";
                    table.Cell().Element(D).AlignCenter()
                        .Text(val ?? ".").FontColor(isPresent ? Colors.Green.Darken2 : Colors.Red.Medium);
                }
                table.Cell().Element(D).AlignCenter().Text(present.ToString()).Bold();
                sl++;
            }
        });
    }
}
