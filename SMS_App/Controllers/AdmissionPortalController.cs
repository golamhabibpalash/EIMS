using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.OnlineAdmission;
using SMS_App.ViewModels.OnlineAdmission;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// The guardian (applicant) portal: account, multi-step application form with draft saving,
/// document upload, submission, tracking, corrections and offline payment. Guardians sign in
/// with their own cookie scheme (see <see cref="GuardianPortalControllerBase"/>) and can only
/// ever reach their own applications.
/// </summary>
public class AdmissionPortalController : GuardianPortalControllerBase
{
    private const string PendingResetMobileKey = "AdmissionResetMobile";

    private readonly IAdmissionGuardianManager _guardianManager;
    private readonly IAdmissionSetupManager _setupManager;
    private readonly IAdmissionApplicationManager _applicationManager;
    private readonly IAdmissionPaymentManager _paymentManager;
    private readonly IGenderManager _genderManager;
    private readonly IReligionManager _religionManager;
    private readonly INationalityManager _nationalityManager;
    private readonly IBloodGroupManager _bloodGroupManager;
    private readonly IDivisionManager _divisionManager;
    private readonly IDistrictManager _districtManager;
    private readonly IUpazilaManager _upazilaManager;
    private readonly AdmissionFileStore _fileStore;
    private readonly AdmissionNotifier _notifier;
    private readonly IAppLogger _appLogger;

    public AdmissionPortalController(
        IAdmissionGuardianManager guardianManager,
        IAdmissionSetupManager setupManager,
        IAdmissionApplicationManager applicationManager,
        IAdmissionPaymentManager paymentManager,
        IGenderManager genderManager,
        IReligionManager religionManager,
        INationalityManager nationalityManager,
        IBloodGroupManager bloodGroupManager,
        IDivisionManager divisionManager,
        IDistrictManager districtManager,
        IUpazilaManager upazilaManager,
        AdmissionFileStore fileStore,
        AdmissionNotifier notifier,
        IAppLogger appLogger)
        : base(guardianManager, setupManager)
    {
        _guardianManager = guardianManager;
        _setupManager = setupManager;
        _applicationManager = applicationManager;
        _paymentManager = paymentManager;
        _genderManager = genderManager;
        _religionManager = religionManager;
        _nationalityManager = nationalityManager;
        _bloodGroupManager = bloodGroupManager;
        _divisionManager = divisionManager;
        _districtManager = districtManager;
        _upazilaManager = upazilaManager;
        _fileStore = fileStore;
        _notifier = notifier;
        _appLogger = appLogger;
    }

    private AdmissionActor Actor => AdmissionActor.Guardian(GuardianId, Guardian?.FullName);

    #region Account

    [GuardianAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        if (Guardian != null)
            return RedirectToLocal(returnUrl);
        return View(new GuardianLoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> Login(GuardianLoginVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _guardianManager.SignInAsync(model.MobileNumber, model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        var account = await _guardianManager.GetByIdAsync(result.Id.Value);
        await GuardianPortalAuth.SignInAsync(HttpContext, account);
        return RedirectToLocal(model.ReturnUrl);
    }

    [GuardianAnonymous]
    public IActionResult Register(string returnUrl = null)
    {
        if (Guardian != null)
            return RedirectToLocal(returnUrl);
        return View(new GuardianRegisterVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> Register(GuardianRegisterVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _guardianManager.RegisterAsync(model.FullName, model.MobileNumber, model.Email, model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        var account = await _guardianManager.GetByIdAsync(result.Id.Value);
        await GuardianPortalAuth.SignInAsync(HttpContext, account);

        var settings = await _setupManager.GetSettingsAsync();
        if (settings.RequireOtpVerification)
        {
            await SendCodeAsync(account, IAdmissionGuardianManager.OtpPurposeVerify);
            return RedirectToAction(nameof(VerifyMobile), new { returnUrl = model.ReturnUrl });
        }

        TempData["success"] = "Your account is ready. You can now apply.";
        return RedirectToLocal(model.ReturnUrl);
    }

    /// <summary>Only reachable signed in; reachable while unverified (the base class sends unverified guardians here).</summary>
    [GuardianAnonymous]
    public IActionResult VerifyMobile(string returnUrl = null)
    {
        if (Guardian == null)
            return RedirectToAction(nameof(Login));
        if (Guardian.IsMobileVerified)
            return RedirectToLocal(returnUrl);
        ViewBag.ReturnUrl = returnUrl;
        return View(new GuardianOtpVM { MaskedMobile = Mask(Guardian.MobileNumber) });
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> VerifyMobile(GuardianOtpVM model, string returnUrl = null)
    {
        if (Guardian == null)
            return RedirectToAction(nameof(Login));

        model.MaskedMobile = Mask(Guardian.MobileNumber);
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var result = await _guardianManager.VerifyOtpAsync(GuardianId, IAdmissionGuardianManager.OtpPurposeVerify, model.Code);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        TempData["success"] = "Your mobile number is verified.";
        return RedirectToLocal(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> ResendCode(string returnUrl = null)
    {
        if (Guardian == null)
            return RedirectToAction(nameof(Login));
        await SendCodeAsync(Guardian, IAdmissionGuardianManager.OtpPurposeVerify);
        return RedirectToAction(nameof(VerifyMobile), new { returnUrl });
    }

    [GuardianAnonymous]
    public IActionResult ForgotPassword() => View(new GuardianForgotPasswordVM());

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> ForgotPassword(GuardianForgotPasswordVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var mobile = SMS.BLL.Managers.AdmissionRules.NormaliseMobile(model.MobileNumber);
        if (mobile == null)
        {
            ModelState.AddModelError(nameof(model.MobileNumber), "Please enter a valid Bangladeshi mobile number, e.g. 01712345678.");
            return View(model);
        }

        // Same response whether or not the number is registered, so the page can't be used to
        // discover who has an account.
        var account = await _guardianManager.GetByMobileAsync(mobile);
        if (account != null)
        {
            await SendCodeAsync(account, IAdmissionGuardianManager.OtpPurposeReset);
        }
        HttpContext.Session.SetString(PendingResetMobileKey, mobile);
        TempData["success"] = "If this number has an account, a reset code has been sent by SMS.";
        return RedirectToAction(nameof(ResetPassword));
    }

    [GuardianAnonymous]
    public IActionResult ResetPassword()
    {
        var mobile = HttpContext.Session.GetString(PendingResetMobileKey);
        if (string.IsNullOrEmpty(mobile))
            return RedirectToAction(nameof(ForgotPassword));
        ViewBag.MaskedMobile = Mask(mobile);
        return View(new GuardianResetPasswordVM { MobileNumber = mobile });
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> ResetPassword(GuardianResetPasswordVM model)
    {
        // The number comes from the session set by ForgotPassword, not from the form.
        var mobile = HttpContext.Session.GetString(PendingResetMobileKey);
        if (string.IsNullOrEmpty(mobile))
            return RedirectToAction(nameof(ForgotPassword));
        model.MobileNumber = mobile;
        ViewBag.MaskedMobile = Mask(mobile);
        ModelState.Remove(nameof(model.MobileNumber));
        if (!ModelState.IsValid)
            return View(model);

        var result = await _guardianManager.ResetPasswordAsync(mobile, model.Code, model.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        HttpContext.Session.Remove(PendingResetMobileKey);
        TempData["success"] = "Your password has been changed. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken, GuardianAnonymous]
    public async Task<IActionResult> Logout()
    {
        await GuardianPortalAuth.SignOutAsync(HttpContext);
        return RedirectToAction("Index", "Admission");
    }

    private async Task SendCodeAsync(AdmissionGuardianAccount account, string purpose)
    {
        var issued = await _guardianManager.IssueOtpAsync(account.Id, purpose);
        if (!issued.Succeeded)
        {
            TempData["fail"] = issued.Error;
            return;
        }
        if (!await _notifier.SendOtpAsync(account.MobileNumber, issued.Value))
        {
            TempData["fail"] = "We could not send the SMS code right now. Please try again in a few minutes, or call the school's helpline.";
        }
        else
        {
            TempData["success"] = $"A 6-digit code has been sent to {Mask(account.MobileNumber)}.";
        }
    }

    #endregion Account

    #region Dashboard & tracking

    // GET: /AdmissionPortal - "My applications"
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var sessions = await _setupManager.GetPublishedSessionsAsync();
        return View(new GuardianDashboardVM
        {
            Guardian = Guardian,
            Applications = await _applicationManager.GetForGuardianAsync(GuardianId),
            OpenSessions = sessions
                .Select(s => new AdmissionNoticeCardVM { Session = s, State = _setupManager.GetWindowState(s, now) })
                .Where(n => SMS.BLL.Managers.AdmissionRules.IsWindowOpen(n.State))
                .ToList()
        });
    }

    // POST: start a new application for an offered class
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int programId)
    {
        var result = await _applicationManager.StartDraftAsync(GuardianId, programId, Actor);
        if (!result.Succeeded)
        {
            TempData["fail"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Apply), new { id = result.Id });
    }

    // GET: /AdmissionPortal/Application/5 - status tracking, messages, receipts
    public async Task<IActionResult> Application(int id)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        if (application == null)
            return NotFound();
        if (application.Status == AdmissionApplicationStatus.Draft)
            return RedirectToAction(nameof(Apply), new { id });
        return View(application);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, string reason)
    {
        var result = await _applicationManager.WithdrawAsync(id, GuardianId, reason, Actor);
        TempData[result.Succeeded ? "success" : "fail"] = result.Succeeded ? "The application has been withdrawn." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    #endregion Dashboard & tracking

    #region Application form

    // GET: /AdmissionPortal/Apply/5?step=2
    public async Task<IActionResult> Apply(int id, int? step)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        if (application == null)
            return NotFound();
        if (!AdmissionRulesView.IsEditable(application))
            return RedirectToAction(nameof(Application), new { id });

        // Steps can be revisited freely, but not skipped past the furthest one reached.
        var current = Math.Clamp(step ?? application.CurrentStep, 1, Math.Max(1, application.CurrentStep));
        return View(await BuildFormAsync(application, current, application));
    }

    // POST: save one step. "next" moves on (step must be complete); "draft" just saves.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int id, int step, ApplicationFormVM model, string command)
    {
        var goNext = command != "draft";
        var goBack = command == "back";

        if (step >= IAdmissionApplicationManager.StepStudent && step <= IAdmissionApplicationManager.StepDocuments)
        {
            var result = await _applicationManager.SaveStepAsync(id, GuardianId, step, model.Form ?? new AdmissionApplication(), requireComplete: goNext && !goBack);
            if (!result.Succeeded)
            {
                var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
                if (application == null)
                    return NotFound();
                foreach (var line in result.Error.Split('\n'))
                    ModelState.AddModelError(string.Empty, line);
                // Re-show what the guardian typed, not the saved values.
                return View(await BuildFormAsync(application, step, model.Form));
            }
        }

        if (goBack)
            return RedirectToAction(nameof(Apply), new { id, step = Math.Max(1, step - 1) });
        if (!goNext)
        {
            TempData["success"] = "Draft saved. You can come back and finish it any time before the deadline.";
            return RedirectToAction(nameof(Apply), new { id, step });
        }
        return RedirectToAction(nameof(Apply), new { id, step = Math.Min(IAdmissionApplicationManager.StepReview, step + 1) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting(AdmissionRateLimits.Upload)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile photo)
    {
        var error = await _applicationManager.ValidatePhotoAsync(photo?.FileName, photo?.Length ?? 0);
        if (error == null && await _applicationManager.GetOwnedAsync(id, GuardianId) == null)
            return NotFound();

        if (error == null)
        {
            var (stored, saveError) = await _fileStore.SaveAsync(id, photo);
            error = saveError;
            if (stored != null)
            {
                var result = await _applicationManager.SetPhotoAsync(id, GuardianId, stored);
                if (result.Succeeded)
                {
                    _fileStore.TryDelete(id, result.Value);
                }
                else
                {
                    _fileStore.TryDelete(id, stored);
                    error = result.Error;
                }
            }
        }

        TempData[error == null ? "success" : "fail"] = error ?? "Photo uploaded.";
        return RedirectToAction(nameof(Apply), new { id, step = IAdmissionApplicationManager.StepStudent });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting(AdmissionRateLimits.Upload)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(int id, int requirementId, IFormFile file)
    {
        var error = await _applicationManager.ValidateDocumentUploadAsync(id, GuardianId, requirementId, file?.FileName, file?.Length ?? 0);
        if (error == null)
        {
            var (stored, saveError) = await _fileStore.SaveAsync(id, file);
            error = saveError;
            if (stored != null)
            {
                var result = await _applicationManager.AddDocumentAsync(id, GuardianId, requirementId, stored, file.FileName, file.ContentType, file.Length);
                if (result.Succeeded)
                {
                    _fileStore.TryDelete(id, result.Value);
                }
                else
                {
                    _fileStore.TryDelete(id, stored);
                    error = result.Error;
                }
            }
        }

        TempData[error == null ? "success" : "fail"] = error ?? "Document uploaded.";
        return RedirectToAction(nameof(Apply), new { id, step = IAdmissionApplicationManager.StepDocuments });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDocument(int id, int documentId)
    {
        var result = await _applicationManager.RemoveDocumentAsync(documentId, GuardianId);
        if (result.Succeeded)
        {
            _fileStore.TryDelete(result.Id.Value, result.Value);
            TempData["success"] = "Document removed.";
        }
        else
        {
            TempData["fail"] = result.Error;
        }
        return RedirectToAction(nameof(Apply), new { id, step = IAdmissionApplicationManager.StepDocuments });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, bool eligibilityAcknowledged, bool policyAcknowledged)
    {
        var result = await _applicationManager.SubmitAsync(id, GuardianId, eligibilityAcknowledged, policyAcknowledged, Actor);
        if (!result.Succeeded)
        {
            TempData["fail"] = result.Error;
            return RedirectToAction(nameof(Apply), new { id, step = IAdmissionApplicationManager.StepReview });
        }

        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        var resubmitted = application.Status == AdmissionApplicationStatus.Resubmitted;
        if (resubmitted)
            await _notifier.ApplicationResubmittedAsync(application);
        else
            await _notifier.ApplicationSubmittedAsync(application);
        await _notifier.NewSubmissionAsync(application, resubmitted);

        TempData["success"] = $"Your application has been submitted. Application number: {result.Value}. Please keep this number.";
        return RedirectToAction(nameof(Application), new { id });
    }

    /// <summary>Streams one of the guardian's own uploads (photo, document or payment proof).</summary>
    public async Task<IActionResult> Download(int id, string name)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        if (application == null || !BelongsTo(application, name))
            return NotFound();

        var stream = _fileStore.OpenRead(id, name);
        return stream == null ? NotFound() : File(stream, AdmissionFileStore.ContentTypeFor(name));
    }

    private static bool BelongsTo(AdmissionApplication application, string storedName) =>
        !string.IsNullOrEmpty(storedName)
        && (application.PhotoFileName == storedName
            || application.Documents.Any(d => d.StoredFileName == storedName)
            || application.Payments.Any(p => p.ProofFileName == storedName));

    private async Task<ApplicationFormVM> BuildFormAsync(AdmissionApplication application, int step, AdmissionApplication formValues)
    {
        var model = new ApplicationFormVM
        {
            Step = step,
            Application = application,
            Form = formValues ?? application,
            Session = await _setupManager.GetSessionDetailsAsync(application.AdmissionSessionId),
            Program = application.AdmissionProgram
        };

        if (step == IAdmissionApplicationManager.StepStudent)
        {
            model.GenderList = ToSelectList(await _genderManager.GetAllAsync(), g => g.Id, g => g.Name, model.Form.GenderId);
            model.ReligionList = ToSelectList(await _religionManager.GetAllAsync(), r => r.Id, r => r.Name, model.Form.ReligionId);
            model.NationalityList = ToSelectList(await _nationalityManager.GetAllAsync(), n => n.Id, n => n.Name, model.Form.NationalityId);
            model.BloodGroupList = ToSelectList(await _bloodGroupManager.GetAllAsync(), b => b.Id, b => b.Name, model.Form.BloodGroupId);
        }
        else if (step == IAdmissionApplicationManager.StepGuardian)
        {
            model.DivisionList = ToSelectList(await _divisionManager.GetAllAsync(), d => d.Id, d => d.Name, null);
            model.DistrictsJson = JsonConvert.SerializeObject((await _districtManager.GetAllAsync())
                .OrderBy(d => d.Name).Select(d => new { id = d.Id, name = d.Name, parentId = d.DivisionId }));
            model.UpazilasJson = JsonConvert.SerializeObject((await _upazilaManager.GetAllAsync())
                .OrderBy(u => u.Name).Select(u => new { id = u.Id, name = u.Name, parentId = u.DistrictId }));
        }
        else if (step == IAdmissionApplicationManager.StepReview)
        {
            model.MissingItems = await _applicationManager.GetMissingItemsAsync(application);
            model.EligibilityWarnings = await _applicationManager.GetEligibilityWarningsAsync(application);
        }
        return model;
    }

    private static List<SelectListItem> ToSelectList<T>(IEnumerable<T> items, Func<T, int> id, Func<T, string> name, int? selected) =>
        items.OrderBy(name).Select(i => new SelectListItem(name(i), id(i).ToString(), selected == id(i))).ToList();

    #endregion Application form

    #region Payment

    // GET: /AdmissionPortal/Payment/5
    public async Task<IActionResult> Payment(int id)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        if (application == null)
            return NotFound();
        return View(BuildPaymentModel(application, new OfflinePaymentVM()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting(AdmissionRateLimits.Upload)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Payment(int id, OfflinePaymentVM model)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        if (application == null)
            return NotFound();
        if (!ModelState.IsValid)
            return View(BuildPaymentModel(application, model));

        string proof = null;
        if (model.Proof != null && model.Proof.Length > 0)
        {
            var proofError = await _applicationManager.ValidatePhotoAsync(model.Proof.FileName, model.Proof.Length);
            if (proofError != null && !model.Proof.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Proof), proofError);
                return View(BuildPaymentModel(application, model));
            }
            if (model.Proof.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.Proof), "The proof file must be 2 MB or smaller.");
                return View(BuildPaymentModel(application, model));
            }
            var (stored, saveError) = await _fileStore.SaveAsync(id, model.Proof);
            if (stored == null)
            {
                ModelState.AddModelError(nameof(model.Proof), saveError);
                return View(BuildPaymentModel(application, model));
            }
            proof = stored;
        }

        var result = await _paymentManager.SubmitOfflinePaymentAsync(id, GuardianId, model.Channel, model.Amount, model.PaidOn,
            model.TransactionReference, proof, Actor);
        if (!result.Succeeded)
        {
            if (proof != null)
                _fileStore.TryDelete(id, proof);
            ModelState.AddModelError(string.Empty, result.Error);
            return View(BuildPaymentModel(application, model));
        }

        await _notifier.OfflinePaymentSubmittedAsync(application, model.Amount);
        TempData["success"] = "Thank you. Your payment details were sent to the school. The payment will show as paid once the school verifies it.";
        return RedirectToAction(nameof(Application), new { id });
    }

    private OfflinePaymentVM BuildPaymentModel(AdmissionApplication application, OfflinePaymentVM model)
    {
        model.ApplicationId = application.Id;
        model.Application = application;
        model.Outstanding = _paymentManager.GetOutstanding(application);
        model.PendingAmount = _paymentManager.GetPendingAmount(application);
        model.Blocker = _paymentManager.GetPaymentBlocker(application);
        if (model.Amount <= 0)
            model.Amount = Math.Max(0, model.Outstanding - model.PendingAmount);
        return model;
    }

    // GET: /AdmissionPortal/Receipt/12 - receipt for a verified payment
    public async Task<IActionResult> Receipt(int id, int paymentId)
    {
        var application = await _applicationManager.GetOwnedAsync(id, GuardianId);
        var payment = application?.Payments.FirstOrDefault(p => p.Id == paymentId && p.Status == AdmissionPaymentStatus.Verified);
        if (payment == null)
            return NotFound();
        ViewBag.Payment = payment;
        return View(application);
    }

    #endregion Payment

    private IActionResult RedirectToLocal(string returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));

    private static string Mask(string mobile) =>
        string.IsNullOrEmpty(mobile) || mobile.Length < 4 ? mobile : mobile.Substring(0, 3) + new string('*', mobile.Length - 6) + mobile[^3..];
}

/// <summary>View-side shortcuts over the BLL rules.</summary>
public static class AdmissionRulesView
{
    public static bool IsEditable(AdmissionApplication application) =>
        SMS.BLL.Managers.AdmissionRules.IsEditableByGuardian(application.Status);
}
