using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.OnlineAdmission;
using SMS_App.ViewModels.OnlineAdmission;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// Admission office work queue: dashboard, application list, review (documents, corrections,
/// notes), decisions (approve / reject / waitlist / eligibility exception) and enrolment.
/// Each group of actions has its own policy so officers, approvers and enrolment staff can be
/// given different permissions.
/// </summary>
public class AdmissionApplicationsController : Controller
{
    private readonly IAdmissionApplicationManager _applicationManager;
    private readonly IAdmissionSetupManager _setupManager;
    private readonly IAdmissionPaymentManager _paymentManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdmissionFileStore _fileStore;
    private readonly AdmissionNotifier _notifier;
    private readonly IWebHostEnvironment _host;
    private readonly IAppLogger _appLogger;

    public AdmissionApplicationsController(
        IAdmissionApplicationManager applicationManager,
        IAdmissionSetupManager setupManager,
        IAdmissionPaymentManager paymentManager,
        IAcademicSectionManager academicSectionManager,
        UserManager<ApplicationUser> userManager,
        AdmissionFileStore fileStore,
        AdmissionNotifier notifier,
        IWebHostEnvironment host,
        IAppLogger appLogger)
    {
        _applicationManager = applicationManager;
        _setupManager = setupManager;
        _paymentManager = paymentManager;
        _academicSectionManager = academicSectionManager;
        _userManager = userManager;
        _fileStore = fileStore;
        _notifier = notifier;
        _host = host;
        _appLogger = appLogger;
    }

    private AdmissionActor Actor => AdmissionActor.Staff(_userManager.GetUserId(User), User.Identity?.Name, MACService.GetMAC());

    #region Dashboard & list

    [Authorize(Policy = "DashboardAdmissionPolicy")]
    public async Task<IActionResult> Dashboard(int? sessionId)
    {
        var sessions = await _setupManager.GetSessionsAsync(includeArchived: true);
        sessionId ??= sessions.FirstOrDefault(s => s.Status == AdmissionSessionStatus.Published)?.Id ?? sessions.FirstOrDefault()?.Id;

        var model = new AdmissionDashboardVM
        {
            AdmissionSessionId = sessionId,
            SessionList = sessions.Select(s => new SelectListItem(s.Title, s.Id.ToString(), s.Id == sessionId)).ToList()
        };
        if (sessionId.HasValue)
        {
            model.StatusCounts = await _applicationManager.GetStatusCountsAsync(sessionId);
            model.Programs = sessions.First(s => s.Id == sessionId).Programs.OrderBy(p => p.AcademicClass?.ClassSerial).ToList();
            model.SeatUsage = (await _applicationManager.GetSeatUsageAsync(sessionId)).ToDictionary(u => u.AdmissionProgramId);
            model.Payments = await _paymentManager.SearchAsync(null, sessionId);
        }
        return View(model);
    }

    [Authorize(Policy = "IndexAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Index([Bind(Prefix = "Filter")] AdmissionApplicationFilter filter)
    {
        filter ??= new AdmissionApplicationFilter();
        var sessions = await _setupManager.GetSessionsAsync(includeArchived: true);
        if (!filter.AdmissionSessionId.HasValue && sessions.Count > 0)
            filter.AdmissionSessionId = sessions.FirstOrDefault(s => s.Status == AdmissionSessionStatus.Published)?.Id ?? sessions[0].Id;

        var programs = sessions.FirstOrDefault(s => s.Id == filter.AdmissionSessionId)?.Programs ?? new List<AdmissionProgram>();
        return View(new AdmissionApplicationListVM
        {
            Filter = filter,
            Applications = await _applicationManager.SearchAsync(filter),
            StatusCounts = await _applicationManager.GetStatusCountsAsync(filter.AdmissionSessionId),
            SessionList = sessions.Select(s => new SelectListItem(s.Title, s.Id.ToString(), s.Id == filter.AdmissionSessionId)).ToList(),
            ProgramList = programs.OrderBy(p => p.AcademicClass?.ClassSerial)
                .Select(p => new SelectListItem(p.AcademicClass?.Name, p.Id.ToString(), p.Id == filter.AdmissionProgramId)).ToList(),
            StatusList = Enum.GetValues<AdmissionApplicationStatus>()
                .Where(s => s != AdmissionApplicationStatus.Draft)
                .Select(s => new SelectListItem(AdmissionDisplay.Name(s), s.ToString(), s == filter.Status)).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ReviewAdmissionApplicationsPolicy")]
    public async Task<IActionResult> BulkStartReview(int[] selectedIds, int? sessionId)
    {
        var result = await _applicationManager.BulkStartReviewAsync(selectedIds ?? Array.Empty<int>(), Actor);
        TempData["success"] = $"{result.Id ?? 0} application(s) moved to review.";
        return RedirectToAction(nameof(Index), new Dictionary<string, string> { ["Filter.AdmissionSessionId"] = sessionId?.ToString() });
    }

    #endregion Dashboard & list

    #region Details & review

    [Authorize(Policy = "IndexAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var application = await _applicationManager.GetFullAsync(id);
        if (application == null || application.Status == AdmissionApplicationStatus.Draft)
            return NotFound();

        var payable = application.Status == AdmissionApplicationStatus.PaymentPending || application.Status == AdmissionApplicationStatus.PaymentVerificationPending;
        var model = new AdmissionApplicationDetailsVM
        {
            Application = application,
            MissingItems = await _applicationManager.GetMissingItemsAsync(application),
            Outstanding = _paymentManager.GetOutstanding(application),
            PendingAmount = _paymentManager.GetPendingAmount(application),
            EnrolmentBlocker = _applicationManager.GetEnrolmentBlocker(application, hasExceptionReason: payable),
            EnrolmentNeedsException = payable,
            SeatsHeld = (await _applicationManager.GetSeatUsageAsync(application.AdmissionSessionId))
                .FirstOrDefault(u => u.AdmissionProgramId == application.AdmissionProgramId)?.SeatsHeld ?? 0,
            LastCorrectionChanges = CompareLastSubmissions(application)
        };

        var staffIds = application.Payments.Select(p => p.VerifiedBy)
            .Concat(application.Documents.Select(d => d.VerifiedBy))
            .Append(application.DecisionBy).Append(application.EnrolledBy)
            .Where(x => !string.IsNullOrEmpty(x)).Distinct();
        foreach (var staffId in staffIds)
        {
            var user = await _userManager.FindByIdAsync(staffId);
            model.StaffNames[staffId] = user?.UserName ?? "(removed user)";
        }
        return View(model);
    }

    /// <summary>Fields the guardian changed between the previous and the latest submission.</summary>
    private static List<(string, string, string)> CompareLastSubmissions(AdmissionApplication application)
    {
        var snapshots = application.History.Where(h => !string.IsNullOrEmpty(h.Snapshot)).OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).ToList();
        var changes = new List<(string, string, string)>();
        if (snapshots.Count < 2)
            return changes;

        try
        {
            var before = JsonSerializer.Deserialize<Dictionary<string, string>>(snapshots[^2].Snapshot);
            var after = JsonSerializer.Deserialize<Dictionary<string, string>>(snapshots[^1].Snapshot);
            foreach (var key in after.Keys.Union(before.Keys))
            {
                before.TryGetValue(key, out var b);
                after.TryGetValue(key, out var a);
                if (!string.Equals(b ?? string.Empty, a ?? string.Empty, StringComparison.Ordinal))
                    changes.Add((key, b, a));
            }
        }
        catch (JsonException)
        {
            // A damaged snapshot only hides the comparison; the history itself is still shown.
        }
        return changes;
    }

    /// <summary>Streams an applicant's upload (photo, document, payment proof) to authorised staff.</summary>
    [Authorize(Policy = "IndexAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Download(int id, string name)
    {
        var application = await _applicationManager.GetFullAsync(id);
        if (application == null || string.IsNullOrEmpty(name)
            || !(application.PhotoFileName == name || application.Documents.Any(d => d.StoredFileName == name) || application.Payments.Any(p => p.ProofFileName == name)))
            return NotFound();

        var stream = _fileStore.OpenRead(id, name);
        return stream == null ? NotFound() : File(stream, AdmissionFileStore.ContentTypeFor(name));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ReviewAdmissionApplicationsPolicy")]
    public async Task<IActionResult> StartReview(int id) =>
        Done(id, await _applicationManager.StartReviewAsync(id, Actor), "The application is now under review.");

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ReviewAdmissionApplicationsPolicy")]
    public async Task<IActionResult> VerifyDocument(int id, int documentId, DocumentVerificationStatus status, string remarks) =>
        Done(id, await _applicationManager.VerifyDocumentAsync(documentId, status, remarks, Actor), "Document " + status.ToString().ToLowerInvariant() + ".");

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ReviewAdmissionApplicationsPolicy")]
    public async Task<IActionResult> RequestCorrection(int id, string reason, DateTime? deadline)
    {
        var result = await _applicationManager.RequestCorrectionAsync(id, reason, deadline, Actor);
        if (result.Succeeded)
            await _notifier.CorrectionRequestedAsync(await _applicationManager.GetFullAsync(id));
        return Done(id, result, "Correction requested. The guardian has been notified.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ReviewAdmissionApplicationsPolicy")]
    public async Task<IActionResult> AddNote(int id, string note, bool isInternal)
    {
        var result = await _applicationManager.AddNoteAsync(id, note, isInternal, Actor);
        if (result.Succeeded && !isInternal)
            await _notifier.GuardianMessageAsync(await _applicationManager.GetFullAsync(id));
        return Done(id, result, isInternal ? "Internal note added." : "Message sent to the guardian.");
    }

    #endregion Details & review

    #region Decisions

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "DecideAdmissionApplicationsPolicy")]
    public async Task<IActionResult> EligibilityException(int id, string reason) =>
        Done(id, await _applicationManager.RecordEligibilityExceptionAsync(id, reason, Actor), "Eligibility exception recorded.");

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "DecideAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Approve(int id, string note)
    {
        var result = await _applicationManager.ApproveAsync(id, note, Actor);
        if (result.Succeeded)
            await _notifier.ApprovedAsync(await _applicationManager.GetFullAsync(id));
        return Done(id, result, "Application approved and the offer sent to the guardian.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "DecideAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var result = await _applicationManager.RejectAsync(id, reason, Actor);
        if (result.Succeeded)
            await _notifier.RejectedAsync(await _applicationManager.GetFullAsync(id));
        return Done(id, result, "Application rejected.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "DecideAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Waitlist(int id, string reason)
    {
        var result = await _applicationManager.WaitlistAsync(id, reason, Actor);
        if (result.Succeeded)
            await _notifier.WaitlistedAsync(await _applicationManager.GetFullAsync(id));
        return Done(id, result, "Application waitlisted.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "DecideAdmissionApplicationsPolicy")]
    public async Task<IActionResult> ExpireOffer(int id) =>
        Done(id, await _applicationManager.ExpireOfferAsync(id, Actor), "The offer has been marked expired.");

    #endregion Decisions

    #region Enrolment

    [Authorize(Policy = "EnrolAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Enrol(int id)
    {
        var application = await _applicationManager.GetFullAsync(id);
        if (application == null)
            return NotFound();
        var model = await BuildEnrolModelAsync(application, new AdmissionEnrolVM());
        model.RollNo = await _applicationManager.SuggestRollAsync(id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "EnrolAdmissionApplicationsPolicy")]
    public async Task<IActionResult> Enrol(int id, AdmissionEnrolVM model)
    {
        var result = await _applicationManager.EnrolAsync(new AdmissionEnrolmentRequest
        {
            ApplicationId = id,
            AcademicSectionId = model.AcademicSectionId > 0 ? model.AcademicSectionId : null,
            RollNo = model.RollNo,
            AdmissionDate = model.AdmissionDate,
            ExceptionReason = model.ExceptionReason
        }, Actor);

        var application = await _applicationManager.GetFullAsync(id);
        if (application == null)
            return NotFound();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(await BuildEnrolModelAsync(application, model));
        }

        // The student record is committed. Photo and login are follow-ups: if either fails the
        // enrolment stands and staff can finish it from the Students screens.
        var studentId = result.Id.Value;
        var uniqueId = result.Value;
        var warnings = new List<string>();

        if (!string.IsNullOrEmpty(application.PhotoFileName))
        {
            try
            {
                var photoName = _applicationManager.BuildStudentPhotoFileName(application, uniqueId, Path.GetExtension(application.PhotoFileName));
                var destination = Path.Combine(_host.WebRootPath, "Images", "Student", photoName);
                if (_fileStore.CopyTo(id, application.PhotoFileName, destination))
                    await _applicationManager.SetStudentPhotoAsync(studentId, photoName);
                else
                    warnings.Add("the photo file was missing and was not copied");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add("the photo could not be copied");
                await _appLogger.ErrorAsync("Admission enrolment photo copy failed for application " + id, ex.Message);
            }
        }

        var password = await CreateStudentLoginAsync(application, uniqueId, warnings);
        await _notifier.EnrolledAsync(application, uniqueId, password);

        TempData["success"] = $"Enrolled. Student ID {uniqueId}." + (warnings.Count > 0 ? " Note: " + string.Join("; ", warnings) + "." : string.Empty);
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Same login the Students > Create screen creates: username = student ID, role Student.</summary>
    private async Task<string> CreateStudentLoginAsync(AdmissionApplication application, string uniqueId, List<string> warnings)
    {
        try
        {
            if (await _userManager.FindByNameAsync(uniqueId) != null)
            {
                warnings.Add("a login with this student ID already existed, so no new one was created");
                return null;
            }

            var user = new ApplicationUser
            {
                UserName = uniqueId,
                Email = application.Email,
                EmailConfirmed = true,
                PhoneNumber = application.GuardianPhone,
                PhoneNumberConfirmed = true,
                UserType = 's',
                ReferenceId = Convert.ToInt32(uniqueId)
            };
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var password = new string(Enumerable.Range(0, 8).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                warnings.Add("the student login could not be created (" + string.Join(", ", created.Errors.Select(e => e.Description)) + ")");
                return null;
            }
            await _userManager.AddToRoleAsync(user, "Student");
            return password;
        }
        catch (Exception ex)
        {
            warnings.Add("the student login could not be created");
            await _appLogger.ErrorAsync("Admission enrolment login creation failed for " + uniqueId, ex.Message);
            return null;
        }
    }

    private async Task<AdmissionEnrolVM> BuildEnrolModelAsync(AdmissionApplication application, AdmissionEnrolVM model)
    {
        model.Application = application;
        model.NeedsException = application.Status == AdmissionApplicationStatus.PaymentPending
                               || application.Status == AdmissionApplicationStatus.PaymentVerificationPending;
        model.Blocker = _applicationManager.GetEnrolmentBlocker(application, hasExceptionReason: model.NeedsException);
        var sections = await _academicSectionManager.GetAllByClassWithSessionId(
            application.AdmissionProgram.AcademicClassId, application.AdmissionSession.AcademicSessionId);
        model.SectionList = sections.OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.AcademicSectionId)).ToList();
        return model;
    }

    #endregion Enrolment

    private IActionResult Done(int id, AdmissionResult result, string successMessage)
    {
        TempData[result.Succeeded ? "success" : "fail"] = result.Succeeded ? successMessage : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }
}
