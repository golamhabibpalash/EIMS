using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Renders an official A4 Transfer Certificate from a saved (Issued) <see cref="TransferCertificate"/>.
/// All student fields come from the certificate's point-in-time snapshot, so the printed document
/// always matches the record as issued - not the student's current data.
/// </summary>
public class TransferCertificatePdfBuilder : IDocument
{
    private const string Ink = "#1F2A37";
    private const string Accent = "#1565C0";

    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly TransferCertificate _tc;

    public TransferCertificatePdfBuilder(Institute institute, byte[] logo, byte[] signature, TransferCertificate tc)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _tc = tc;
    }

    private static string D(DateTime? d) => d?.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) ?? "-";
    private static string V(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s.Trim();

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.0f, Unit.Inch);
            page.DefaultTextStyle(x => x.FontSize(11).FontColor(Ink));

            page.Content().Border(1.2f).BorderColor(Accent).Padding(22).Column(col =>
            {
                ComposeHeader(col);

                col.Item().PaddingTop(10).AlignCenter().Text("TRANSFER CERTIFICATE")
                    .Bold().FontSize(16).FontColor(Accent).Underline();

                col.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text(t => { t.Span("Certificate No: ").SemiBold(); t.Span(V(_tc.CertificateNo)); });
                    r.RelativeItem().AlignRight().Text(t => { t.Span("Date of Issue: ").SemiBold(); t.Span(D(_tc.IssueDate)); });
                });

                col.Item().PaddingTop(14).Column(body =>
                {
                    body.Spacing(7);
                    Field(body, "Student's Name", V(_tc.StudentNameSnapshot));
                    Field(body, "Student ID", V(_tc.UniqueIdSnapshot));
                    Field(body, "Class", V(_tc.ClassNameSnapshot));
                    Field(body, "Roll No", _tc.ClassRollSnapshot > 0 ? _tc.ClassRollSnapshot.ToString() : "-");
                    Field(body, "Academic Session", V(_tc.SessionNameSnapshot));
                    Field(body, "Date of Admission", D(_tc.AdmissionDateSnapshot));
                    Field(body, "Date of Leaving", D(_tc.LeavingDate));
                    Field(body, "Reason for Leaving", V(_tc.Reason));
                    Field(body, "Conduct / Character", V(_tc.Conduct));
                    if (!string.IsNullOrWhiteSpace(_tc.Remarks))
                        Field(body, "Remarks", V(_tc.Remarks));
                });

                col.Item().PaddingTop(28).Text(
                    "Certified that the above particulars are correct as per the records of this institution.")
                    .Italic().FontSize(10);

                // Signature block hugging the bottom-right.
                col.Item().PaddingTop(40).Row(r =>
                {
                    r.RelativeItem();
                    r.ConstantItem(200).Column(sig =>
                    {
                        if (_signature != null)
                        {
                            try { sig.Item().Height(40).AlignRight().Image(_signature).FitArea(); } catch { }
                        }
                        sig.Item().PaddingTop(2).LineHorizontal(0.75f).LineColor(Ink);
                        sig.Item().AlignCenter().Text("Head of the Institution").SemiBold().FontSize(10);
                        sig.Item().AlignCenter().Text(_institute?.Name ?? "").FontSize(9);
                    });
                });
            });
        });
    }

    private void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().Row(r =>
        {
            if (_logo != null)
            {
                try { r.ConstantItem(64).Image(_logo).FitArea(); } catch { }
            }
            r.RelativeItem().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text(_institute?.Name ?? "").Bold().FontSize(18).FontColor(Accent);
                if (!string.IsNullOrWhiteSpace(_institute?.Address))
                    c.Item().AlignCenter().Text(_institute.Address).FontSize(10);
                if (!string.IsNullOrWhiteSpace(_institute?.EIIN))
                    c.Item().AlignCenter().Text($"EIIN: {_institute.EIIN}").FontSize(10);
            });
            if (_logo != null) r.ConstantItem(64); // balance the row so the name stays centred
        });
        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Accent);
    }

    private static void Field(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(150).Text(label).SemiBold();
            r.AutoItem().Text(":");
            r.RelativeItem().PaddingLeft(8).Text(value);
        });
    }
}
