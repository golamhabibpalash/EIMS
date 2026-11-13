using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SMS.BLL.Contracts;
using SMS_App.Utilities.OnlineAdmission;
using SMS_App.ViewModels.OnlineAdmission;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// Public Online Admission pages: the admission notice board, each session's landing page,
/// eligibility, guidelines, the first-time applicant manual and status tracking. No sign-in;
/// nothing private is shown here.
/// </summary>
[AllowAnonymous]
public class AdmissionController : Controller
{
    private readonly IAdmissionSetupManager _setupManager;
    private readonly IAdmissionApplicationManager _applicationManager;

    public AdmissionController(IAdmissionSetupManager setupManager, IAdmissionApplicationManager applicationManager)
    {
        _setupManager = setupManager;
        _applicationManager = applicationManager;
    }

    // GET: /Admission - the admission notice board
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var sessions = await _setupManager.GetPublishedSessionsAsync();
        var model = new AdmissionNoticeListVM
        {
            Notices = sessions
                .Select(s => new AdmissionNoticeCardVM { Session = s, State = _setupManager.GetWindowState(s, now) })
                .OrderBy(n => n.State == SMS.Entities.Enums.AdmissionWindowState.Closed)
                .ThenBy(n => n.Session.ApplicationEndDate)
                .ToList()
        };
        return View(model);
    }

    // GET: /Admission/Notice/5 - the session's landing page
    public async Task<IActionResult> Notice(int id) => await LandingAsync(id, nameof(Notice));

    // GET: /Admission/Eligibility/5
    public async Task<IActionResult> Eligibility(int id) => await LandingAsync(id, nameof(Eligibility));

    // GET: /Admission/Guidelines/5
    public async Task<IActionResult> Guidelines(int id) => await LandingAsync(id, nameof(Guidelines));

    private async Task<IActionResult> LandingAsync(int id, string viewName)
    {
        var session = await _setupManager.GetPublicSessionAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        var usage = await _setupManager.GetSeatUsageAsync(id);
        var guardian = await HttpContext.AuthenticateAsync(GuardianPortalAuth.Scheme);
        return View(viewName, new AdmissionLandingVM
        {
            Session = session,
            State = _setupManager.GetWindowState(session, DateTime.Now),
            SeatUsage = usage.ToDictionary(u => u.AdmissionProgramId),
            IsSignedIn = guardian.Succeeded
        });
    }

    // GET: /Admission/Manual - first-time applicant user manual
    public async Task<IActionResult> Manual()
    {
        ViewBag.ManualNotes = (await _setupManager.GetSettingsAsync()).UserManualNotes;
        return View();
    }

    // GET: /Admission/Track
    public IActionResult Track() => View(new AdmissionTrackVM());

    // POST: /Admission/Track - status only; the application number and the guardian's mobile must
    // both match, and no personal details are shown (sign in for those).
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting(AdmissionRateLimits.Auth)]
    public async Task<IActionResult> Track(AdmissionTrackVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        model.Searched = true;
        model.Result = await _applicationManager.TrackAsync(model.ApplicationNo, model.MobileNumber);
        return View(model);
    }
}
