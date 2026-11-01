using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.Enums;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.Reports;
using SMS_App.ViewModels.TransferCertificates;

namespace SMS_App.Controllers;

[Authorize]
public class TransferCertificatesController : Controller
{
    private readonly ITransferCertificateManager _tcManager;
    private readonly IStudentManager _studentManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _host;

    public TransferCertificatesController(ITransferCertificateManager tcManager, IStudentManager studentManager,
        IInstituteManager instituteManager, IMapper mapper, IWebHostEnvironment host)
    {
        _tcManager = tcManager;
        _studentManager = studentManager;
        _instituteManager = instituteManager;
        _mapper = mapper;
        _host = host;
    }

    #region List / Details

    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTransferCertificatesPolicy")]
    public async Task<IActionResult> Index()
    {
        var items = await _tcManager.GetAllWithStudentAsync();
        return View(items);
    }

    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTransferCertificatesPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _tcManager.GetByIdWithStudentAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    /// <summary>Partial list of a single student's certificates, loaded into the student profile tab.</summary>
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTransferCertificatesPolicy")]
    public async Task<IActionResult> ByStudent(int studentId)
    {
        var items = await _tcManager.GetAllByStudentIdAsync(studentId);
        ViewBag.StudentId = studentId;
        return PartialView("_StudentTransferCertificates", items);
    }

    #endregion

    #region Create

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateTransferCertificatesPolicy")]
    public async Task<IActionResult> Create(int? studentId)
    {
        var vm = new TransferCertificateCreateVM
        {
            StudentId = studentId ?? 0,
            IssueDate = DateTime.Today,
            StudentList = await BuildStudentSelectAsync()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateTransferCertificatesPolicy")]
    public async Task<IActionResult> Create(TransferCertificateCreateVM vm)
    {
        if (!ModelState.IsValid)
            return await RedisplayCreate(vm);

        var student = await _studentManager.GetByIdAsync(vm.StudentId);
        if (student == null)
        {
            ModelState.AddModelError(nameof(vm.StudentId), "The selected student does not exist.");
            return await RedisplayCreate(vm);
        }

        if (await _tcManager.CertificateNoExistsAsync(vm.CertificateNo))
        {
            ModelState.AddModelError(nameof(vm.CertificateNo), "This certificate number is already in use.");
            return await RedisplayCreate(vm);
        }

        var entity = _mapper.Map<TransferCertificate>(vm);
        entity.CertificateNo = vm.CertificateNo.Trim();
        ApplySnapshot(entity, student);
        StampCreate(entity);

        var saved = await _tcManager.AddAsync(entity);
        if (!saved)
        {
            ModelState.AddModelError("", "Could not save the transfer certificate. Please try again.");
            return await RedisplayCreate(vm);
        }

        TempData["saved"] = "Transfer certificate created successfully.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Edit

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditTransferCertificatesPolicy")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _tcManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        // Only drafts are editable; issued/cancelled records are locked.
        if (entity.Status != TransferCertificateStatus.Draft)
        {
            TempData["failed"] = "Only draft certificates can be edited. Issued certificates can be cancelled instead.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = _mapper.Map<TransferCertificateEditVM>(entity);
        vm.StudentList = await BuildStudentSelectAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditTransferCertificatesPolicy")]
    public async Task<IActionResult> Edit(int id, TransferCertificateEditVM vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _tcManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (entity.Status != TransferCertificateStatus.Draft)
        {
            TempData["failed"] = "Only draft certificates can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!ModelState.IsValid)
            return await RedisplayEdit(vm);

        var student = await _studentManager.GetByIdAsync(vm.StudentId);
        if (student == null)
        {
            ModelState.AddModelError(nameof(vm.StudentId), "The selected student does not exist.");
            return await RedisplayEdit(vm);
        }

        if (await _tcManager.CertificateNoExistsAsync(vm.CertificateNo, id))
        {
            ModelState.AddModelError(nameof(vm.CertificateNo), "This certificate number is already in use.");
            return await RedisplayEdit(vm);
        }

        entity.StudentId = vm.StudentId;
        entity.CertificateNo = vm.CertificateNo.Trim();
        entity.IssueDate = vm.IssueDate;
        entity.LeavingDate = vm.LeavingDate;
        entity.Reason = vm.Reason;
        entity.Conduct = vm.Conduct;
        entity.Remarks = vm.Remarks;
        entity.Status = vm.Status; // a draft may be promoted to Issued here
        ApplySnapshot(entity, student);
        StampEdit(entity);

        var updated = await _tcManager.UpdateAsync(entity);
        if (!updated)
        {
            ModelState.AddModelError("", "Could not update the certificate. Please try again.");
            return await RedisplayEdit(vm);
        }

        TempData["edited"] = "Transfer certificate updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Cancel / Delete

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CancelTransferCertificatesPolicy")]
    public async Task<IActionResult> Cancel(int id)
    {
        var entity = await _tcManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (entity.Status != TransferCertificateStatus.Issued)
        {
            TempData["failed"] = "Only issued certificates can be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        entity.Status = TransferCertificateStatus.Cancelled;
        StampEdit(entity);
        await _tcManager.UpdateAsync(entity);

        TempData["edited"] = "Transfer certificate cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Hard delete is allowed ONLY for drafts, and only for SuperAdmin - issued records are
    // official and must be cancelled, never removed.
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "DeleteTransferCertificatesPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _tcManager.GetByIdWithStudentAsync(id);
        if (entity == null) return NotFound();

        if (entity.Status != TransferCertificateStatus.Draft)
        {
            TempData["failed"] = "Only draft certificates can be deleted. Issued certificates must be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }
        return View(entity);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "DeleteTransferCertificatesPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _tcManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (entity.Status != TransferCertificateStatus.Draft)
        {
            TempData["failed"] = "Only draft certificates can be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var removed = await _tcManager.RemoveAsync(entity);
        if (removed) TempData["deleted"] = "Transfer certificate deleted.";
        else TempData["failed"] = "Could not delete the certificate.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Certificate PDF

    // Only an ISSUED certificate may be rendered as an official document - drafts and
    // cancelled records must never be presented as a valid certificate.
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "GenerateTransferCertificatesPolicy")]
    public async Task<IActionResult> Certificate(int id, bool download = false)
    {
        var entity = await _tcManager.GetByIdWithStudentAsync(id);
        if (entity == null) return NotFound();

        if (entity.Status != TransferCertificateStatus.Issued)
        {
            TempData["failed"] = "A certificate can only be generated for an issued record.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var institute = await _instituteManager.GetByIdAsync(1);
        var logo = ReadInstituteImage(string.IsNullOrWhiteSpace(institute?.Logo) ? "smslogo.png" : institute.Logo)
                   ?? ReadInstituteImage("smslogo.png");
        var signature = ReadInstituteImage("signature.jpg");

        var pdf = new TransferCertificatePdfBuilder(institute, logo, signature, entity).GeneratePdf();

        var fileName = $"TC_{entity.CertificateNo}.pdf";
        Response.Headers["Content-Disposition"] = download
            ? $"attachment; filename=\"{fileName}\""
            : $"inline; filename=\"{fileName}\"";
        return File(pdf, "application/pdf");
    }

    #endregion

    #region Helpers

    private async Task<IActionResult> RedisplayCreate(TransferCertificateCreateVM vm)
    {
        vm.StudentList = await BuildStudentSelectAsync();
        return View("Create", vm);
    }

    private async Task<IActionResult> RedisplayEdit(TransferCertificateEditVM vm)
    {
        vm.StudentList = await BuildStudentSelectAsync();
        return View("Edit", vm);
    }

    private async Task<IEnumerable<SelectListItem>> BuildStudentSelectAsync()
    {
        var students = await _studentManager.GetAllAsync();
        return (students ?? Enumerable.Empty<Student>())
            .OrderBy(s => s.AcademicClass?.ClassSerial).ThenBy(s => s.ClassRoll)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.UniqueId} / Roll {s.ClassRoll})"
            }).ToList();
    }

    private static void ApplySnapshot(TransferCertificate e, Student s)
    {
        e.StudentNameSnapshot = s.Name;
        e.UniqueIdSnapshot = s.UniqueId;
        e.ClassNameSnapshot = s.AcademicClass?.Name;
        e.ClassRollSnapshot = s.ClassRoll;
        e.SessionNameSnapshot = s.AcademicSession?.Name;
        e.AdmissionDateSnapshot = s.AdmissionDate;
    }

    private byte[] ReadInstituteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var path = Path.Combine(_host.WebRootPath, "Images", "Institute", fileName);
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllBytes(path) : null;
    }

    private void StampCreate(TransferCertificate e)
    {
        var userId = HttpContext.Session.GetString("UserId");
        e.CreatedBy = userId; e.CreatedAt = DateTime.Now;
        e.EditedBy = userId; e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }

    private void StampEdit(TransferCertificate e)
    {
        e.EditedBy = HttpContext.Session.GetString("UserId");
        e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }

    #endregion
}
