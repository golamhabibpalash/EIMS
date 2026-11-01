using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS_App.ViewModels.Students;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Renders printable student ID cards as a PDF. One FRONT card is produced for every student,
/// tiled in a grid; the common BACK is printed only ONCE, on its own page at the end.
///
/// IMPORTANT: no element inside a card may use <c>Extend()</c>/<c>ExtendVertical()</c>. A fixed
/// height is applied to each card slot, and Extend inside a fixed-height slot makes QuestPDF
/// expand it to the full page - which previously pushed the QR onto a new page and left a ghost
/// blank card. Content is laid out top-to-bottom and is sized to fit the slot instead.
///
/// Three fixed designs are selectable; they differ only by colour palette (same layout), so the
/// output stays predictable and matches the HTML cards on Views/Students/IdCards.cshtml.
/// </summary>
public class StudentIdCardPdfBuilder : IDocument
{
    private const float CardWidthInches = 2.4f;
    // 3.8in slot leaves comfortable headroom over the front content (~2.5in tall) so nothing
    // spills onto a new page. Two rows of cards fit an A4 column; the rest flow to the next page.
    private const float CardHeightInches = 3.8f;
    private const int ColumnsPerRow = 3;

    private const string Ink = "#1F2A37";
    private const string Muted = "#5B6B7B";
    private const string CardBorder = "#D0D7DE";

    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly List<StudentIdCardVM> _cards;
    private readonly string _primary;
    private readonly string _accent;
    private readonly string _light;

    private static string ToTitle(string s) =>
        string.IsNullOrWhiteSpace(s) ? ""
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    public StudentIdCardPdfBuilder(Institute institute, byte[] logo, byte[] signature,
        List<StudentIdCardVM> cards, IdCardDesign design = IdCardDesign.Blue)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _cards = cards ?? new List<StudentIdCardVM>();

        (_primary, _accent, _light) = design switch
        {
            IdCardDesign.Green => ("#0E9F6E", "#34D399", "#E7F7F0"),
            IdCardDesign.Maroon => ("#9D174D", "#DB2777", "#FCE7F0"),
            _ => ("#1565C0", "#2A9FD6", "#E8F3FB"),
        };
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0.3f, Unit.Inch);
            page.DefaultTextStyle(x => x.FontSize(8));

            page.Content().Column(col =>
            {
                col.Spacing(10);

                // All fronts, tiled ColumnsPerRow across. A row with fewer than ColumnsPerRow
                // cards is padded with empty slots so widths stay aligned.
                foreach (var chunk in _cards.Chunk(ColumnsPerRow))
                {
                    col.Item().Row(row =>
                    {
                        for (int i = 0; i < ColumnsPerRow; i++)
                        {
                            if (i > 0) row.ConstantItem(0.1f, Unit.Inch);
                            if (i < chunk.Length)
                            {
                                var card = chunk[i];
                                row.ConstantItem(CardWidthInches, Unit.Inch)
                                    .Height(CardHeightInches, Unit.Inch)
                                    .Element(c => ComposeFront(c, card));
                            }
                            else
                            {
                                row.ConstantItem(CardWidthInches, Unit.Inch);
                            }
                        }
                    });
                }

                // The common back, printed once on its own page.
                col.Item().PageBreak();
                col.Item().Row(row =>
                {
                    row.ConstantItem(CardWidthInches, Unit.Inch)
                        .Height(CardHeightInches, Unit.Inch)
                        .Element(ComposeBack);
                });
            });
        });
    }

    private void ComposeFront(IContainer container, StudentIdCardVM card)
    {
        // Clean white (common) card - the front CONTENT identifies the student.
        container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White).Column(col =>
        {
            ComposeHeader(col);

            // Photo.
            col.Item().PaddingTop(8).AlignCenter().Width(1.0f, Unit.Inch).Height(1.1f, Unit.Inch)
                .Border(2).BorderColor(_accent).Background(_light)
                .Element(c =>
                {
                    if (!string.IsNullOrEmpty(card.PhotoPath) && File.Exists(card.PhotoPath))
                    {
                        try { c.Image(card.PhotoPath).FitArea(); return; } catch { }
                    }
                    c.AlignCenter().AlignMiddle().Text("No Photo").FontColor(Colors.Grey.Medium).FontSize(7);
                });

            // Name + class.
            col.Item().PaddingTop(5).AlignCenter().Text(ToTitle(card.Name)).Bold().FontSize(10.5f).FontColor(Ink);
            col.Item().AlignCenter().Text($"{card.ClassName}{(string.IsNullOrEmpty(card.SectionName) ? "" : " - " + card.SectionName)}")
                .FontSize(8).FontColor(Muted);

            // Detail rows.
            col.Item().PaddingTop(5).PaddingHorizontal(8).Column(info =>
            {
                info.Spacing(1.5f);
                InfoRow(info, "ID NO", card.UniqueId);
                InfoRow(info, "Roll", card.ClassRoll.ToString());
                InfoRow(info, "Blood", card.BloodGroup);
                InfoRow(info, "Phone", card.PhoneNo);
            });

            // QR follows the detail rows (NOT bottom-pinned - see class remarks).
            col.Item().PaddingTop(5).AlignCenter().Column(c =>
            {
                if (card.QrPng != null)
                {
                    try { c.Item().AlignCenter().Width(46).Height(46).Image(card.QrPng).FitArea(); } catch { }
                }
            });
        });
    }

    private void ComposeBack(IContainer container)
    {
        // Common white back - identical for every card (no student-specific identity).
        container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White).Column(col =>
        {
            ComposeHeader(col);

            col.Item().PaddingTop(6).AlignCenter().Text("Terms & Conditions").FontColor(_primary).Bold().FontSize(9);
            col.Item().PaddingHorizontal(8).PaddingTop(4).Column(c =>
            {
                c.Spacing(3);
                foreach (var line in BackInstructions)
                    c.Item().Text("• " + line).FontColor(Ink).FontSize(6.5f);
            });

            col.Item().PaddingHorizontal(8).PaddingTop(6).Column(c =>
            {
                c.Spacing(1);
                if (!string.IsNullOrWhiteSpace(_institute?.Phone1))
                    c.Item().Text($"Phone: {_institute.Phone1}").FontColor(Ink).FontSize(7);
                if (!string.IsNullOrWhiteSpace(_institute?.Email))
                    c.Item().Text($"E-mail: {_institute.Email}").FontColor(Ink).FontSize(7);
                if (!string.IsNullOrWhiteSpace(_institute?.Address))
                    c.Item().Text($"Address: {_institute.Address}").FontColor(Ink).FontSize(7);
            });

            col.Item().PaddingTop(8).Column(c =>
            {
                if (_signature != null)
                {
                    try { c.Item().AlignCenter().Height(22).Image(_signature).FitArea(); } catch { }
                }
                c.Item().PaddingTop(2).AlignCenter().Text("Authorised Signature").FontColor(Muted).FontSize(6.5f);
                if (!string.IsNullOrWhiteSpace(_institute?.Slogan))
                    c.Item().PaddingTop(3).AlignCenter().Text(_institute.Slogan).FontColor(_primary).FontSize(6.5f);
            });
        });
    }

    /// <summary>Common institute header strip shared by both faces.</summary>
    private void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().Background(_primary).Padding(5).Row(r =>
        {
            if (_logo != null)
            {
                try { r.ConstantItem(22).Image(_logo).FitArea(); } catch { }
                r.ConstantItem(4);
            }
            r.RelativeItem().AlignMiddle().Text(_institute?.Name ?? "").FontColor(Colors.White).Bold().FontSize(8f);
        });
    }

    private void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(44).Text(label).SemiBold().FontSize(7.5f).FontColor(_primary);
            r.AutoItem().Text(":").FontSize(7.5f).FontColor(Ink);
            r.RelativeItem().PaddingLeft(3).Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(7.5f).FontColor(Ink);
        });
    }

    /// <summary>Standard card-back instructions. Kept here so both faces stay in one place.</summary>
    private static readonly string[] BackInstructions =
    {
        "This card is the property of the institute and is non-transferable.",
        "The holder must carry this card at all times within the campus.",
        "If found, please return this card to the institute office.",
        "Loss of this card must be reported to the office immediately.",
    };
}
