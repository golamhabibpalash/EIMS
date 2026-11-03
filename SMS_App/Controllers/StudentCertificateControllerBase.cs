using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.Enums;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.Reports;
using SMS_App.ViewModels.Certificates;

namespace SMS_App.Controllers;

/// <summary>
/// Shared implementation for the two certificate modules (Testimonial, Transfer Certificate).
/// Records are stored with a student snapshot; staff create/edit/print and students may apply for
/// their own. The derived controllers only supply the <see cref="Type"/>, route name and title,
/// plus the per-action authorization attributes.
/// </summary>
public abstract class StudentCertificateControllerBase : Controller
{
    protected readonly IStudentCertificateManager _certManager;
    protected readonly IStudentManager _studentManager;
    protected readonly IInstituteManager _instituteManager;
    protected readonly IAcademicSessionManager _sessionManager;
    protected readonly IAcademicClassManager _classManager;
    protected readonly IAcademicSectionManager _sectionManager;
    protected readonly UserManager<ApplicationUser> _userManager;
    protected readonly IWebHostEnvironment _host;

    protected StudentCertificateControllerBase(IStudentCertificateManager certManager, IStudentManager studentManager,
        IInstituteManager instituteManager, IAcademicSessionManager sessionManager, IAcademicClassManager classManager,
        IAcademicSectionManager sectionManager, UserManager<ApplicationUser> userManager, IWebHostEnvironment host)
    {
        _certManager = certManager;
        _studentManager = studentManager;
        _instituteManager = instituteManager;
        _sessionManager = sessionManager;
        _classManager = classManager;
        _sectionManager = sectionManager;
        _userManager = userManager;
        _host = host;
    }

    protected abstract CertificateType Type { get; }
    protected abstract string ControllerName { get; }
    protected abstract string Title { get; }

    // --------------------------------------------------------------------- list

    protected async Task<IActionResult> IndexCore(int? classId, int? sectionId, string search)
    {
        await PopulateFiltersAsync(classId, sectionId);

        var items = await _certManager.GetAllByTypeAsync(Type);
        if (classId != null) items = items.Where(c => c.Student?.AcademicClassId == classId).ToList();
        if (sectionId != null) items = items.Where(c => c.Student?.AcademicSectionId == sectionId).ToList();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim().ToLowerInvariant();
            items = items.Where(c =>
                (c.StudentNameSnapshot?.ToLowerInvariant().Contains(t) ?? false)
                || (c.UniqueIdSnapshot?.ToLowerInvariant().Contains(t) ?? false)
                || c.ClassRollSnapshot.ToString().Contains(t)).ToList();
        }

        return View("CertificatesIndex", new StudentCertificateIndexVM
        {
            Type = Type,
            ControllerName = ControllerName,
            Title = Title,
            Items = items,
            AcademicClassId = classId,
            AcademicSectionId = sectionId,
            SearchString = search,
        });
    }

    // ------------------------------------------------------------------- create

    protected async Task<IActionResult> CreateGetCore(int? studentId)
    {
        var vm = NewFormVM();
        vm.StudentId = studentId ?? 0;
        vm.StudentList = await BuildStudentSelectAsync();
        return View("CertificatesForm", vm);
    }

    protected async Task<IActionResult> CreatePostCore(StudentCertificateFormVM vm)
    {
        DecorateFormVM(vm);
        if (!ModelState.IsValid) { vm.StudentList = await BuildStudentSelectAsync(); return View("CertificatesForm", vm); }

        var student = await _studentManager.GetByIdAsync(vm.StudentId);
        if (student == null)
        {
            ModelState.AddModelError(nameof(vm.StudentId), "The selected student does not exist.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View("CertificatesForm", vm);
        }

        var cert = new StudentCertificate { Type = Type };
        ApplyForm(cert, vm);
        SnapshotFromStudent(cert, student);
        StampCreate(cert);

        if (!await _certManager.AddAsync(cert))
        {
            ModelState.AddModelError("", "Could not save the record. Please try again.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View("CertificatesForm", vm);
        }
        TempData["saved"] = $"{Title} saved.";
        return RedirectToAction("Index");
    }

    // --------------------------------------------------------------------- edit

    protected async Task<IActionResult> EditGetCore(int id)
    {
        var cert = await _certManager.GetByIdAsync(id);
        if (cert == null || cert.Type != Type) return NotFound();

        var vm = ToFormVM(cert);
        vm.StudentList = await BuildStudentSelectAsync();
        return View("CertificatesForm", vm);
    }

    protected async Task<IActionResult> EditPostCore(int id, StudentCertificateFormVM vm)
    {
        if (id != vm.Id) return NotFound();
        DecorateFormVM(vm);

        var cert = await _certManager.GetByIdAsync(id);
        if (cert == null || cert.Type != Type) return NotFound();
        if (!ModelState.IsValid) { vm.StudentList = await BuildStudentSelectAsync(); return View("CertificatesForm", vm); }

        var student = await _studentManager.GetByIdAsync(vm.StudentId);
        if (student == null)
        {
            ModelState.AddModelError(nameof(vm.StudentId), "The selected student does not exist.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View("CertificatesForm", vm);
        }

        ApplyForm(cert, vm);
        SnapshotFromStudent(cert, student); // refresh snapshot from the (possibly changed) student
        StampEdit(cert);

        if (!await _certManager.UpdateAsync(cert))
        {
            ModelState.AddModelError("", "Could not update the record. Please try again.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View("CertificatesForm", vm);
        }
        TempData["edited"] = $"{Title} updated.";
        return RedirectToAction("Index");
    }

    // ----------------------------------------------------------- details/delete

    protected async Task<IActionResult> DetailsCore(int id)
    {
        var cert = await _certManager.GetByIdWithStudentAsync(id);
        if (cert == null || cert.Type != Type) return NotFound();
        ViewBag.ControllerName = ControllerName;
        ViewBag.Title = Title;
        return View("CertificatesDetails", cert);
    }

    protected async Task<IActionResult> DeleteConfirmedCore(int id)
    {
        var cert = await _certManager.GetByIdAsync(id);
        if (cert == null || cert.Type != Type) return NotFound();
        if (await _certManager.RemoveAsync(cert)) TempData["deleted"] = $"{Title} deleted.";
        else TempData["failed"] = "Could not delete the record.";
        return RedirectToAction("Index");
    }

    // -------------------------------------------------------------------- print

    protected async Task<IActionResult> PrintCore(int id, bool download)
    {
        var cert = await _certManager.GetByIdWithStudentAsync(id);
        if (cert == null || cert.Type != Type) return NotFound();

        // A student may only print their own certificate; staff still need the module's claim.
        if (User.IsInRole("Student"))
        {
            if (!await IsOwnedByCurrentStudentAsync(cert.StudentId)) return Forbid();
        }
        else
        {
            var requiredClaim = Type == CertificateType.Testimonial ? "View Testimonials" : "Generate Transfer Certificate";
            if (!User.Claims.Any(c => c.Type == requiredClaim)) return Forbid();
        }

        var (institute, logo, signature) = await LoadBrandingAsync();
        byte[] pdf = Type == CertificateType.Testimonial
            ? new TestimonialPdfBuilder(institute, logo, signature, cert).GeneratePdf()
            : new TransferCertificatePdfBuilder(institute, logo, signature, cert).GeneratePdf();

        var prefix = Type == CertificateType.Testimonial ? "Testimonial" : "TransferCertificate";
        var fileName = $"{prefix}_{cert.UniqueIdSnapshot}.pdf";
        Response.Headers["Content-Disposition"] = download
            ? $"attachment; filename=\"{fileName}\""
            : $"inline; filename=\"{fileName}\"";
        return File(pdf, "application/pdf");
    }

    // -------------------------------------------------------------------- apply

    /// <summary>A student applies for their own certificate (or an admin applies on their behalf).</summary>
    protected async Task<IActionResult> ApplyCore(int studentId, string returnUrl)
    {
        // Students can only apply for themselves; resolve their own id and ignore any passed value.
        if (User.IsInRole("Student") && !(User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))
        {
            var ownId = await CurrentStudentIdAsync();
            if (ownId == null) return Forbid();
            studentId = ownId.Value;
        }

        var student = await _studentManager.GetByIdAsync(studentId);
        if (student == null) return NotFound();

        var cert = new StudentCertificate
        {
            Type = Type,
            IssueDate = DateTime.Today,
            SignatoryDesignation = "Headmaster",
        };
        SnapshotFromStudent(cert, student);
        StampCreate(cert);
        await _certManager.AddAsync(cert);

        TempData["saved"] = $"Applied for {Title}.";
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index");
    }

    // ------------------------------------------------------------------ helpers

    protected async Task PopulateFiltersAsync(int? classId, int? sectionId)
    {
        var current = await _sessionManager.GetCurrentAcademicSessionAsync();
        var classes = await _classManager.GetAllAsync();
        ViewBag.academicClassId = new SelectList(classes.Where(c => c.Status == true), "Id", "Name", classId);
        if (classId != null)
        {
            var sections = await _sectionManager.GetAllByClassWithSessionId(classId.Value, current?.Id ?? 0);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name", sectionId);
        }
    }

    protected async Task<IEnumerable<SelectListItem>> BuildStudentSelectAsync()
    {
        var students = await _studentManager.GetAllAsync();
        return (students ?? Enumerable.Empty<Student>())
            .OrderBy(s => s.AcademicClass?.ClassSerial).ThenBy(s => s.ClassRoll)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} ({s.UniqueId} / Roll {s.ClassRoll})" })
            .ToList();
    }

    protected static void SnapshotFromStudent(StudentCertificate cert, Student s)
    {
        cert.StudentId = s.Id;
        cert.StudentNameSnapshot = s.Name;
        cert.FatherNameSnapshot = s.FatherName;
        cert.MotherNameSnapshot = s.MotherName;
        cert.DobSnapshot = s.DOB;
        cert.ClassNameSnapshot = s.AcademicClass?.Name;
        cert.SectionNameSnapshot = s.AcademicSection?.Name;
        cert.ClassRollSnapshot = s.ClassRoll;
        cert.SessionNameSnapshot = s.AcademicSession?.Name;
        cert.UniqueIdSnapshot = s.UniqueId;
        cert.GenderNameSnapshot = s.Gender?.Name;
    }

    protected static void ApplyForm(StudentCertificate cert, StudentCertificateFormVM vm)
    {
        cert.IssueDate = vm.IssueDate;
        cert.DeliveryDate = vm.DeliveryDate;
        cert.SignatoryName = vm.SignatoryName;
        cert.SignatoryDesignation = vm.SignatoryDesignation;
        if (cert.Type == CertificateType.TransferCertificate)
        {
            cert.LeavingDate = vm.LeavingDate;
            cert.Reason = vm.Reason;
            cert.ExamName = vm.ExamName;
            cert.TotalMarks = vm.TotalMarks;
            cert.Gpa = vm.Gpa;
            cert.Grade = vm.Grade;
            cert.MeritRank = vm.MeritRank;
        }
    }

    protected StudentCertificateFormVM ToFormVM(StudentCertificate c) => Decorate(new StudentCertificateFormVM
    {
        Id = c.Id,
        StudentId = c.StudentId,
        IssueDate = c.IssueDate,
        DeliveryDate = c.DeliveryDate,
        LeavingDate = c.LeavingDate,
        Reason = c.Reason,
        ExamName = c.ExamName,
        TotalMarks = c.TotalMarks,
        Gpa = c.Gpa,
        Grade = c.Grade,
        MeritRank = c.MeritRank,
        SignatoryName = c.SignatoryName,
        SignatoryDesignation = c.SignatoryDesignation,
    });

    protected StudentCertificateFormVM NewFormVM() => Decorate(new StudentCertificateFormVM());

    private StudentCertificateFormVM Decorate(StudentCertificateFormVM vm) { DecorateFormVM(vm); return vm; }

    private void DecorateFormVM(StudentCertificateFormVM vm)
    {
        vm.Type = Type;
        vm.ControllerName = ControllerName;
        vm.Title = Title;
    }

    protected async Task<(Institute Institute, byte[] Logo, byte[] Signature)> LoadBrandingAsync()
    {
        var institute = await _instituteManager.GetByIdAsync(1);
        var logo = ReadInstituteImage(string.IsNullOrWhiteSpace(institute?.Logo) ? "smslogo.png" : institute.Logo)
                   ?? ReadInstituteImage("smslogo.png");
        var signature = ReadInstituteImage("signature.jpg");
        return (institute, logo, signature);
    }

    protected byte[] ReadInstituteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var path = Path.Combine(_host.WebRootPath, "Images", "Institute", fileName);
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllBytes(path) : null;
    }

    /// <summary>The logged-in student's own Student.Id (UserName == student UniqueId), else null.</summary>
    protected async Task<int?> CurrentStudentIdAsync()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName)) return null;
        var students = await _studentManager.GetAllAsync();
        return students?.FirstOrDefault(s => s.UniqueId == userName)?.Id;
    }

    protected async Task<bool> IsOwnedByCurrentStudentAsync(int studentId)
    {
        var own = await CurrentStudentIdAsync();
        return own != null && own.Value == studentId;
    }

    protected void StampCreate(StudentCertificate e)
    {
        var userId = HttpContext.Session.GetString("UserId");
        e.CreatedBy = userId; e.CreatedAt = DateTime.Now;
        e.EditedBy = userId; e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }

    protected void StampEdit(StudentCertificate e)
    {
        e.EditedBy = HttpContext.Session.GetString("UserId");
        e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }
}
