using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.Utilities;
using SMS_App.Areas.API.Models;
using SMS_App.Utilities.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Employee self-service endpoints for the mobile app. The signed-in account is
/// resolved from the bearer token, then its linked employee record supplies the
/// real name and photo shown on the staff dashboard.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class EmployeeController : ControllerBase
{
    private static readonly HashSet<string> FinanceClaimValues = new(StringComparer.Ordinal)
    {
        "PaymentStudentPaymentsPolicy", "DuePaymentStudentPaymentsPolicy",
        "PreviousDuePaymentStudentPaymentsPolicy", "StudentPaymentInfoReportsPolicy",
        "StudentPaymentReportsPolicy", "ReceiptPaymentReportsPolicy",
        "IndexStudentFeeAllocationsPolicy", "GroupStudentFeeAllocationsPolicy",
        "IndexStudentFeeHeadsPolicy", "IndexClassFeeListsPolicy"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeManager _employeeManager;
    private readonly IStudentManager _studentManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IStudentPaymentManager _studentPaymentManager;

    public EmployeeController(
        UserManager<ApplicationUser> userManager,
        IEmployeeManager employeeManager,
        IStudentManager studentManager,
        IAcademicSessionManager academicSessionManager,
        IAttendanceMachineManager attendanceMachineManager,
        IInstituteManager instituteManager,
        IStudentPaymentManager studentPaymentManager)
    {
        _userManager = userManager;
        _employeeManager = employeeManager;
        _studentManager = studentManager;
        _academicSessionManager = academicSessionManager;
        _attendanceMachineManager = attendanceMachineManager;
        _instituteManager = instituteManager;
        _studentPaymentManager = studentPaymentManager;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        var user = string.IsNullOrEmpty(userId)
            ? null
            : await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (user.UserType != 'e')
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "This area is for employee accounts." });
        }

        var employee = await _employeeManager.GetByIdAsync(user.ReferenceId);
        if (employee == null)
        {
            return NotFound(new { message = "Employee record not found." });
        }

        var dashboard = new EmployeeDashboardDto
        {
            Name = employee.EmployeeName,
            Email = string.IsNullOrWhiteSpace(employee.Email) ? user.Email : employee.Email,
            Designation = employee.Designation?.DesignationName,
            PhoneNumber = employee.Phone,
            PhotoUrl = PhotoUrl(employee.Image),
            CanSeeCollection = User.Claims.Any(c => FinanceClaimValues.Contains(c.Value)),
        };

        var presence = await BuildPresenceAsync(employee);
        dashboard.TodaysPresence = presence.Message;
        dashboard.PresenceStatus = presence.Status;
        dashboard.PresenceTime = presence.Time;
        await ApplyStatsAsync(dashboard);

        return Ok(dashboard);
    }

    private async Task<EmployeePresence> BuildPresenceAsync(Employee employee)
    {
        try
        {
            var punches = await _attendanceMachineManager.GetAllAttendanceByDateAsync(DateTime.Today);
            var firstPunch = punches
                .Where(p => AttendancePinMatcher.Matches(p.CardNo, employee.MachineUserId) ||
                            AttendancePinMatcher.Matches(p.CardNo, employee.Id.ToString()))
                .OrderBy(p => p.PunchDatetime)
                .FirstOrDefault();
            if (firstPunch == null) return EmployeePresence.Classify(null, null);

            var institute = await _instituteManager.GetFirstOrDefaultAsync();
            return EmployeePresence.Classify(firstPunch.PunchDatetime, institute?.LateTime.TimeOfDay);
        }
        catch
        {
            return EmployeePresence.NotAvailable;
        }
    }

    private async Task ApplyStatsAsync(EmployeeDashboardDto dashboard)
    {
        try
        {
            var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
            var students = await _studentManager.GetAllAsync();
            var activeStudents = students
                .Where(s => s.Status &&
                            (currentSession == null || s.AcademicSessionId == currentSession.Id))
                .ToList();
            var employees = await _employeeManager.GetAllAsync();

            dashboard.TotalStudents = activeStudents.Count;
            dashboard.TotalEmployees = employees.Count(e => e.Status);

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var absentStudents = await _attendanceMachineManager.GetTodaysAbsentStudentAsync(today);
            var absentIds = absentStudents?.Select(s => s.Id).ToHashSet() ?? new HashSet<int>();
            dashboard.TodayPresentStudents = activeStudents.Count(s => !absentIds.Contains(s.Id));

            if (dashboard.CanSeeCollection)
            {
                var collections = await _studentPaymentManager.GetPaymentSummeryByDate(today);
                dashboard.TodayCollection = collections == null
                    ? 0
                    : (decimal)collections.Sum(c => c.Payments);
            }
        }
        catch
        {
            dashboard.TodayPresentStudents = dashboard.TotalStudents;
            dashboard.TodayCollection = 0;
        }
    }

    private static string PhotoUrl(string photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : "/Images/Employee/photo/" + photo;
}
