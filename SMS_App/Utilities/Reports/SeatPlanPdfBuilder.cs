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

    private const float InstituteFont = 11f;
    private const float ExamFont = 8f;
    private const float BannerFont = 11f;   // "SEAT PLAN" on the black banner
    private const float RollLabelFont = 8f; // "ROLL NO."
    private const float RollFont = 22f;     // the large roll number
    private const float NameFont = 11f;
    private const float ClassFont = 8.5f;

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
            page.DefaultTextStyle(x => x.FontSize(ClassFont));

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
                        // Specify the border colour explicitly: some QuestPDF versions default the
                        // border to a non-black/transparent colour, which renders the card grid
                        // invisible ("data shows but the boxes are gone") even though Border(1) is set.
                        .Border(1).BorderColor(Colors.Black)
                        .Element(c => ComposeCard(c, student));
                }
            });
        });
    }

    private void ComposeCard(IContainer container, RptAdmitCardVM student)
    {
        // Roll is displayed as 6 digits (the leading digit of the stored 7-digit
        // class roll is dropped), matching the Admit Card.
        var classRollDigits = student.ClassRoll.ToString("D7", CultureInfo.InvariantCulture);
        var rollDisplay = classRollDigits.Length > 6 ? classRollDigits[^6..] : classRollDigits;
        var classLine = $"Class: {student.ClassName}" +
            (string.IsNullOrWhiteSpace(student.SectionName) ? "" : " - " + student.SectionName);

        container.Padding(6).Column(card =>
        {
            card.Spacing(2);

            // Institute + exam, centred.
            card.Item().AlignCenter().Text(student.InstituteName ?? _institute?.Name ?? "")
                .Bold().FontSize(InstituteFont);

            var examName = student.ExamGroupName ?? student.ExamTypeName ?? "";
            if (!string.IsNullOrWhiteSpace(examName))
                card.Item().AlignCenter().Text(examName).FontSize(ExamFont);

            // "SEAT PLAN" as a white-on-black banner spanning the card width.
            card.Item().PaddingTop(1).Background(Colors.Black).PaddingVertical(3)
                .AlignCenter().Text("SEAT PLAN").FontColor(Colors.White).Bold().FontSize(BannerFont);

            // Roll number: small label over a large, bold, centred number.
            card.Item().PaddingTop(1).AlignCenter().Text("ROLL NO.").FontSize(RollLabelFont);
            card.Item().AlignCenter().Text(rollDisplay).Bold().FontSize(RollFont);

            // Student name (bold, centred) and class line.
            card.Item().AlignCenter().Text(ToTitle(student.StudentName)).Bold().FontSize(NameFont);
            card.Item().AlignCenter().Text(classLine).FontSize(ClassFont);
        });
    }
}
