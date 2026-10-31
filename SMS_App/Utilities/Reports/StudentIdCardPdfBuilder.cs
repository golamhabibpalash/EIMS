using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS_App.ViewModels.Students;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Renders printable student ID cards as a PDF. Each student gets a front and a back face laid
/// out side by side so they can be printed on one sheet and cut apart (or laminated back-to-back).
/// Several students flow down the page and QuestPDF breaks to a new page automatically.
///
/// This is the deliberately-simpler sibling of the HTML card (Views/Students/IdCards.cshtml):
/// QuestPDF cannot reproduce the demo's organic curved shapes, so the design here is reduced to
/// flat colour bands while keeping the same fields and layout order.
/// </summary>
public class StudentIdCardPdfBuilder : IDocument
{
    // Portrait card roughly the proportions of the lanyard card in the demo.
    private const float CardWidthInches = 2.45f;
    private const float CardHeightInches = 3.85f;

    // Clean palette: a single accent on an otherwise white (common) card.
    private const string PrimaryBlue = "#1565C0";
    private const string AccentBlue = "#2A9FD6";
    private const string LightBlue = "#E8F3FB";
    private const string Ink = "#1F2A37";
    private const string Muted = "#5B6B7B";
    private const string CardBorder = "#D0D7DE";

    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly List<StudentIdCardVM> _cards;

    private static string ToTitle(string s) =>
        string.IsNullOrWhiteSpace(s) ? ""
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    public StudentIdCardPdfBuilder(Institute institute, byte[] logo, byte[] signature,
        List<StudentIdCardVM> cards)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _cards = cards ?? new List<StudentIdCardVM>();
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
                col.Spacing(12);
                foreach (var card in _cards)
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(CardWidthInches, Unit.Inch)
                            .Height(CardHeightInches, Unit.Inch)
                            .Element(c => ComposeFront(c, card));
                        row.ConstantItem(0.2f, Unit.Inch);
                        row.AutoItem().Width(CardWidthInches, Unit.Inch)
                            .Height(CardHeightInches, Unit.Inch)
                            .Element(c => ComposeBack(c, card));
                    });
                }
            });
        });
    }

    private void ComposeFront(IContainer container, StudentIdCardVM card)
    {
        // Clean white (common) card - the front CONTENT is what identifies the student.
        container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White).Column(col =>
        {
            // Common header strip: institute identity (same on every card).
            col.Item().Background(PrimaryBlue).Padding(5).Row(r =>
            {
                if (_logo != null)
                {
                    try { r.ConstantItem(24).Image(_logo).FitArea(); } catch { }
                    r.ConstantItem(4);
                }
                r.RelativeItem().AlignMiddle().Text(_institute?.Name ?? "").FontColor(Colors.White).Bold().FontSize(8.5f);
            });

            // Photo.
            col.Item().PaddingTop(10).AlignCenter().Width(1.15f, Unit.Inch).Height(1.35f, Unit.Inch)
                .Border(2).BorderColor(AccentBlue).Background(LightBlue)
                .Element(c =>
                {
                    if (!string.IsNullOrEmpty(card.PhotoPath) && File.Exists(card.PhotoPath))
                    {
                        try { c.Image(card.PhotoPath).FitArea(); return; } catch { }
                    }
                    c.AlignCenter().AlignMiddle().Text("No Photo").FontColor(Colors.Grey.Medium).FontSize(7);
                });

            // Name + class.
            col.Item().PaddingTop(6).AlignCenter().Text(ToTitle(card.Name)).Bold().FontSize(11).FontColor(Ink);
            col.Item().AlignCenter().Text($"{card.ClassName}{(string.IsNullOrEmpty(card.SectionName) ? "" : " - " + card.SectionName)}")
                .FontSize(8).FontColor(Muted);

            // Detail rows.
            col.Item().PaddingTop(6).PaddingHorizontal(8).Column(info =>
            {
                info.Spacing(2);
                InfoRow(info, "ID NO", card.UniqueId);
                InfoRow(info, "Roll", card.ClassRoll.ToString());
                InfoRow(info, "Blood", card.BloodGroup);
                InfoRow(info, "Phone", card.PhoneNo);
                InfoRow(info, "Session", card.SessionName);
            });

            // QR pinned to the bottom.
            col.Item().PaddingTop(4).Extend().AlignBottom().AlignCenter().PaddingBottom(6).Column(c =>
            {
                if (card.QrPng != null)
                {
                    try { c.Item().AlignCenter().Width(50).Height(50).Image(card.QrPng).FitArea(); } catch { }
                }
            });
        });
    }

    private void ComposeBack(IContainer container, StudentIdCardVM card)
    {
        // Common white back - identical for every card (no student-specific identity).
        container.Border(0.75f).BorderColor(CardBorder).Background(Colors.White).Column(col =>
        {
            col.Item().Background(PrimaryBlue).Padding(5).Row(r =>
            {
                if (_logo != null)
                {
                    try { r.ConstantItem(24).Image(_logo).FitArea(); } catch { }
                    r.ConstantItem(4);
                }
                r.RelativeItem().AlignMiddle().Text(_institute?.Name ?? "").FontColor(Colors.White).Bold().FontSize(8.5f);
            });

            col.Item().PaddingTop(8).AlignCenter().Text("Terms & Conditions").FontColor(PrimaryBlue).Bold().FontSize(9);
            col.Item().PaddingHorizontal(8).PaddingTop(4).Column(c =>
            {
                c.Spacing(3);
                foreach (var line in BackInstructions)
                    c.Item().Text("• " + line).FontColor(Ink).FontSize(6.5f);
            });

            col.Item().PaddingHorizontal(8).PaddingTop(8).Column(c =>
            {
                c.Spacing(1);
                if (!string.IsNullOrWhiteSpace(_institute?.Phone1))
                    c.Item().Text($"Phone: {_institute.Phone1}").FontColor(Ink).FontSize(7);
                if (!string.IsNullOrWhiteSpace(_institute?.Email))
                    c.Item().Text($"E-mail: {_institute.Email}").FontColor(Ink).FontSize(7);
                if (!string.IsNullOrWhiteSpace(_institute?.Address))
                    c.Item().Text($"Address: {_institute.Address}").FontColor(Ink).FontSize(7);
            });

            // Signature + slogan hug the bottom.
            col.Item().Extend().AlignBottom().PaddingBottom(8).Column(c =>
            {
                if (_signature != null)
                {
                    try { c.Item().AlignCenter().Height(24).Image(_signature).FitArea(); } catch { }
                }
                c.Item().PaddingTop(2).AlignCenter().Text("Authorised Signature").FontColor(Muted).FontSize(6.5f);
                if (!string.IsNullOrWhiteSpace(_institute?.Slogan))
                    c.Item().PaddingTop(3).AlignCenter().Text(_institute.Slogan).FontColor(PrimaryBlue).FontSize(6.5f);
            });
        });
    }

    private static void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(46).Text(label).SemiBold().FontSize(7.5f).FontColor(PrimaryBlue);
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
