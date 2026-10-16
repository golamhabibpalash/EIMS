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

/// <summary>Paper the admit cards are tiled onto before being cut apart.</summary>
public enum AdmitCardSheet
{
    Legal,
    A4,

    /// <summary>
    /// A4 paper carrying three cards instead of two. The card keeps the full A4 width
    /// but is scaled down vertically so three fit on one sheet (A4 is 11.69in tall, so
    /// each card gets a third of that). Everything on the card shrinks by <see cref="_scale"/>.
    /// </summary>
    A4ThreePerPage,
}

public class AdmitCardPdfBuilder : IDocument
{
    // Cards are NOT one-per-page. They are tiled onto a real paper sheet and cut
    // apart, because a printer fed with Legal/A4 paper rescales a small 8.5x4.5
    // page to fit the sheet - which is what made printed cards come out undersized.
    //
    // Card height is normally 4.5in. Card width follows the sheet width, so Legal
    // gives the full 8.5in; A4 paper is only 8.27in wide and physically cannot
    // carry an 8.5in card, so on A4 the card is 8.27in wide. The A4ThreePerPage
    // mode keeps the 8.27in width but shrinks the card height to a third of the A4
    // sheet and scales the card's contents to match.
    private const float CardHeightInches = 4.5f;

    private const float LegalWidthInches = 8.5f;
    private const float LegalHeightInches = 14f;
    private const float A4WidthInches = 8.26772f;   // 210mm
    private const float A4HeightInches = 11.69291f; // 297mm

    private const float InstituteNameFont = 16f;
    private const float InstituteMetaFont = 10f;
    private const float TitleFont = 17f;
    private const float ExamInfoFont = 12f;
    private const float LabelFont = 11f;
    private const float ValueFont = 10f;
    private const float SubjectFont = 9.5f;
    private const float DirectionFont = 8f;

    private readonly Institute _institute;
    private readonly string _logoBase64;
    private readonly string _signatureBase64;
    private readonly List<RptAdmitCardVM> _data;
    private readonly AdmitCardSheet _sheet;

    // Multiplies every font, padding and fixed size on the card. 1.0 for the Legal/A4
    // layouts (so their output is unchanged); less than 1 for the 3-per-A4 layout, which
    // has to fit the same card design into two thirds of the former vertical space.
    private readonly float _scale;

    /// <summary>Scales a font/padding/fixed dimension by the card's scale factor.</summary>
    private float Sz(float value) => value * _scale;

    private static string ToTitle(string s) =>
        string.IsNullOrWhiteSpace(s) ? ""
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    public AdmitCardPdfBuilder(Institute institute, string logoBase64, string signatureBase64,
        List<RptAdmitCardVM> data, AdmitCardSheet sheet = AdmitCardSheet.Legal)
    {
        _institute = institute;
        _logoBase64 = logoBase64;
        _signatureBase64 = signatureBase64;
        _data = data;
        _sheet = sheet;

        // Three cards share an 11.69in sheet (3.9in each) instead of 4.5in, i.e. ~0.87
        // of the height; scale a touch harder so the content clears its slot cleanly.
        _scale = sheet == AdmitCardSheet.A4ThreePerPage ? 0.84f : 1f;
    }

    // Legal fits 3 cards (13.5in of 14in); A4 fits 2 (9in of 11.69in); A4ThreePerPage
    // fits 3 by giving each card exactly a third of the A4 height.
    private (float Width, float Height, int CardsPerSheet, float CardHeight) SheetSpec => _sheet switch
    {
        AdmitCardSheet.A4ThreePerPage => (A4WidthInches, A4HeightInches, 3, A4HeightInches / 3f),
        AdmitCardSheet.A4 => (A4WidthInches, A4HeightInches,
            Math.Max(1, (int)Math.Floor(A4HeightInches / CardHeightInches)), CardHeightInches),
        _ => (LegalWidthInches, LegalHeightInches,
            Math.Max(1, (int)Math.Floor(LegalHeightInches / CardHeightInches)), CardHeightInches),
    };

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        var students = _data
            .GroupBy(d => d.StudentId)
            .Select(g => (Student: g.First(), Subjects: g.ToList()))
            .ToList();

        var spec = SheetSpec;

        foreach (var sheet in students.Chunk(spec.CardsPerSheet))
        {
            container.Page(page =>
            {
                page.Size(spec.Width, spec.Height, Unit.Inch);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(Sz(ValueFont)));

                page.Content().Column(col =>
                {
                    foreach (var (student, subjects) in sheet)
                    {
                        col.Item()
                            .Height(spec.CardHeight, Unit.Inch)
                            .Element(c => ComposeCard(c, student, subjects));
                    }
                });
            });
        }
    }

    // Renders one card into a full-width, fixed-height slot on the sheet.
    // Nothing here may use ExtendVertical: inside a fixed-height slot it expands to
    // the whole page and pushes every card onto its own sheet.
    private void ComposeCard(IContainer container, RptAdmitCardVM student, List<RptAdmitCardVM> subjects)
    {
        container.Padding(Sz(14)).Border(1).Padding(Sz(4)).Column(card =>
        {
            ComposeBody(card, student, subjects);

            // Both portions hug the bottom of the row so the last direction line and
            // the "Controller of Examinations" line share a baseline regardless of
            // how many lines the directions wrap to.
            card.Item().PaddingTop(Sz(4)).Row(bottomRow =>
            {
                bottomRow.RelativeItem(3).AlignBottom().Column(directions =>
                {
                    directions.Item().Text("Direction:").SemiBold().FontSize(Sz(DirectionFont));
                    directions.Item().Text("1. The Examinee must bring the Admit Card in the Examination hall.").FontSize(Sz(DirectionFont));
                    directions.Item().Text("2. The examinee must sign in the attendance sheet for each subject in the examination hall otherwise will be treated as absent in the respective subject(s).").FontSize(Sz(DirectionFont));
                });

                bottomRow.RelativeItem(2).AlignBottom().AlignRight().Column(sig =>
                {
                    if (!string.IsNullOrEmpty(_signatureBase64))
                    {
                        try
                        {
                            sig.Item().Height(Sz(38)).AlignRight().Image(
                                Convert.FromBase64String(_signatureBase64)).FitArea();
                        }
                        catch { }
                    }
                    sig.Item().AlignCenter().Text("Controller of Examinations").SemiBold().FontSize(Sz(DirectionFont));
                    sig.Item().AlignCenter().Text(_institute.Name ?? "").FontSize(Sz(DirectionFont));
                });
            });
        });
    }

    private void ComposeBody(ColumnDescriptor col, RptAdmitCardVM student, List<RptAdmitCardVM> subjects)
    {
        void InfoCell(IContainer c, string label, string value)
        {
            c.Padding(Sz(1.5f)).Text(t =>
            {
                t.Span(label).SemiBold().FontSize(Sz(LabelFont));
                t.Span(value).FontSize(Sz(ValueFont));
            });
        }

        // Header: logo + institute
        col.Item().Row(headerRow =>
        {
            if (!string.IsNullOrEmpty(_logoBase64))
            {
                try { headerRow.ConstantItem(Sz(50)).Image(Convert.FromBase64String(_logoBase64)).FitArea(); } catch { }
            }
            headerRow.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text(_institute.Name ?? "").Bold().FontSize(Sz(InstituteNameFont));
                if (!string.IsNullOrEmpty(_institute.Address))
                    c.Item().AlignCenter().Text(_institute.Address).FontSize(Sz(InstituteMetaFont));
                if (!string.IsNullOrEmpty(_institute.EIIN))
                    c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(Sz(InstituteMetaFont));
            });
        });

        // Title + exam info on same row
        col.Item().Row(titleRow =>
        {
            titleRow.RelativeItem(2).AlignCenter().Text("ADMIT CARD").Bold().FontSize(Sz(TitleFont));
            titleRow.RelativeItem(3).AlignCenter().Text($"{student.ExamGroupName ?? student.ExamTypeName}  |  {student.SessionName}").FontSize(Sz(ExamInfoFont));
        });

        col.Item().LineHorizontal(1);

        // Name fields in 2-column table (wider for long names)
        col.Item().PaddingVertical(Sz(1.5f)).Table(nameTable =>
        {
            nameTable.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
            });

            nameTable.Cell().Element(c => InfoCell(c, "Student Name: ", ToTitle(student.StudentName)));
            nameTable.Cell().Element(c => InfoCell(c, "Father's Name: ", ToTitle(student.FatherName)));
            nameTable.Cell().Element(c => InfoCell(c, "Mother's Name: ", ToTitle(student.MotherName)));
            nameTable.Cell().Element(c => InfoCell(c, "Gender: ", student.Gender ?? ""));
        });

        // Roll is displayed as 6 digits (the leading digit of the stored class roll is
        // dropped); the Registration Number is a fixed 10 digits, built as
        // "20" + the full class roll + "0".
        var classRollDigits = student.ClassRoll.ToString("D7", CultureInfo.InvariantCulture);
        var rollDisplay = classRollDigits.Length > 6
            ? classRollDigits[^6..]
            : classRollDigits;
        var registrationNo = $"20{classRollDigits}0";

        // Other fields in 4-column table
        col.Item().Table(otherTable =>
        {
            otherTable.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.4f);
                c.RelativeColumn();
                c.RelativeColumn(1.4f);
                c.RelativeColumn();
            });

            otherTable.Cell().Element(c => InfoCell(c, "Class: ", $"{student.ClassName} - {student.SectionName}"));
            otherTable.Cell().Element(c => InfoCell(c, "Roll: ", rollDisplay));
            otherTable.Cell().Element(c => InfoCell(c, "Reg. No: ", registrationNo));
            otherTable.Cell().Element(c => InfoCell(c, "Religion: ", student.Religion ?? ""));
        });

        col.Item().LineHorizontal(1);

        // Subjects in 4-column table. The card must never spill onto a second page,
        // so tighten the rows as the subject count grows.
        // The A4 layouts are 0.23in narrower than Legal, so subject names wrap an extra
        // line there; tighten one step earlier to keep the card inside its slot.
        var rowCount = (int)Math.Ceiling(subjects.Count / 4.0);
        var density = rowCount + (_sheet == AdmitCardSheet.Legal ? 0 : 1);
        var (subjectFont, subjectPadding) = density switch
        {
            <= 3 => (Sz(SubjectFont), Sz(1.5f)),
            4 => (Sz(8f), Sz(1f)),
            5 => (Sz(7f), Sz(0.75f)),
            _ => (Sz(6f), Sz(0.5f)),
        };

        col.Item().Text("Subjects:").SemiBold().FontSize(Sz(LabelFont));
        col.Item().PaddingTop(Sz(1.5f)).Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
            });

            for (int i = 0; i < subjects.Count; i += 4)
            {
                for (int j = 0; j < 4; j++)
                {
                    var idx = i + j;
                    if (idx < subjects.Count)
                    {
                        var bg = (i / 4) % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;
                        IContainer D(IContainer c) => c.Background(bg).Padding(subjectPadding).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                        t.Cell().Element(D).AlignLeft().Text(txt =>
                        {
                            txt.Span(subjects[idx].SubjectCode?.ToString() ?? "").SemiBold().FontSize(subjectFont);
                            txt.Span(" ").FontSize(subjectFont);
                            txt.Span(subjects[idx].SubjectName ?? "").FontSize(subjectFont);
                        });
                    }
                    else
                    {
                        t.Cell().Text("");
                    }
                }
            }
        });
    }
}
