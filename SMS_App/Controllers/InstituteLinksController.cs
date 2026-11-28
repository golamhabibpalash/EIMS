using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.InstituteVM;

namespace SMS_App.Controllers;

/// <summary>
/// CRUD for the institute's external links (website, social media, other resources), shown as a
/// section on the <c>/Institutes</c> profile page.
///
/// The system holds exactly one institute, so every action resolves that institute server-side
/// (<see cref="IInstituteManager.GetFirstOrDefaultAsync"/>) and never trusts an InstituteId from
/// the client. Edit/Delete/Toggle additionally verify the target link belongs to that institute.
///
/// Authorization mirrors <c>InstitutesController</c>: the same SuperAdmin/Admin roles and the
/// existing Institute claim policies, so no new claims need seeding on each instance.
/// </summary>
[Authorize(Roles = "SuperAdmin, Admin")]
public class InstituteLinksController : Controller
{
    private readonly IInstituteLinkManager _linkManager;
    private readonly IInstituteManager _instituteManager;
    private readonly ILogger<InstituteLinksController> _logger;

    public InstituteLinksController(
        IInstituteLinkManager linkManager,
        IInstituteManager instituteManager,
        ILogger<InstituteLinksController> logger)
    {
        _linkManager = linkManager;
        _instituteManager = instituteManager;
        _logger = logger;
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<IActionResult> Create(InstituteLinkFormVM model)
    {
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
        {
            TempData["error"] = "Set up the institute profile before adding links.";
            return RedirectToAction("Index", "Institutes");
        }

        if (!ModelState.IsValid)
        {
            TempData["error"] = FirstError();
            return RedirectToAction("Index", "Institutes");
        }

        var link = new InstituteLink
        {
            InstituteId = institute.Id,
            LinkTitle = model.LinkTitle.Trim(),
            LinkType = model.LinkType,
            Url = model.Url.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
        Stamp(link, isNew: true);

        if (await TrySaveAsync(() => _linkManager.AddAsync(link), "create"))
        {
            TempData["created"] = "Link added successfully.";
        }
        return RedirectToAction("Index", "Institutes");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<IActionResult> Edit(InstituteLinkFormVM model)
    {
        var link = await ResolveOwnedLinkAsync(model.Id);
        if (link == null)
        {
            return RedirectToAction("Index", "Institutes");
        }

        if (!ModelState.IsValid)
        {
            TempData["error"] = FirstError();
            return RedirectToAction("Index", "Institutes");
        }

        link.LinkTitle = model.LinkTitle.Trim();
        link.LinkType = model.LinkType;
        link.Url = model.Url.Trim();
        link.DisplayOrder = model.DisplayOrder;
        link.IsActive = model.IsActive;
        Stamp(link, isNew: false);

        if (await TrySaveAsync(() => _linkManager.UpdateAsync(link), "update"))
        {
            TempData["updated"] = "Link updated successfully.";
        }
        return RedirectToAction("Index", "Institutes");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var link = await ResolveOwnedLinkAsync(id);
        if (link == null)
        {
            return RedirectToAction("Index", "Institutes");
        }

        link.IsActive = !link.IsActive;
        Stamp(link, isNew: false);

        if (await TrySaveAsync(() => _linkManager.UpdateAsync(link), "toggle"))
        {
            TempData["updated"] = link.IsActive ? "Link enabled." : "Link disabled.";
        }
        return RedirectToAction("Index", "Institutes");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var link = await ResolveOwnedLinkAsync(id);
        if (link == null)
        {
            return RedirectToAction("Index", "Institutes");
        }

        if (await TrySaveAsync(() => _linkManager.RemoveAsync(link), "delete"))
        {
            TempData["deleted"] = "Link deleted successfully.";
        }
        return RedirectToAction("Index", "Institutes");
    }

    /// <summary>
    /// Loads a link by id and confirms it belongs to this deployment's institute. Sets a TempData
    /// error and returns null when the link is missing or belongs elsewhere, so callers can bail out
    /// without trusting the client-supplied id.
    /// </summary>
    private async Task<InstituteLink> ResolveOwnedLinkAsync(int id)
    {
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        var link = id > 0 ? await _linkManager.GetByIdAsync(id) : null;
        if (institute == null || link == null || link.InstituteId != institute.Id)
        {
            TempData["error"] = "The link was not found.";
            return null;
        }
        return link;
    }

    private void Stamp(InstituteLink link, bool isNew)
    {
        var now = DateTime.Now;
        var user = HttpContext.Session.GetString("UserId") ?? User.Identity?.Name;
        if (isNew)
        {
            link.CreatedAt = now;
            link.CreatedBy = user;
        }
        link.EditedAt = now;
        link.EditedBy = user;
        link.MACAddress = MACService.GetMAC();
    }

    private async Task<bool> TrySaveAsync(Func<Task<bool>> save, string operation)
    {
        try
        {
            if (await save())
            {
                return true;
            }
            TempData["error"] = "The link could not be saved. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Institute link {Operation} failed", operation);
            TempData["error"] = "The link could not be saved. The error has been logged.";
        }
        return false;
    }

    private string FirstError() =>
        "Please check the link details: " + string.Join(" ", ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => e.Value.Errors.First().ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct());
}
