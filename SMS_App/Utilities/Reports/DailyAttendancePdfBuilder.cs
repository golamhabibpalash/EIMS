using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels.AttendanceVM;

namespace SMS_App.Utilities.Reports;

public class DailyAttendancePdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly List<RptDailyAttendaceVM> _data;
    private readonly string _reportName;
    private readonly string _attendanceDate;
    private readonly bool _isEmployee;

    public DailyAttendancePdfBuilder(
        Institute institute,
        string logoBase64,
        List<RptDailyAttendaceVM> data,
        string reportName,
        string attendanceDate,
        bool isEmployee = false)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _data = data;
        _reportName = reportName;
        _attendanceDate = attendanceDate;
        _isEmployee = isEmployee;
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
            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Total: {_data.Count}");
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
                    c.Item().AlignCenter().Text(_reportName).Bold().FontSize(11);
                    c.Item().AlignCenter().Text($"Date: {_attendanceDate}").FontSize(9);
                });
            });
            col.Item().PaddingVertical(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Table(table =>
        {
            if (_isEmployee)
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(30);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(3).AlignCenter();
                table.Header(h =>
                {
                    h.Cell().Element(H).Text("SL").Bold();
                    h.Cell().Element(H).Text("Name / Designation").Bold();
                    h.Cell().Element(H).Text("Phone").Bold();
                    h.Cell().Element(H).Text("Punch Time").Bold();
                });

                int sl = 1;
                foreach (var d in _data)
                {
                    var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                    IContainer D(IContainer c) => c.Background(bg).Padding(3);
                    table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                    table.Cell().Element(D).Column(cc =>
                    {
                        cc.Item().Text(d.Name ?? "");
                        cc.Item().Text(d.Class_Designation ?? "").FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Element(D).Text(d.Phone ?? "");
                    table.Cell().Element(D).Text(d.PunchTime ?? "");
                    sl++;
                }
            }
            else
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(30);
                    c.ConstantColumn(45);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                static IContainer H(IContainer c) => c.Background(Colors.Grey.Lighten1).Padding(3).AlignCenter();
                table.Header(h =>
                {
                    h.Cell().Element(H).Text("SL").Bold();
                    h.Cell().Element(H).Text("Roll").Bold();
                    h.Cell().Element(H).Text("Name").Bold();
                    h.Cell().Element(H).Text("Class").Bold();
                    h.Cell().Element(H).Text("Phone").Bold();
                    h.Cell().Element(H).Text("Guardian").Bold();
                    h.Cell().Element(H).Text("Punch Time").Bold();
                });

                int sl = 1;
                foreach (var d in _data)
                {
                    var bg = sl % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                    IContainer D(IContainer c) => c.Background(bg).Padding(3);
                    table.Cell().Element(D).AlignCenter().Text(sl.ToString());
                    table.Cell().Element(D).AlignCenter().Text(d.ClassRoll ?? "");
                    table.Cell().Element(D).Text(d.Name ?? "");
                    table.Cell().Element(D).Text(d.Class_Designation ?? "");
                    table.Cell().Element(D).Text(d.Phone ?? "");
                    table.Cell().Element(D).Text(d.GuardianPhone ?? "");
                    table.Cell().Element(D).Text(d.PunchTime ?? "");
                    sl++;
                }
            }
        });
    }
}
