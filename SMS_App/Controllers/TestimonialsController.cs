using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.Enums;
using SMS_App.ViewModels.Certificates;

namespace SMS_App.Controllers;

[Authorize]
public class TestimonialsController : StudentCertificateControllerBase
{
    public TestimonialsController(IStudentCertificateManager certManager, IStudentManager studentManager,
        IInstituteManager instituteManager, IAcademicSessionManager sessionManager, IAcademicClassManager classManager,
        IAcademicSectionManager sectionManager, UserManager<ApplicationUser> userManager, IWebHostEnvironment host, SMS_App.MultiTenancy.ITenantFileStorage tenantFiles)
        : base(certManager, studentManager, instituteManager, sessionManager, classManager, sectionManager, userManager, host, tenantFiles)
    {
    }

    protected override CertificateType Type => CertificateType.Testimonial;
    protected override string ControllerName => "Testimonials";
    protected override string Title => "Testimonial";

    [Authorize(Roles = "SuperAdmin, Admin,Teacher"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Index(int? academicClassId, int? academicSectionId, string searchString)
        => IndexCore(academicClassId, academicSectionId, searchString);

    [Authorize(Roles = "SuperAdmin, Admin"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Create(int? studentId) => CreateGetCore(studentId);

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Create(StudentCertificateFormVM vm) => CreatePostCore(vm);

    [Authorize(Roles = "SuperAdmin, Admin"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Edit(int id) => EditGetCore(id);

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Edit(int id, StudentCertificateFormVM vm) => EditPostCore(id, vm);

    [Authorize(Roles = "SuperAdmin, Admin,Teacher"), Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> Details(int id) => DetailsCore(id);

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public Task<IActionResult> DeleteConfirmed(int id) => DeleteConfirmedCore(id);

    [Authorize(Roles = "SuperAdmin, Admin,Teacher,Student")]
    public Task<IActionResult> Print(int id, bool download = false) => PrintCore(id, download);

    // Students (and staff) can apply; no claim required so student accounts can submit.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "SuperAdmin, Admin,Teacher,Student")]
    public Task<IActionResult> Apply(int studentId, string returnUrl = null) => ApplyCore(studentId, returnUrl);

    // Student profile tab: that student's testimonial records. The owning student may view theirs.
    [Authorize(Roles = "SuperAdmin, Admin,Teacher,Student")]
    public async Task<IActionResult> ByStudent(int studentId)
    {
        var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Teacher");
        if (!isStaff && !await IsOwnedByCurrentStudentAsync(studentId)) return Forbid();

        var items = await _certManager.GetAllByStudentIdAsync(studentId);
        ViewBag.StudentId = studentId;
        ViewBag.ControllerName = ControllerName;
        ViewBag.Title = Title;
        return PartialView("_StudentCertificates", items.FindAll(c => c.Type == CertificateType.Testimonial));
    }
}
