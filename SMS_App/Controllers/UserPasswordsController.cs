using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Entities;
using SMS_App.Utilities.LoggerService;
using SMS_App.ViewModels.AdministrationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// Lets a user holding the "Reset User Password" permission set a new password for another
/// account, for people who have no email (or where email/SMS reset is unavailable). Gated only
/// by the permission, not by role. A SuperAdmin's password can never be set from here.
/// </summary>
[Authorize(Policy = "ResetUserPasswordPolicy")]
public class UserPasswordsController : Controller
{
    public const string SuperAdminRole = "SuperAdmin";
    public const string SuperAdminRefusal =
        "This user is a SuperAdmin. The password of a SuperAdmin account cannot be reset or set by anyone else. A SuperAdmin must change their own password.";

    private const int PageSize = 50;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppLogger _appLogger;

    public UserPasswordsController(UserManager<ApplicationUser> userManager, IAppLogger appLogger)
    {
        _userManager = userManager;
        _appLogger = appLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string search)
    {
        search = search?.Trim();
        var query = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
        {
            var upper = search.ToUpperInvariant();
            query = query.Where(u =>
                u.NormalizedUserName.Contains(upper) ||
                u.NormalizedEmail.Contains(upper) ||
                u.PhoneNumber.Contains(search));
        }

        var users = await query.OrderBy(u => u.UserName).Take(PageSize + 1).ToListAsync();
        var superAdminIds = (await _userManager.GetUsersInRoleAsync(SuperAdminRole)).Select(u => u.Id).ToHashSet();

        var model = new UserPasswordIndexVM
        {
            Search = search,
            HasMore = users.Count > PageSize,
            Users = users.Take(PageSize).Select(u => new UserPasswordListItemVM
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                UserType = UserTypeName(u.UserType),
                IsSuperAdmin = superAdminIds.Contains(u.Id),
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
            }).ToList()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Reset(string id, string search)
    {
        var user = string.IsNullOrEmpty(id) ? null : await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        if (await _userManager.IsInRoleAsync(user, SuperAdminRole))
        {
            TempData["error"] = SuperAdminRefusal;
            return RedirectToAction(nameof(Index), new { search });
        }
        return View(ToResetVM(user, search));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(UserPasswordResetVM model)
    {
        var user = string.IsNullOrEmpty(model.UserId) ? null : await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        // Checked again on post: the page could be stale or the request crafted by hand.
        if (await _userManager.IsInRoleAsync(user, SuperAdminRole))
        {
            await _appLogger.WarningAsync($"Password reset refused: {User.Identity?.Name} tried to set the password of SuperAdmin {user.UserName}.");
            TempData["error"] = SuperAdminRefusal;
            return RedirectToAction(nameof(Index), new { search = model.Search });
        }

        if (!ModelState.IsValid)
        {
            return View(Refill(model, user));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.NewPassword), error.Description);
            }
            return View(Refill(model, user));
        }

        // A user locked out after wrong attempts can sign in with the new password straight away.
        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        // Never log the password itself.
        await _appLogger.InfoAsync($"Password reset: {User.Identity?.Name} set a new password for {user.UserName}.");

        TempData["success"] = $"The password for {user.UserName} has been changed. Their old password no longer works; give them the new one.";
        return RedirectToAction(nameof(Index), new { search = model.Search });
    }

    private static UserPasswordResetVM ToResetVM(ApplicationUser user, string search) => new()
    {
        UserId = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        UserType = UserTypeName(user.UserType),
        Search = search
    };

    private static UserPasswordResetVM Refill(UserPasswordResetVM posted, ApplicationUser user)
    {
        var vm = ToResetVM(user, posted.Search);
        // Passwords are not echoed back into the form.
        return vm;
    }

    private static string UserTypeName(char type) => type switch
    {
        's' => "Student",
        'e' => "Employee",
        _ => "Other"
    };
}
