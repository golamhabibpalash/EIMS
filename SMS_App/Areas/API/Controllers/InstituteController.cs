using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.Institutes;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Institute branding for the SchoolBD mobile app: the single institute row's
/// name, logo, EIIN and contact details.
///
/// Anonymous on purpose. The mobile app shows this branding on its login screen
/// before any token exists, the same way the web layout loads the institute
/// logo server-side for the login page. It exposes only public branding — no
/// student, staff or financial data.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
public class InstituteController : ControllerBase
{
    private readonly IInstituteManager _instituteManager;

    public InstituteController(IInstituteManager instituteManager)
    {
        _instituteManager = instituteManager;
    }

    /// <summary>
    /// Branding for the deployment. <c>logo</c> is a path relative to the
    /// instance root (e.g. <c>/Images/Institute/instituteLogo_x.png</c>) and is
    /// null when the institute has not uploaded one, so the app keeps its
    /// bundled default.
    /// </summary>
    [HttpGet("profile")]
    [AllowAnonymous]
    public async Task<IActionResult> Profile()
    {
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
            return NotFound(new { message = "Institute is not configured." });

        return Ok(new
        {
            name = institute.Name,
            shortName = institute.ShortName,
            eiin = institute.EIIN,
            address = institute.Address,
            phone = institute.Phone1,
            email = institute.Email,
            slogan = institute.Slogan,
            logo = string.IsNullOrWhiteSpace(institute.Logo)
                ? null
                : $"/Images/Institute/{institute.Logo}",
            isOpen = IsOpenNow(institute),
        });
    }

    /// <summary>
    /// Whether the current time falls within the institute's opening hours, or
    /// null when no hours are configured (both times default to midnight).
    /// Uses Bangladesh time: the Linux host's own clock is UTC.
    /// </summary>
    private static bool? IsOpenNow(Institute institute) =>
        InstituteClock.IsOpen(
            institute.StartingTime.TimeOfDay,
            institute.ClosingTime.TimeOfDay,
            InstituteClock.Now().TimeOfDay);
}
