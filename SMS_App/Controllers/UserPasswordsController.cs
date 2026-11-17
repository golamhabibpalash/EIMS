using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
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

    // How many matching students/employees a search may pull in by name/roll/Id.
    private const int SearchMatchLimit = 500;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppLogger _appLogger;
    private readonly IStudentManager _studentManager;
    private readonly IEmployeeManager _employeeManager;

    public UserPasswordsController(UserManager<ApplicationUser> userManager, IAppLogger appLogger,
        IStudentManager studentManager, IEmployeeManager employeeManager)
    {
        _userManager = userManager;
        _appLogger = appLogger;
        _studentManager = studentManager;
        _employeeManager = employeeManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string search, string type, int page = 1, int pageSize = 25)
    {
        search = search?.Trim();
        type = type is "s" or "e" or "o" ? type : "";
        if (!UserPasswordIndexVM.PageSizes.Contains(pageSize))
        {
            pageSize = UserPasswordIndexVM.PageSizes[0];
        }

        var query = _userManager.Users.AsNoTracking();
        query = type switch
        {
            "s" => query.Where(u => u.UserType == 's'),
            "e" => query.Where(u => u.UserType == 'e'),
            "o" => query.Where(u => u.UserType != 's' && u.UserType != 'e'),
            _ => query
        };

        if (!string.IsNullOrEmpty(search))
        {
            // Also find accounts by the linked student's name / roll / UniqueId or employee's name / Id.
            var students = await _studentManager.SearchForAccountsAsync(search, SearchMatchLimit);
            var employees = await _employeeManager.SearchForAccountsAsync(search, SearchMatchLimit);
            var studentRefs = students.Select(s => s.Id)
                .Concat(students.Select(s => int.TryParse(s.UniqueId, out var n) ? n : 0))
                .Where(n => n > 0).Distinct().ToList();
            var studentNames = students.Select(s => s.UniqueId).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();
            var employeeIds = employees.Select(e => e.Id).ToList();
            var upper = search.ToUpperInvariant();

            query = query.Where(u =>
                u.NormalizedUserName.Contains(upper) ||
                u.NormalizedEmail.Contains(upper) ||
                u.PhoneNumber.Contains(search) ||
                (u.UserType == 's' && (studentRefs.Contains(u.ReferenceId) || studentNames.Contains(u.UserName))) ||
                (u.UserType == 'e' && employeeIds.Contains(u.ReferenceId)));
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (total + pageSize - 1) / pageSize);
        page = Math.Clamp(page, 1, totalPages);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var superAdminIds = (await _userManager.GetUsersInRoleAsync(SuperAdminRole)).Select(u => u.Id).ToHashSet();
        var rows = users.Select(u => new UserPasswordListItemVM
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            UserType = UserTypeName(u.UserType),
            IsSuperAdmin = superAdminIds.Contains(u.Id),
            IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
        }).ToList();
        await FillLinkedPeopleAsync(users, rows);

        return View(new UserPasswordIndexVM
        {
            Search = search,
            Type = type,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Users = rows
        });
    }

    /// <summary>
    /// Adds the student's name + class roll or the employee's name + Id to one page of rows, with
    /// two batched queries (no per-row lookups). Student accounts store the UniqueId as user name
    /// and ReferenceId; a few older ones store the student's Id, so both are tried.
    /// </summary>
    private async Task FillLinkedPeopleAsync(List<ApplicationUser> users, List<UserPasswordListItemVM> rows)
    {
        var studentUsers = users.Where(u => u.UserType == 's').ToList();
        if (studentUsers.Count > 0)
        {
            var students = await _studentManager.GetAccountLookupAsync(
                studentUsers.Select(u => u.ReferenceId).Where(n => n > 0).Distinct().ToList(),
                studentUsers.SelectMany(u => new[] { u.UserName, u.ReferenceId.ToString() }).Distinct().ToList());
            var byUniqueId = students
                .GroupBy(s => NormalizeUniqueId(s.UniqueId))
                .ToDictionary(g => g.Key, g => g.First());
            var byId = students.ToDictionary(s => s.Id);

            foreach (var user in studentUsers)
            {
                var student = (byUniqueId.TryGetValue(NormalizeUniqueId(user.UserName), out var s1) ? s1 : null)
                    ?? (byUniqueId.TryGetValue(NormalizeUniqueId(user.ReferenceId.ToString()), out var s2) ? s2 : null)
                    ?? (byId.TryGetValue(user.ReferenceId, out var s3) ? s3 : null);
                if (student == null) continue;
                var row = rows.First(r => r.Id == user.Id);
                row.Name = student.Name;
                row.ClassRoll = student.ClassRoll;
            }
        }

        var employeeUsers = users.Where(u => u.UserType == 'e' && u.ReferenceId > 0).ToList();
        if (employeeUsers.Count > 0)
        {
            var employees = (await _employeeManager.GetAccountLookupAsync(
                    employeeUsers.Select(u => u.ReferenceId).Distinct().ToList()))
                .ToDictionary(e => e.Id);
            foreach (var user in employeeUsers)
            {
                if (!employees.TryGetValue(user.ReferenceId, out var employee)) continue;
                var row = rows.First(r => r.Id == user.Id);
                row.Name = employee.EmployeeName;
                row.EmployeeId = employee.Id;
            }
        }
    }

    private static string NormalizeUniqueId(string value) => (value ?? string.Empty).Trim().TrimStart('0');

    [HttpGet]
    public async Task<IActionResult> Reset(string id, string search, string type, int page = 1, int pageSize = 25)
    {
        var user = string.IsNullOrEmpty(id) ? null : await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        if (await _userManager.IsInRoleAsync(user, SuperAdminRole))
        {
            TempData["error"] = SuperAdminRefusal;
            return RedirectToAction(nameof(Index), new { search, type, page, pageSize });
        }
        var vm = ToResetVM(user, search);
        vm.Type = type;
        vm.Page = page;
        vm.PageSize = pageSize;
        return View(vm);
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
            return RedirectToAction(nameof(Index), BackToList(model));
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
        return RedirectToAction(nameof(Index), BackToList(model));
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
        vm.Type = posted.Type;
        vm.Page = posted.Page;
        vm.PageSize = posted.PageSize;
        // Passwords are not echoed back into the form.
        return vm;
    }

    private static object BackToList(UserPasswordResetVM m) =>
        new { search = m.Search, type = m.Type, page = m.Page, pageSize = m.PageSize };


    private static string UserTypeName(char type) => type switch
    {
        's' => "Student",
        'e' => "Employee",
        _ => "Other"
    };
}
