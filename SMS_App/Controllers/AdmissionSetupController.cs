using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.OnlineAdmission;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// School-side Online Admission configuration: portal settings (helpline, WhatsApp, SMS, OTP),
/// admission sessions with their notice/eligibility/policy/payment rules, the classes offered
/// (seats, age/GPA rules, fee) and the documents applicants must upload.
/// </summary>
public class AdmissionSetupController : Controller
{
    private readonly IAdmissionSetupManager _setupManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IStudentFeeHeadManager _studentFeeHeadManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdmissionSetupController(
        IAdmissionSetupManager setupManager,
        IAcademicSessionManager academicSessionManager,
        IAcademicClassManager academicClassManager,
        IStudentFeeHeadManager studentFeeHeadManager,
        UserManager<ApplicationUser> userManager)
    {
        _setupManager = setupManager;
        _academicSessionManager = academicSessionManager;
        _academicClassManager = academicClassManager;
        _studentFeeHeadManager = studentFeeHeadManager;
        _userManager = userManager;
    }

    private AdmissionActor Actor => AdmissionActor.Staff(_userManager.GetUserId(User), User.Identity?.Name, MACService.GetMAC());

    #region Settings

    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> Settings() => View(await _setupManager.GetSettingsAsync());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> Settings(AdmissionSetting model)
    {
        var result = await _setupManager.SaveSettingsAsync(model, Actor);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        TempData["success"] = "Admission portal settings saved.";
        return RedirectToAction(nameof(Settings));
    }

    #endregion Settings

    #region Sessions

    [Authorize(Policy = "IndexAdmissionSetupPolicy")]
    public async Task<IActionResult> Index(bool includeArchived = false)
    {
        return View(new AdmissionSessionListVM
        {
            Sessions = await _setupManager.GetSessionsAsync(includeArchived),
            IncludeArchived = includeArchived
        });
    }

    [Authorize(Policy = "IndexAdmissionSetupPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var session = await _setupManager.GetSessionDetailsAsync(id);
        if (session == null)
            return NotFound();

        session.Programs = session.Programs.OrderBy(p => p.AcademicClass?.ClassSerial).ToList();
        session.DocumentRequirements = session.DocumentRequirements.OrderBy(d => d.SortOrder).ThenBy(d => d.Id).ToList();
        var usage = await _setupManager.GetSeatUsageAsync(id);
        return View(new AdmissionSessionDetailsVM
        {
            Session = session,
            State = _setupManager.GetWindowState(session, DateTime.Now),
            SeatUsage = usage.ToDictionary(u => u.AdmissionProgramId),
            PublicUrl = Url.Action("Notice", "Admission", new { id }, Request.Scheme)
        });
    }

    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> Create()
    {
        var model = new AdmissionSessionEditVM();
        var current = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        model.Session.AcademicSessionId = current?.Id ?? 0;
        await FillSessionListsAsync(model);
        return View("Edit", model);
    }

    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> Edit(int id)
    {
        var session = await _setupManager.GetSessionDetailsAsync(id);
        if (session == null)
            return NotFound();
        var model = new AdmissionSessionEditVM { Session = session };
        await FillSessionListsAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> Save(AdmissionSessionEditVM model)
    {
        var result = await _setupManager.SaveSessionAsync(model.Session, Actor);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            await FillSessionListsAsync(model);
            return View("Edit", model);
        }
        TempData["success"] = "Admission session saved.";
        return RedirectToAction(nameof(Details), new { id = result.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> ChangeStatus(int id, AdmissionSessionStatus status)
    {
        var result = await _setupManager.ChangeSessionStatusAsync(id, status, Actor);
        TempData[result.Succeeded ? "success" : "fail"] = result.Succeeded
            ? $"The admission session is now {status.ToString().ToLowerInvariant()}."
            : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task FillSessionListsAsync(AdmissionSessionEditVM model)
    {
        model.AcademicSessionList = (await _academicSessionManager.GetAllAsync())
            .OrderByDescending(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.Session.AcademicSessionId))
            .ToList();
    }

    #endregion Sessions

    #region Offered classes

    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> ProgramEdit(int sessionId, int? id)
    {
        var session = await _setupManager.GetSessionDetailsAsync(sessionId);
        if (session == null)
            return NotFound();

        var model = new AdmissionProgramEditVM { Session = session };
        model.Program.AdmissionSessionId = sessionId;
        if (id.HasValue)
        {
            var program = session.Programs.FirstOrDefault(p => p.Id == id.Value);
            if (program == null)
                return NotFound();
            model.Program = program;
        }
        await FillProgramListsAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> SaveProgram(AdmissionProgramEditVM model)
    {
        var result = await _setupManager.SaveProgramAsync(model.Program, Actor);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            model.Session = await _setupManager.GetSessionDetailsAsync(model.Program.AdmissionSessionId);
            if (model.Session == null)
                return NotFound();
            await FillProgramListsAsync(model);
            return View("ProgramEdit", model);
        }
        TempData["success"] = "Class offering saved.";
        return RedirectToAction(nameof(Details), new { id = model.Program.AdmissionSessionId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> RemoveProgram(int id, int sessionId)
    {
        var result = await _setupManager.RemoveProgramAsync(id);
        TempData[result.Succeeded ? "success" : "fail"] = result.Succeeded ? "Class offering removed." : result.Error;
        return RedirectToAction(nameof(Details), new { id = sessionId });
    }

    private async Task FillProgramListsAsync(AdmissionProgramEditVM model)
    {
        model.ClassList = (await _academicClassManager.GetAllAsync())
            .Where(c => c.Status || c.Id == model.Program.AcademicClassId)
            .OrderBy(c => c.ClassSerial)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.Program.AcademicClassId))
            .ToList();
        model.FeeHeadList = (await _studentFeeHeadManager.GetAllAsync())
            .OrderBy(f => f.Name)
            .Select(f => new SelectListItem(f.Name, f.Id.ToString(), f.Id == model.Program.AdmissionFeeHeadId))
            .ToList();
    }

    #endregion Offered classes

    #region Required documents

    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> DocumentEdit(int sessionId, int? id)
    {
        var session = await _setupManager.GetSessionDetailsAsync(sessionId);
        if (session == null)
            return NotFound();

        var model = new AdmissionDocumentRequirementEditVM { Session = session };
        model.Requirement.AdmissionSessionId = sessionId;
        model.Requirement.SortOrder = session.DocumentRequirements.Count + 1;
        if (id.HasValue)
        {
            var requirement = session.DocumentRequirements.FirstOrDefault(d => d.Id == id.Value);
            if (requirement == null)
                return NotFound();
            model.Requirement = requirement;
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> SaveDocument(AdmissionDocumentRequirementEditVM model)
    {
        var result = await _setupManager.SaveDocumentRequirementAsync(model.Requirement, Actor);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            model.Session = await _setupManager.GetSessionDetailsAsync(model.Requirement.AdmissionSessionId);
            if (model.Session == null)
                return NotFound();
            return View("DocumentEdit", model);
        }
        TempData["success"] = "Document requirement saved.";
        return RedirectToAction(nameof(Details), new { id = model.Requirement.AdmissionSessionId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManageAdmissionSetupPolicy")]
    public async Task<IActionResult> RemoveDocument(int id, int sessionId)
    {
        var result = await _setupManager.RemoveDocumentRequirementAsync(id);
        TempData[result.Succeeded ? "success" : "fail"] = result.Succeeded ? "Document requirement removed." : result.Error;
        return RedirectToAction(nameof(Details), new { id = sessionId });
    }

    #endregion Required documents
}
