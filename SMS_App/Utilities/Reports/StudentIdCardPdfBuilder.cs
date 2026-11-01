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
/// Layout is chosen via <see cref="IdCardDesign"/> (Classic / Wave / SidePanel) and the accent
/// colour is supplied separately, so any layout can be rendered in any colour - matching the
/// HTML cards on Views/Students/IdCards.cshtml.
///
/// IMPORTANT: no element inside a card may use <c>Extend()</c>/<c>ExtendVertical()</c>. Each card
/// sits in a fixed-height slot, and Extend inside a fixed-height slot makes QuestPDF expand it to
/// the full page - which previously pushed the QR onto a new page and left a ghost blank card.
/// Content is laid out top-to-bottom and sized to fit the slot instead.
/// </summary>
public class StudentIdCardPdfBuilder : IDocument
{
    private const float CardWidthInches = 2.4f;
    // 3.8in slot leaves comfortable headroom over the front content so nothing spills onto a new
    // page. Two rows of cards fit an A4 column; the rest flow to the next page.
    private const float CardHeightInches = 3.8f;
    private const int ColumnsPerRow = 3;

    private const string Ink = "#1F2A37";
    private const string Muted = "#5B6B7B";
    private const string CardBorder = "#D0D7DE";
    private const string PhotoBg = "#EEF2F7";

    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly List<StudentIdCardVM> _cards;
    private readonly IdCardDesign _design;
    private readonly string _accent;

    private static string ToTitle(string s) =>
        string.IsNullOrWhiteSpace(s) ? ""
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    public StudentIdCardPdfBuilder(Institute institute, byte[] logo, byte[] signature,
        List<StudentIdCardVM> cards, IdCardDesign design = IdCardDesign.Classic, string accentColor = "#1565C0")
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _cards = cards ?? new List<StudentIdCardVM>();
        _design = design;
        _accent = string.IsNullOrWhiteSpace(accentColor) ? "#1565C0" : accentColor;
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

                // All fronts, tiled ColumnsPerRow across. Short rows are padded with empty slots.
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
        var framed = container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White);
        switch (_design)
        {
            case IdCardDesign.Wave: ComposeFrontWave(framed, card); break;
            case IdCardDesign.SidePanel: ComposeFrontSidePanel(framed, card); break;
            default: ComposeFrontClassic(framed, card); break;
        }
    }

    // ---- Design 1: Classic - thin accent header, round-ish photo, details below ----
    private void ComposeFrontClassic(IContainer container, StudentIdCardVM card)
    {
        container.Column(col =>
        {
            HeaderStrip(col);
            col.Item().PaddingTop(8).AlignCenter().Element(c => Photo(c, card, 1.0f, 1.1f));
            NameBlock(col, card);
            DetailRows(col, card);
            col.Item().PaddingTop(5).AlignCenter().Element(c => Qr(c, card, 46));
        });
    }

    // ---- Design 2: Wave - large accent top panel carrying logo/photo, details on white ----
    private void ComposeFrontWave(IContainer container, StudentIdCardVM card)
    {
        container.Column(col =>
        {
            // Accent top panel with the institute name and the photo sitting inside it.
            col.Item().Background(_accent).PaddingVertical(8).PaddingHorizontal(6).Column(top =>
            {
                top.Item().AlignCenter().Text(_institute?.Name ?? "").FontColor(Colors.White).Bold().FontSize(8);
                top.Item().PaddingTop(6).AlignCenter().Element(c => Photo(c, card, 1.0f, 1.1f, white: true));
            });

            NameBlock(col, card);
            DetailRows(col, card);
            col.Item().PaddingTop(5).AlignCenter().Element(c => Qr(c, card, 44));
        });
    }

    // ---- Design 3: SidePanel - accent column (photo + QR) on the left, details on the right ----
    private void ComposeFrontSidePanel(IContainer container, StudentIdCardVM card)
    {
        container.Row(row =>
        {
            // Left accent panel.
            row.ConstantItem(0.95f, Unit.Inch).Background(_accent).Padding(6).Column(side =>
            {
                side.Item().AlignCenter().Element(c => Photo(c, card, 0.78f, 0.9f, white: true));
                side.Item().PaddingTop(8).AlignCenter().Text(card.UniqueId ?? "").FontColor(Colors.White).Bold().FontSize(7);
                side.Item().PaddingTop(6).AlignCenter().Background(Colors.White).Padding(2).Element(c => Qr(c, card, 40));
            });

            // Right details panel.
            row.RelativeItem().Padding(8).Column(right =>
            {
                right.Item().Text(_institute?.Name ?? "").FontColor(_accent).Bold().FontSize(8);
                right.Item().PaddingTop(6).Text(ToTitle(card.Name)).Bold().FontSize(10).FontColor(Ink);
                right.Item().Text($"{card.ClassName}{(string.IsNullOrEmpty(card.SectionName) ? "" : " - " + card.SectionName)}")
                    .FontSize(8).FontColor(Muted);
                right.Item().PaddingTop(6).Column(info =>
                {
                    info.Spacing(2);
                    InfoRow(info, "Roll", card.ClassRoll.ToString());
                    InfoRow(info, "Blood", card.BloodGroup);
                    InfoRow(info, "Phone", card.PhoneNo);
                    InfoRow(info, "Session", card.SessionName);
                });
            });
        });
    }

    private void ComposeBack(IContainer container)
    {
        container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White).Column(col =>
        {
            HeaderStrip(col);

            col.Item().PaddingTop(6).AlignCenter().Text("Terms & Conditions").FontColor(_accent).Bold().FontSize(9);
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
                    c.Item().PaddingTop(3).AlignCenter().Text(_institute.Slogan).FontColor(_accent).FontSize(6.5f);
            });
        });
    }

    // ----------------------------------------------------------------------- shared pieces

    /// <summary>Common institute header strip (logo + name on an accent band).</summary>
    private void HeaderStrip(ColumnDescriptor col)
    {
        col.Item().Background(_accent).Padding(5).Row(r =>
        {
            if (_logo != null)
            {
                try { r.ConstantItem(22).Image(_logo).FitArea(); } catch { }
                r.ConstantItem(4);
            }
            r.RelativeItem().AlignMiddle().Text(_institute?.Name ?? "").FontColor(Colors.White).Bold().FontSize(8f);
        });
    }

    private void Photo(IContainer container, StudentIdCardVM card, float wInch, float hInch, bool white = false)
    {
        container.Width(wInch, Unit.Inch).Height(hInch, Unit.Inch)
            .Border(2).BorderColor(white ? "#FFFFFF" : _accent).Background(PhotoBg)
            .Element(c =>
            {
                if (!string.IsNullOrEmpty(card.PhotoPath) && File.Exists(card.PhotoPath))
                {
                    try { c.Image(card.PhotoPath).FitArea(); return; } catch { }
                }
                c.AlignCenter().AlignMiddle().Text("No Photo").FontColor(Colors.Grey.Medium).FontSize(7);
            });
    }

    private void NameBlock(ColumnDescriptor col, StudentIdCardVM card)
    {
        col.Item().PaddingTop(5).AlignCenter().Text(ToTitle(card.Name)).Bold().FontSize(10.5f).FontColor(Ink);
        col.Item().AlignCenter().Text($"{card.ClassName}{(string.IsNullOrEmpty(card.SectionName) ? "" : " - " + card.SectionName)}")
            .FontSize(8).FontColor(Muted);
    }

    private void DetailRows(ColumnDescriptor col, StudentIdCardVM card)
    {
        col.Item().PaddingTop(5).PaddingHorizontal(8).Column(info =>
        {
            info.Spacing(1.5f);
            InfoRow(info, "ID NO", card.UniqueId);
            InfoRow(info, "Roll", card.ClassRoll.ToString());
            InfoRow(info, "Blood", card.BloodGroup);
            InfoRow(info, "Phone", card.PhoneNo);
        });
    }

    private void Qr(IContainer container, StudentIdCardVM card, float size)
    {
        if (card.QrPng == null) { container.Width(size).Height(size); return; }
        try { container.Width(size).Height(size).Image(card.QrPng).FitArea(); }
        catch { container.Width(size).Height(size); }
    }

    private void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(44).Text(label).SemiBold().FontSize(7.5f).FontColor(_accent);
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
