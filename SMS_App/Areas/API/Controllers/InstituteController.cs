using System;
using System.Linq;
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
    private readonly IInstituteLinkManager _linkManager;

    public InstituteController(IInstituteManager instituteManager, IInstituteLinkManager linkManager)
    {
        _instituteManager = instituteManager;
        _linkManager = linkManager;
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
    /// The institute's published external links (website, social media, other resources) for the
    /// mobile app. Anonymous and active-only, like <see cref="Profile"/> — these are public
    /// branding the app can show before sign-in. Ordered by DisplayOrder then title.
    /// </summary>
    [HttpGet("links")]
    [AllowAnonymous]
    public async Task<IActionResult> Links()
    {
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
            return NotFound(new { message = "Institute is not configured." });

        var links = await _linkManager.GetActiveByInstituteIdAsync(institute.Id);

        return Ok(links.Select(l => new
        {
            id = l.Id,
            instituteId = l.InstituteId,
            title = l.LinkTitle,
            type = l.LinkType.ToString(),
            url = l.Url,
            displayOrder = l.DisplayOrder,
            isActive = l.IsActive,
        }));
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
