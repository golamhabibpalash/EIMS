using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS.Entities.RptModels;

namespace SMS_App.Utilities.Reports;

/// <summary>Paper the seat-plan cards are tiled onto before being cut apart.</summary>
public enum SeatPlanSheet
{
    Legal,
    A4,
}

/// <summary>
/// Renders small 3in x 2in seat-plan cards tiled two-across onto a Legal or A4 sheet.
/// Every card is bordered and the cards sit flush against one another so the border
/// lines form a continuous grid the user can cut along.
/// </summary>
public class SeatPlanPdfBuilder : IDocument
{
    // Card physical size is fixed by requirement: 3in wide x 2in tall.
    private const float CardWidthInches = 3f;
    private const float CardHeightInches = 2f;
    private const int Columns = 2; // two 3in cards = 6in, fits both Legal (8.5) and A4 (8.27)

    private const float LegalWidthInches = 8.5f;
    private const float LegalHeightInches = 14f;
    private const float A4WidthInches = 8.26772f;   // 210mm
    private const float A4HeightInches = 11.69291f; // 297mm

    private const float InstituteFont = 9f;
    private const float ExamFont = 7.5f;
    private const float TitleFont = 9f;
    private const float LabelFont = 8f;
    private const float ValueFont = 8f;

    private readonly Institute _institute;
    private readonly List<RptAdmitCardVM> _data;
    private readonly SeatPlanSheet _sheet;

    private static string ToTitle(string s) =>
        string.IsNullOrWhiteSpace(s) ? ""
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    public SeatPlanPdfBuilder(Institute institute, List<RptAdmitCardVM> data,
        SeatPlanSheet sheet = SeatPlanSheet.Legal)
    {
        _institute = institute;
        _data = data;
        _sheet = sheet;
    }

    private (float Width, float Height) SheetSpec => _sheet switch
    {
        SeatPlanSheet.A4 => (A4WidthInches, A4HeightInches),
        _ => (LegalWidthInches, LegalHeightInches),
    };

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        // One card per student (GetAdmitCard returns one row per subject).
        var students = _data
            .GroupBy(d => d.StudentId)
            .Select(g => g.First())
            .ToList();

        var spec = SheetSpec;

        // Centre the 6in-wide grid on the sheet; a small vertical margin.
        float horizontalMargin = Math.Max(0f, (spec.Width - CardWidthInches * Columns) / 2f);

        container.Page(page =>
        {
            page.Size(spec.Width, spec.Height, Unit.Inch);
            page.MarginVertical(0.25f, Unit.Inch);
            page.MarginHorizontal(horizontalMargin, Unit.Inch);
            page.DefaultTextStyle(x => x.FontSize(ValueFont));

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    for (int i = 0; i < Columns; i++)
                        c.ConstantColumn(CardWidthInches, Unit.Inch);
                });

                foreach (var student in students)
                {
                    table.Cell()
                        .Height(CardHeightInches, Unit.Inch)
                        .Border(1)
                        .Element(c => ComposeCard(c, student));
                }
            });
        });
    }

    private void ComposeCard(IContainer container, RptAdmitCardVM student)
    {
        container.Padding(5).Column(card =>
        {
            // Header: Institute Name, Exam Name, "SEAT PLAN"
            card.Item().AlignCenter().Text(student.InstituteName ?? _institute?.Name ?? "")
                .Bold().FontSize(InstituteFont);

            var examName = student.ExamGroupName ?? student.ExamTypeName ?? "";
            if (!string.IsNullOrWhiteSpace(examName))
                card.Item().AlignCenter().Text(examName).FontSize(ExamFont);

            card.Item().AlignCenter().Text("SEAT PLAN").Bold().FontSize(TitleFont);

            card.Item().PaddingVertical(2).LineHorizontal(0.75f);

            void InfoLine(string label, string value) =>
                card.Item().Text(t =>
                {
                    t.Span(label).SemiBold().FontSize(LabelFont);
                    t.Span(value).FontSize(ValueFont);
                });

            // Roll is displayed as 6 digits (the leading digit of the stored 7-digit
            // class roll is dropped), matching the Admit Card.
            var classRollDigits = student.ClassRoll.ToString("D7", CultureInfo.InvariantCulture);
            var rollDisplay = classRollDigits.Length > 6 ? classRollDigits[^6..] : classRollDigits;

            InfoLine("Name: ", ToTitle(student.StudentName));
            InfoLine("Roll: ", rollDisplay);
            InfoLine("Class: ", $"{student.ClassName}{(string.IsNullOrWhiteSpace(student.SectionName) ? "" : " - " + student.SectionName)}");
        });
    }
}
