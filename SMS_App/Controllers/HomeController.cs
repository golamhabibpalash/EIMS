using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.Students;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentVM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;
[Authorize(Roles = "SuperAdmin, Admin, Teacher, Student")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStudentManager _studentManager;
    private readonly IEmployeeManager _employeeManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly IDesignationManager _designationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IAttendanceManager _attendanceManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IStudentPaymentManager _studentPaymentManager;
    private readonly ILogManager _logManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicExamManager _academicExamManager;

    public HomeController(ILogger<HomeController> logger, IStudentManager studentManager, IEmployeeManager employeeManager, UserManager<ApplicationUser> userManager, IInstituteManager instituteManager, IAcademicClassManager academicClassManager, IDesignationManager designationManager, IAttendanceManager attendanceManager, IAttendanceMachineManager attendanceMachineManager, IStudentPaymentManager studentPaymentManager, ILogManager logManager, IAcademicSectionManager academicSectionManager, IAcademicSessionManager academicSessionManager, IAcademicExamManager academicExamManager)
    {
        _logger = logger;
        _studentManager = studentManager;
        _employeeManager = employeeManager;
        _userManager = userManager;
        _instituteManager = instituteManager;
        _academicClassManager = academicClassManager;
        _designationManager = designationManager;
        _attendanceManager = attendanceManager;
        _attendanceMachineManager = attendanceMachineManager;
        _studentPaymentManager = studentPaymentManager;
        _logManager = logManager;
        _academicSectionManager = academicSectionManager;
        _academicSessionManager = academicSessionManager;
        _academicExamManager = academicExamManager;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            HttpContext.Session.SetString("macAddress", MACService.GetMAC());
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            
            HttpContext.Session.SetString("UserId", user.Id);

            // Students get their own dashboard (own attendance/result/fees only),
            // never the school-wide admin tiles.
            if (user.UserType == 's')
            {
                return await StudentDashboard(user);
            }

            // Money is restricted: only holders of a finance/accounts permission
            // claim see collection figures; everyone else (teachers, other
            // staff) gets the dashboard without them. Strictly claim-based, so
            // granting/revoking works through User Profile with no code change.
            // To widen/narrow the audience, edit this list only.
            string[] financeClaimValues = new[]
            {
                "PaymentStudentPaymentsPolicy", "DuePaymentStudentPaymentsPolicy",
                "PreviousDuePaymentStudentPaymentsPolicy", "StudentPaymentInfoReportsPolicy",
                "StudentPaymentReportsPolicy", "ReceiptPaymentReportsPolicy",
                "IndexStudentFeeAllocationsPolicy", "GroupStudentFeeAllocationsPolicy",
                "IndexStudentFeeHeadsPolicy", "IndexClassFeeListsPolicy"
            };
            bool canSeeFinance = financeClaimValues.Any(v => User.Claims.Any(c => c.Value == v));
            ViewBag.CanSeeFinance = canSeeFinance;

            var institute = await _instituteManager.GetFirstOrDefaultAsync();
            ViewBag.InstituteLogo = institute?.Logo;
            ViewBag.InstituteName = institute?.Name;

            var dashboard = new DashboardIndexVM();

            var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
            
            if (currentSession == null)
            {
                dashboard.CurrentSessionName = "No Active Session";
                ViewBag.Warning = "No active academic session found.";
                return View(dashboard);
            }
            
            dashboard.CurrentSession = currentSession;
            dashboard.CurrentSessionName = currentSession.Name;

            var allStudents = await _studentManager.GetAllAsync();
            var sessionStudents = allStudents.Where(s => s.AcademicSessionId == currentSession.Id).ToList();
            var activeSessionStudents = sessionStudents.Where(s => s.Status).ToList();

            var allEmployees = await _employeeManager.GetAllAsync();
            var activeEmployees = allEmployees.Where(e => e.Status).ToList();

            var allClasses = await _academicClassManager.GetAllAsync();
            var activeClasses = allClasses.Where(c => c.Status).ToList();

            var allSections = await _academicSectionManager.GetAllAsync();
            var activeSections = allSections.Where(s => s.Status).ToList();

            dashboard.TotalStudents = activeSessionStudents.Count;
            dashboard.TotalEmployees = activeEmployees.Count;
            dashboard.TotalClasses = activeClasses.Count;
            dashboard.TotalSections = activeSections.Count;
            
            // Attendance data
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            
            try
            {
                var todayAbsentStudents = await _attendanceMachineManager.GetTodaysAbsentStudentAsync(today);
                var todayAbsentEmployees = await _attendanceMachineManager.GetTodaysAbsentEmployeeAsync(today);

                // TotalStudents only counts the current session, so scope the
                // absent list to it too or Present Today can go negative.
                var sessionStudentIds = activeSessionStudents.Select(s => s.Id).ToHashSet();
                var sessionAbsentStudents = (todayAbsentStudents ?? new List<Student>())
                    .Where(s => sessionStudentIds.Contains(s.Id))
                    .ToList();

                dashboard.TodayAbsentStudents = sessionAbsentStudents.Count;
                dashboard.TodayPresentStudents = dashboard.TotalStudents - dashboard.TodayAbsentStudents;
                dashboard.TodayAbsentEmployees = todayAbsentEmployees?.Count ?? 0;
                dashboard.TodayPresentEmployees = dashboard.TotalEmployees - dashboard.TodayAbsentEmployees;
                dashboard.TodayAbsentStudentList = sessionAbsentStudents.Take(10).ToList();
            }
            catch
            {
                dashboard.TodayAbsentStudents = 0;
                dashboard.TodayPresentStudents = dashboard.TotalStudents;
                dashboard.TodayAbsentEmployees = 0;
                dashboard.TodayPresentEmployees = dashboard.TotalEmployees;
                dashboard.TodayAbsentStudentList = new List<Student>();
            }

            // Today's Collection (skipped entirely for staff without finance access)
            if (canSeeFinance)
            {
            try
            {
                var todayCollections = await _studentPaymentManager.GetPaymentSummeryByDate(today);
                dashboard.TodayCollection = todayCollections != null ? (decimal)todayCollections.Sum(c => c.Payments) : 0;
                dashboard.TodayCollections = todayCollections?.ToList() ?? new List<StudentPaymentSummeryVM>();
            }
            catch
            {
                dashboard.TodayCollection = 0;
                dashboard.TodayCollections = new List<StudentPaymentSummeryVM>();
            }

            // Monthly Collection
            try
            {
                var monthYear = DateTime.Today.ToString("yyyy-MM");
                var monthlyCollections = await _studentPaymentManager.GetPaymentSummeryByMonthYear(monthYear);
                dashboard.MonthlyCollection = monthlyCollections != null ? (decimal)monthlyCollections.Sum(c => c.Payments) : 0;
            }
            catch
            {
                dashboard.MonthlyCollection = 0;
            }
            }

            // Class-wise student counts
            dashboard.ClassWiseStudentCounts = activeSessionStudents
                .GroupBy(s => s.AcademicClass?.Name ?? "N/A")
                .Select(g => new ClassWiseStudentCount { ClassName = g.Key, StudentCount = g.Count() })
                .OrderBy(c => c.ClassName)
                .ToList();

            // Recent students
            dashboard.RecentStudents = activeSessionStudents
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToList();

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            var dashboard = new DashboardIndexVM();
            dashboard.CurrentSessionName = "Error";
            ViewBag.Error = "Error: " + ex.Message;
            return View(dashboard);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    private async Task<IActionResult> StudentDashboard(ApplicationUser user)
    {
        var dashboard = new StudentDashboardVM();
        try
        {
            var student = await _studentManager.GetStudentByUniqueIdAsync(user.ReferenceId.ToString());
            if (student == null)
            {
                return NotFound();
            }
            dashboard.Student = student;
            dashboard.SessionName = (await _academicSessionManager.GetCurrentAcademicSessionAsync())?.Name ?? string.Empty;
            try { dashboard.Attendance = await _studentManager.GetProfileAttendanceAsync(student.Id) ?? new ProfileAttendance(); } catch { }
            try { dashboard.Result = await _academicExamManager.GetSingleStudentResultDetailForProfile(student.Id) ?? new ProfileResult(); } catch { }
            try { dashboard.Payment = await _studentPaymentManager.GetProfilePaymentAsync(student.Id) ?? new ProfilePayment(); } catch { }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading student dashboard");
        }
        return View("IndexStudent", dashboard);
    }

    // HomeController is role-gated, so without this the error page itself bounces a
    // student or signed-out user to the login screen instead of showing the error.
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}