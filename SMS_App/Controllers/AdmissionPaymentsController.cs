using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.OnlineAdmission;
using SMS_App.ViewModels.OnlineAdmission;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// Finance desk for admission fees: verify or reject offline payments submitted by guardians,
/// record cash taken at the school, and print receipts. Only verified payments count as paid.
/// </summary>
public class AdmissionPaymentsController : Controller
{
    private readonly IAdmissionPaymentManager _paymentManager;
    private readonly IAdmissionApplicationManager _applicationManager;
    private readonly IAdmissionSetupManager _setupManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdmissionNotifier _notifier;
    private readonly AdmissionFileStore _fileStore;

    public AdmissionPaymentsController(
        IAdmissionPaymentManager paymentManager,
        IAdmissionApplicationManager applicationManager,
        IAdmissionSetupManager setupManager,
        UserManager<ApplicationUser> userManager,
        AdmissionNotifier notifier,
        AdmissionFileStore fileStore)
    {
        _fileStore = fileStore;
        _paymentManager = paymentManager;
        _applicationManager = applicationManager;
        _setupManager = setupManager;
        _userManager = userManager;
        _notifier = notifier;
    }

    private AdmissionActor Actor => AdmissionActor.Staff(_userManager.GetUserId(User), User.Identity?.Name, MACService.GetMAC());

    [Authorize(Policy = "IndexAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Index(AdmissionPaymentStatus? status = AdmissionPaymentStatus.PendingVerification, int? sessionId = null, bool all = false)
    {
        if (all)
            status = null;
        var sessions = await _setupManager.GetSessionsAsync(includeArchived: true);
        return View(new AdmissionPaymentListVM
        {
            Status = status,
            AdmissionSessionId = sessionId,
            SessionList = sessions.Select(s => new SelectListItem(s.Title, s.Id.ToString(), s.Id == sessionId)).ToList(),
            Payments = await _paymentManager.SearchAsync(status, sessionId)
        });
    }

    [Authorize(Policy = "IndexAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var payment = await _paymentManager.GetFullAsync(id);
        if (payment == null)
            return NotFound();
        ViewBag.Outstanding = _paymentManager.GetOutstanding(await _applicationManager.GetFullAsync(payment.AdmissionApplicationId));
        if (!string.IsNullOrEmpty(payment.VerifiedBy))
            ViewBag.VerifiedByName = (await _userManager.FindByIdAsync(payment.VerifiedBy))?.UserName;
        return View(payment);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "VerifyAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Verify(int id, decimal verifiedAmount, string reference, string remarks)
    {
        var result = await _paymentManager.VerifyAsync(id, verifiedAmount, reference, remarks, Actor);
        if (result.Succeeded)
        {
            var payment = await _paymentManager.GetFullAsync(id);
            await _notifier.PaymentVerifiedAsync(await _applicationManager.GetFullAsync(payment.AdmissionApplicationId), result.Value);
            TempData["success"] = $"Payment verified. Receipt {result.Value}.";
        }
        else
        {
            TempData["fail"] = result.Error;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "VerifyAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Reject(int id, bool needsCorrection, string reason)
    {
        var result = await _paymentManager.RejectAsync(id, needsCorrection, reason, Actor);
        if (result.Succeeded)
        {
            var payment = await _paymentManager.GetFullAsync(id);
            await _notifier.PaymentRejectedAsync(await _applicationManager.GetFullAsync(payment.AdmissionApplicationId));
            TempData["success"] = needsCorrection ? "The guardian has been asked to correct the payment details." : "Payment rejected.";
        }
        else
        {
            TempData["fail"] = result.Error;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "VerifyAdmissionPaymentsPolicy")]
    public async Task<IActionResult> RecordCash(int applicationId, decimal amount, DateTime paidOn, string reference, string remarks)
    {
        var result = await _paymentManager.RecordCashPaymentAsync(applicationId, amount, paidOn, reference, remarks, Actor);
        if (result.Succeeded)
        {
            await _notifier.PaymentVerifiedAsync(await _applicationManager.GetFullAsync(applicationId), result.Value);
            TempData["success"] = $"Cash payment recorded. Receipt {result.Value}.";
            return RedirectToAction(nameof(Receipt), new { id = result.Id });
        }
        TempData["fail"] = result.Error;
        return RedirectToAction(nameof(AdmissionApplicationsController.Details), "AdmissionApplications", new { id = applicationId });
    }

    [Authorize(Policy = "IndexAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Receipt(int id)
    {
        var payment = await _paymentManager.GetFullAsync(id);
        if (payment == null || payment.Status != AdmissionPaymentStatus.Verified)
            return NotFound();
        return View(payment);
    }

    [Authorize(Policy = "IndexAdmissionPaymentsPolicy")]
    public async Task<IActionResult> Proof(int id)
    {
        var payment = await _paymentManager.GetFullAsync(id);
        if (payment == null || string.IsNullOrEmpty(payment.ProofFileName))
            return NotFound();
        var stream = _fileStore.OpenRead(payment.AdmissionApplicationId, payment.ProofFileName);
        return stream == null ? NotFound() : File(stream, AdmissionFileStore.ContentTypeFor(payment.ProofFileName));
    }
}
