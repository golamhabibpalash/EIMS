using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>
/// Dynamic Student Testimonial (character certificate) - generated on demand for any student,
/// matching the institute's landscape-A4 template. Nothing is stored; every value is read live
/// from the student record at print time.
/// </summary>
public class TestimonialPdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly Student _s;
    private readonly DateTime _issueDate;
    private readonly string _signatoryName;
    private readonly string _signatoryDesignation;

    public TestimonialPdfBuilder(Institute institute, byte[] logo, byte[] signature, Student student,
        DateTime issueDate, string signatoryName, string signatoryDesignation)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _s = student;
        _issueDate = issueDate;
        _signatoryName = signatoryName;
        _signatoryDesignation = signatoryDesignation;
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
                CertificatePdfParts.Footer(col, _institute, _signature, _issueDate, _signatoryName, _signatoryDesignation);
            });
        });
    }

    private void ComposeBody(IContainer container)
    {
        var (subj, obj, _) = CertificatePdfParts.Pronouns(_s.Gender?.Name);
        var dob = _s.DOB.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        container.Column(col =>
        {
            col.Spacing(12);

            col.Item().Text(t =>
            {
                t.Justify();
                void V(string v) => t.Span(string.IsNullOrWhiteSpace(v) ? " " : v).Bold().Underline();

                t.Span("This is to certify that, ");
                V(_s.Name);
                t.Span("  Father's Name ");
                V(_s.FatherName);
                t.Span("  Mother's Name ");
                V(_s.MotherName);
                t.Span($" . {subj} is a regular student from Session ");
                V(_s.AcademicSession?.Name);
                t.Span("  Class ");
                V(_s.AcademicClass?.Name);
                t.Span("  Section ");
                V(_s.AcademicSection?.Name);
                t.Span("  Roll ");
                V(_s.ClassRoll.ToString());
                t.Span("  Student Id ");
                V(_s.UniqueId);
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
