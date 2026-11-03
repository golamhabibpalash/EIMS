using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Student Testimonial (character certificate), rendered from a saved <see cref="StudentCertificate"/>
/// snapshot so the printed document always matches the record as issued.
/// </summary>
public class TestimonialPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly StudentCertificate _c;

    public TestimonialPdfBuilder(Institute institute, byte[] logo, byte[] signature, StudentCertificate cert)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _c = cert;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            CertificatePdfParts.Frame(page).Column(col =>
            {
                CertificatePdfParts.Header(col, _institute, _logo);
                col.Item().PaddingTop(12).AlignCenter().Text("Testimonial").Bold().FontSize(17);
                col.Item().PaddingTop(14).Element(ComposeBody);
                col.Item().Extend();
                CertificatePdfParts.Footer(col, _institute, _signature, _c.IssueDate, _c.SignatoryName, _c.SignatoryDesignation);
            });
        });
    }

    private void ComposeBody(IContainer container)
    {
        var (subj, obj, _) = CertificatePdfParts.Pronouns(_c.GenderNameSnapshot);
        var dob = _c.DobSnapshot.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        container.Column(col =>
        {
            col.Spacing(12);
            col.Item().Text(t =>
            {
                t.Justify();
                void V(string v) => t.Span(string.IsNullOrWhiteSpace(v) ? " " : v).Bold().Underline();

                t.Span("This is to certify that, ");
                V(_c.StudentNameSnapshot);
                t.Span("  Father's Name ");
                V(_c.FatherNameSnapshot);
                t.Span("  Mother's Name ");
                V(_c.MotherNameSnapshot);
                t.Span($" . {subj} is a regular student from Session ");
                V(_c.SessionNameSnapshot);
                t.Span("  Class ");
                V(_c.ClassNameSnapshot);
                t.Span("  Section ");
                V(_c.SectionNameSnapshot);
                t.Span("  Roll ");
                V(_c.ClassRollSnapshot.ToString());
                t.Span("  Student Id ");
                V(_c.UniqueIdSnapshot);
                t.Span("  and birth date is ");
                V(dob);
                t.Span(" .");
            });

            col.Item().Text(
                $"As far as I know, {subj.ToLowerInvariant()} is of good character and is not involved in any activities against this institution and the state.");
            col.Item().Text($"I wish {obj} all the best.");
        });
    }
}
