using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Student Transfer Certificate, rendered from a saved <see cref="StudentCertificate"/> snapshot.
/// The optional last-exam line is printed only when any exam field is present on the record.
/// </summary>
public class TransferCertificatePdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly StudentCertificate _c;

    public TransferCertificatePdfBuilder(Institute institute, byte[] logo, byte[] signature, StudentCertificate cert)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _c = cert;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    private bool HasExam =>
        !string.IsNullOrWhiteSpace(_c.ExamName) || !string.IsNullOrWhiteSpace(_c.TotalMarks) ||
        !string.IsNullOrWhiteSpace(_c.Gpa) || !string.IsNullOrWhiteSpace(_c.Grade) || !string.IsNullOrWhiteSpace(_c.MeritRank);

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            CertificatePdfParts.Frame(page).Column(col =>
            {
                CertificatePdfParts.Header(col, _institute, _logo);
                col.Item().PaddingTop(12).AlignCenter().Text("Transfer Certificate").Bold().FontSize(17);
                col.Item().PaddingTop(14).Element(ComposeBody);
                col.Item().Extend();
                CertificatePdfParts.Footer(col, _institute, _signature, _c.IssueDate, _c.SignatoryName, _c.SignatoryDesignation);
            });
        });
    }

    private void ComposeBody(IContainer container)
    {
        var (subj, obj, poss) = CertificatePdfParts.Pronouns(_c.GenderNameSnapshot);
        var dob = _c.DobSnapshot.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        container.Column(col =>
        {
            col.Spacing(12);
            col.Item().Text(t =>
            {
                t.Justify();
                void V(string v) => t.Span(string.IsNullOrWhiteSpace(v) ? " " : v).Bold().Underline();

                t.Span("This is to certify that , ");
                V(_c.StudentNameSnapshot);
                t.Span("  Father's Name ");
                V(_c.FatherNameSnapshot);
                t.Span("  Mother's Name ");
                V(_c.MotherNameSnapshot);
                t.Span("  Date of Birth ");
                V(dob);
                t.Span($" {subj} has been a diligent student at our institute throughout the academic calender of ");
                V(_c.SessionNameSnapshot);
                t.Span($" {subj} was in Class ");
                V(_c.ClassNameSnapshot);
                t.Span("  Section ");
                V(_c.SectionNameSnapshot);
                t.Span($" and {poss} roll: ");
                V(_c.ClassRollSnapshot.ToString());
                t.Span(" .");

                if (HasExam)
                {
                    t.Span("  In the last ");
                    V(_c.ExamName);
                    t.Span($" , {poss} total marks was ");
                    V(_c.TotalMarks);
                    t.Span(" , GPA was ");
                    V(_c.Gpa);
                    t.Span(" , grade was ");
                    V(_c.Grade);
                    t.Span(" and merit rank was ");
                    V(_c.MeritRank);
                    t.Span(" .");
                }
            });

            col.Item().Text(
                $"As far as I know, {subj.ToLowerInvariant()} has always shown good behavior and has never been involved in any activities against this institution and the state.");
            col.Item().Text($"I wish {obj} all the best for the future.");
        });
    }
}
