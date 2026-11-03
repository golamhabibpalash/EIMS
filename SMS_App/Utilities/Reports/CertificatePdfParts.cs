using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Shared building blocks for the on-demand student certificates (Testimonial, Transfer
/// Certificate): the decorative landscape-A4 frame, the institute header, the signatory footer,
/// and gender-aware pronoun resolution. Each certificate builder supplies only its title + body.
/// </summary>
public static class CertificatePdfParts
{
    public const string Ink = "#111111";

    /// <summary>Resolves subject/object/possessive pronouns from a gender name.</summary>
    public static (string Subject, string Object, string Possessive) Pronouns(string genderName)
    {
        var g = (genderName ?? "").Trim().ToLowerInvariant();
        bool female = g.StartsWith("f") || g.Contains("female") || g.Contains("মহিলা") || g.Contains("মেয়ে") || g.Contains("ছাত্রী");
        return female ? ("She", "her", "her") : ("He", "him", "his");
    }

    /// <summary>Institute identity block at the top of the certificate.</summary>
    public static void Header(ColumnDescriptor col, Institute inst, byte[] logo)
    {
        col.Item().Row(r =>
        {
            if (logo != null)
            {
                try { r.ConstantItem(72).Height(72).AlignMiddle().Image(logo).FitArea(); } catch { }
            }
            r.RelativeItem().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text(inst?.Name ?? "").Bold().FontSize(20).FontColor(Ink);
                if (!string.IsNullOrWhiteSpace(inst?.Address))
                    c.Item().AlignCenter().Text(inst.Address).FontSize(10);

                var contact = "";
                if (!string.IsNullOrWhiteSpace(inst?.Email)) contact += $"E-mail: {inst.Email}";
                if (!string.IsNullOrWhiteSpace(inst?.Phone1)) contact += (contact.Length > 0 ? ", " : "") + $"Mobile: {inst.Phone1}";
                if (contact.Length > 0) c.Item().AlignCenter().Text(contact).FontSize(9);

                if (!string.IsNullOrWhiteSpace(inst?.EIIN))
                    c.Item().AlignCenter().Text($"EIIN No: {inst.EIIN}").FontSize(9);
            });
            // Balance the row so the institute name stays centred when a logo is present.
            if (logo != null) r.ConstantItem(72);
        });
    }

    /// <summary>Issue date (left) and signatory block (right) pinned to the bottom of the card.</summary>
    public static void Footer(ColumnDescriptor col, Institute inst, byte[] signature,
        DateTime issueDate, string signatoryName, string signatoryDesignation)
    {
        var desig = string.IsNullOrWhiteSpace(signatoryDesignation) ? "Headmaster" : signatoryDesignation.Trim();

        col.Item().Row(r =>
        {
            r.RelativeItem().AlignBottom().Text(t =>
            {
                t.Span("Issue Date ").FontSize(11);
                t.Span(issueDate.ToString("dd/MM/yyyy")).Bold().FontSize(11);
            });

            r.ConstantItem(240).Column(c =>
            {
                if (signature != null)
                {
                    try { c.Item().Height(42).AlignCenter().Image(signature).FitArea(); } catch { }
                }
                else
                {
                    c.Item().Height(42);
                }
                c.Item().LineHorizontal(0.8f).LineColor(Ink);
                if (!string.IsNullOrWhiteSpace(signatoryName))
                    c.Item().AlignCenter().Text(signatoryName.Trim()).Bold().FontSize(11);
                c.Item().AlignCenter().Text($"{desig}'s Signature").FontSize(10);
                c.Item().AlignCenter().Text(inst?.Name ?? "").Bold().FontSize(9);
                if (!string.IsNullOrWhiteSpace(inst?.Address))
                    c.Item().AlignCenter().Text(inst.Address).FontSize(8);
            });
        });
    }

    /// <summary>Applies the shared page setup (landscape A4) and double decorative border.</summary>
    public static IContainer Frame(PageDescriptor page)
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(0.45f, Unit.Inch);
        page.DefaultTextStyle(x => x.FontSize(12).FontColor(Ink).LineHeight(1.35f));
        // Double frame: outer thick, small gap, inner thin.
        return page.Content().Border(2.2f).BorderColor(Ink).Padding(5).Border(1).BorderColor(Ink).Padding(18);
    }
}
