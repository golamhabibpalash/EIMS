using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using SMS.BLL.Contracts;
using SMS_App.Utilities.Reports;

namespace SMS_App.Controllers;

[Authorize]
public class TestimonialsController : CertificateGeneratorController
{
    public TestimonialsController(IStudentManager studentManager, IInstituteManager instituteManager,
        IAcademicSessionManager sessionManager, IAcademicClassManager classManager,
        IAcademicSectionManager sectionManager, IWebHostEnvironment host)
        : base(studentManager, instituteManager, sessionManager, classManager, sectionManager, host)
    {
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> Index(int? academicSessionId, int? academicClassId,
        int? academicSectionId, string searchString)
    {
        await PopulateFilterDropdownsAsync(academicSessionId, academicClassId, academicSectionId);
        var vm = await BuildFilterVMAsync(academicSessionId, academicClassId, academicSectionId, searchString);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> Generate(int studentId, DateTime? issueDate,
        string signatoryName, string signatoryDesignation, bool download = false)
    {
        var student = await _studentManager.GetByIdAsync(studentId);
        if (student == null) return NotFound();

        var (institute, logo, signature) = await LoadBrandingAsync();
        var pdf = new TestimonialPdfBuilder(institute, logo, signature, student,
            issueDate ?? DateTime.Today, signatoryName, signatoryDesignation).GeneratePdf();

        var fileName = $"Testimonial_{student.UniqueId}_{DateTime.Now:yyyyMMdd}.pdf";
        Response.Headers["Content-Disposition"] = download
            ? $"attachment; filename=\"{fileName}\""
            : $"inline; filename=\"{fileName}\"";
        return File(pdf, "application/pdf");
    }

    /// <summary>Generate panel shown on the student profile tab.</summary>
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public IActionResult ByStudent(int studentId)
    {
        ViewBag.StudentId = studentId;
        return PartialView("_StudentTestimonial");
    }
}
