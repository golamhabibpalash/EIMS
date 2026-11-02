using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Entities;

namespace SMS_App.Utilities.Reports;

/// <summary>Optional last-exam details printed on the transfer certificate.</summary>
public class TcExamInfo
{
    public string ExamName { get; set; }
    public string TotalMarks { get; set; }
    public string Gpa { get; set; }
    public string Grade { get; set; }
    public string MeritRank { get; set; }

    public bool HasAny =>
        !string.IsNullOrWhiteSpace(ExamName) || !string.IsNullOrWhiteSpace(TotalMarks) ||
        !string.IsNullOrWhiteSpace(Gpa) || !string.IsNullOrWhiteSpace(Grade) || !string.IsNullOrWhiteSpace(MeritRank);
}

/// <summary>
/// Dynamic Student Transfer Certificate - generated on demand for any student, matching the
/// institute's landscape-A4 template. Values are read live from the student record; the optional
/// last-exam line is supplied by the operator at generate time.
/// </summary>
public class TransferCertificatePdfBuilder : IDocument
{
    private readonly Institute _institute;
    private readonly byte[] _logo;
    private readonly byte[] _signature;
    private readonly Student _s;
    private readonly DateTime _issueDate;
    private readonly string _signatoryName;
    private readonly string _signatoryDesignation;
    private readonly TcExamInfo _exam;

    public TransferCertificatePdfBuilder(Institute institute, byte[] logo, byte[] signature, Student student,
        DateTime issueDate, string signatoryName, string signatoryDesignation, TcExamInfo exam)
    {
        _institute = institute;
        _logo = logo;
        _signature = signature;
        _s = student;
        _issueDate = issueDate;
        _signatoryName = signatoryName;
        _signatoryDesignation = signatoryDesignation;
        _exam = exam ?? new TcExamInfo();
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
                CertificatePdfParts.Footer(col, _institute, _signature, _issueDate, _signatoryName, _signatoryDesignation);
            });
        });
    }

    private void ComposeBody(IContainer container)
    {
        var (subj, obj, poss) = CertificatePdfParts.Pronouns(_s.Gender?.Name);
        var dob = _s.DOB.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        container.Column(col =>
        {
            col.Spacing(12);

            col.Item().Text(t =>
            {
                t.Justify();
                void V(string v) => t.Span(string.IsNullOrWhiteSpace(v) ? " " : v).Bold().Underline();

                t.Span("This is to certify that , ");
                V(_s.Name);
                t.Span("  Father's Name ");
                V(_s.FatherName);
                t.Span("  Mother's Name ");
                V(_s.MotherName);
                t.Span("  Date of Birth ");
                V(dob);
                t.Span($" {subj} has been a diligent student at our institute throughout the academic calender of ");
                V(_s.AcademicSession?.Name);
                t.Span($" {subj} was in Class ");
                V(_s.AcademicClass?.Name);
                t.Span("  Section ");
                V(_s.AcademicSection?.Name);
                t.Span($" and {poss} roll: ");
                V(_s.ClassRoll.ToString());
                t.Span(" .");

                if (_exam.HasAny)
                {
                    t.Span("  In the last ");
                    V(_exam.ExamName);
                    t.Span($" , {poss} total marks was ");
                    V(_exam.TotalMarks);
                    t.Span(" , GPA was ");
                    V(_exam.Gpa);
                    t.Span(" , grade was ");
                    V(_exam.Grade);
                    t.Span(" and merit rank was ");
                    V(_exam.MeritRank);
                    t.Span(" .");
                }
            });

            col.Item().Text(
                $"As far as I know, {subj.ToLowerInvariant()} has always shown good behavior and has never been involved in any activities against this institution and the state.");

            col.Item().Text($"I wish {obj} all the best for the future.");
        });
    }
}
