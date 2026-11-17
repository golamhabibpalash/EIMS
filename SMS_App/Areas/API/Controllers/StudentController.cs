using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentVM;
using SMS_App.Areas.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// 'Student' alone binds to the sibling area namespace SMS_App.Areas.Student;
// alias the entity so the type is unambiguous here.
using StudentEntity = SMS.Entities.Student;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Student self-service endpoints for the mobile app. Everything is scoped to
/// the signed-in student (resolved from the bearer token's user), so a student
/// can only ever read their own profile, attendance, payments, results, etc.
/// Mirrors the web app's student dashboard/profile, reusing the same managers.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class StudentController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentManager _studentManager;
    private readonly IStudentPaymentManager _studentPaymentManager;
    private readonly IAcademicExamManager _academicExamManager;
    private readonly IExamResultManager _examResultManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicClassSubjectManager _classSubjectManager;
    private readonly IUserNotificationManager _notificationManager;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        UserManager<ApplicationUser> userManager,
        IStudentManager studentManager,
        IStudentPaymentManager studentPaymentManager,
        IAcademicExamManager academicExamManager,
        IExamResultManager examResultManager,
        IAcademicSessionManager academicSessionManager,
        IAcademicClassSubjectManager classSubjectManager,
        IUserNotificationManager notificationManager,
        ILogger<StudentController> logger)
    {
        _userManager = userManager;
        _studentManager = studentManager;
        _studentPaymentManager = studentPaymentManager;
        _academicExamManager = academicExamManager;
        _examResultManager = examResultManager;
        _academicSessionManager = academicSessionManager;
        _classSubjectManager = classSubjectManager;
        _notificationManager = notificationManager;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;
        var s = ctx.Student;

        var sessionName = s.AcademicSession?.Name
            ?? (await _academicSessionManager.GetCurrentAcademicSessionAsync())?.Name;

        return Ok(new StudentProfileDto
        {
            Id = s.Id,
            UniqueId = s.UniqueId,
            Name = s.Name,
            NameBangla = s.NameBangla,
            ClassRoll = s.ClassRoll,
            ClassName = s.AcademicClass?.Name,
            SectionName = s.AcademicSection?.Name,
            SessionName = sessionName,
            FatherName = s.FatherName,
            MotherName = s.MotherName,
            FatherNameBangla = s.FatherNameBangla,
            MotherNameBangla = s.MotherNameBangla,
            GuardianPhone = s.GuardianPhone,
            PhoneNo = s.PhoneNo,
            Email = s.Email,
            Address = s.AddressInfo,
            Gender = s.Gender?.Name,
            Nationality = s.Nationality?.Name,
            Religion = s.Religion?.Name,
            BloodGroup = s.BloodGroup?.Name,
            BirthCertificateNo = s.BirthCertificateNo,
            PresentAddress = Address(
                s.PresentAddressArea,
                s.PresentAddressPO,
                s.PresentUpazila?.Name,
                s.PresentDistrict?.Name,
                s.PresentDivision?.Name),
            PermanentAddress = Address(
                s.PermanentAddressArea,
                s.PermanentAddressPO,
                s.PermanentUpazila?.Name,
                s.PermanentDistrict?.Name,
                s.PermanentDivision?.Name),
            PreviousSchool = s.PreviousSchool,
            DateOfBirth = s.DOB,
            AdmissionDate = s.AdmissionDate,
            IsResidential = s.IsResidential,
            Status = s.Status,
            PhotoUrl = PhotoUrl(s.Photo),
        });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;
        var s = ctx.Student;

        var dto = new StudentDashboardDto
        {
            Name = s.Name,
            ClassName = s.AcademicClass?.Name,
            SectionName = s.AcademicSection?.Name,
            ClassRoll = s.ClassRoll,
            PhotoUrl = PhotoUrl(s.Photo),
            SessionName = s.AcademicSession?.Name,
        };

        // Match the web dashboard: each aggregate is best-effort so one failing
        // section never blanks the whole screen.
        try { dto.SessionName ??= (await _academicSessionManager.GetCurrentAcademicSessionAsync())?.Name; } catch { }
        try
        {
            var a = await _studentManager.GetProfileAttendanceAsync(s.Id);
            if (a != null)
            {
                dto.AttendancePercent = a.OverallAttendance;
                dto.DaysPresent = a.DaysPresent;
                dto.DaysAbsent = a.DaysAbsent;
                dto.TodaysAttendance = a.TodaysAttendance;
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "dashboard attendance failed for {StudentId}", s.Id); }
        try
        {
            var r = await _academicExamManager.GetSingleStudentResultDetailForProfile(s.Id);
            if (r != null)
            {
                dto.CurrentCgpa = r.CurrentCGPA;
                dto.CurrentClassRank = r.CurrentClassRank;
                dto.CurrentExamName = r.CurrentExamName;
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "dashboard result failed for {StudentId}", s.Id); }
        try { dto.TotalDue = (await _studentPaymentManager.GetProfilePaymentAsync(s.Id))?.TotalDue ?? 0; }
        catch (Exception ex) { _logger.LogWarning(ex, "dashboard payment failed for {StudentId}", s.Id); }
        try { dto.UnreadNotifications = await _notificationManager.GetUnreadCountAsync(ctx.User.Id); } catch { }

        return Ok(dto);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var a = await _studentManager.GetProfileAttendanceAsync(ctx.Student.Id) ?? new ProfileAttendance();
        return Ok(new AttendanceSummaryDto
        {
            OverallPercent = a.OverallAttendance,
            DaysPresent = a.DaysPresent,
            DaysAbsent = a.DaysAbsent,
            LateArrivals = a.LateArrivals,
            TodaysAttendance = a.TodaysAttendance,
            Months = a.MonthlyAttendances.Select(m => new MonthlyAttendanceDto
            {
                MonthName = m.MonthName,
                TotalDays = m.TotalDays,
                DaysPresent = m.DaysPresent,
                DaysAbsent = m.DaysAbsent,
                LateArrivals = m.LateArrivals,
                Percentage = m.Percentage,
                Status = m.Status,
            }).ToList(),
        });
    }

    [HttpGet("payments")]
    public async Task<IActionResult> Payments()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var p = await _studentPaymentManager.GetProfilePaymentAsync(ctx.Student.Id) ?? new ProfilePayment();
        return Ok(new PaymentSummaryDto
        {
            CurrentSession = p.CurrentSession,
            TotalFees = p.TotalFees,
            TotalPaid = p.TotalPaid,
            TotalDue = p.TotalDue,
            LastPaymentDate = p.LastPaymentDate == default ? null : p.LastPaymentDate,
            History = p.PaymentDetails.Select(d => new PaymentItemDto
            {
                PaymentDate = d.PaymentDate,
                Description = d.Description,
                Amount = d.Amount,
                Method = d.Method,
                TransactionId = d.TransactionId,
            }).ToList(),
            Upcoming = p.UpcomingPayments.Select(u => new UpcomingPaymentDto
            {
                DueDate = u.DueDate,
                Amount = u.Amount,
                Description = u.Description,
            }).ToList(),
        });
    }

    [HttpGet("results")]
    public async Task<IActionResult> Results()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var results = await _examResultManager.GetByStudentIdAsync(ctx.Student.Id);
        return Ok(results.Select(r => new ResultSummaryDto
        {
            CurrentCgpa = r.CGPA,
            CurrentClassRank = r.Rank,
            CurrentExamName = r.AcademicExamGroup?.ExamGroupName,
            CurrentGrade = r.FinalGrade,
            CurrentObtainMarks = r.TotalObtainMarks,
            CurrentTotalFail = r.TotalFails,
            Subjects = (r.ExamResultDetails ?? new List<ExamResultDetail>()).Select(d => new ResultSubjectDto
            {
                SubjectCode = d.AcademicSubject?.SubjectCode?.ToString(),
                SubjectName = d.AcademicSubject?.SubjectName,
                TotalMark = d.TotalMark,
                Grade = d.Grade,
                Gpa = d.GPA,
                TotalObtainMark = d.ObtainMark,
            }).ToList(),
        }).ToList());
    }

    [HttpGet("academics")]
    public async Task<IActionResult> Academics()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;
        var s = ctx.Student;

        var subjects = await _classSubjectManager.GetSubjectsByClassIdAsync(s.AcademicClassId) ?? new List<AcademicSubject>();
        return Ok(new AcademicsDto
        {
            ClassName = s.AcademicClass?.Name,
            SectionName = s.AcademicSection?.Name,
            SessionName = s.AcademicSession?.Name ?? (await _academicSessionManager.GetCurrentAcademicSessionAsync())?.Name,
            ClassRoll = s.ClassRoll,
            Subjects = subjects.Select(sub => new SubjectDto
            {
                Name = sub.SubjectName,
                Code = sub.SubjectCode?.ToString(),
                TotalMarks = sub.TotalMarks,
            }).ToList(),
        });
    }

    [HttpGet("documents")]
    public async Task<IActionResult> Documents()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var documents = await _studentManager.GetStudentProfileDocuments(ctx.Student.Id);
        var items = documents?.Documents ?? new List<DocInfo>();
        return Ok(items.Select(d => new StudentDocumentDto
        {
            Name = d.DocumentName,
            Url = DocumentUrl(d.DocUrl),
            Type = d.DocType,
        }).ToList());
    }

    [HttpGet("notices")]
    public async Task<IActionResult> Notices()
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var items = await _notificationManager.GetByRecipientAsync(ctx.User.Id, unreadOnly: false, take: 50);
        return Ok(items.Select(n => new NoticeDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Category = n.Category.ToString(),
            Url = n.Url,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
        }).ToList());
    }

    [HttpPost("notices/{id:int}/read")]
    public async Task<IActionResult> MarkNoticeRead(int id)
    {
        var ctx = await ResolveAsync();
        if (ctx.Error != null) return ctx.Error;

        var ok = await _notificationManager.MarkReadAsync(id, ctx.User.Id);
        return ok ? Ok(new { message = "Marked as read." }) : NotFound(new { message = "Notice not found." });
    }

    // --- helpers -----------------------------------------------------------

    private sealed class StudentContext
    {
        public ApplicationUser User { get; init; }
        public StudentEntity Student { get; init; }
        public IActionResult Error { get; init; }
    }

    /// <summary>
    /// Resolves the signed-in student, or an error result (401 no user, 403 not a
    /// student account, 404 no student record). ReferenceId is inconsistently the
    /// UniqueId or the PK across users, so try both — same as the web dashboard.
    /// </summary>
    private async Task<StudentContext> ResolveAsync()
    {
        var userId = _userManager.GetUserId(User);
        var user = string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new StudentContext { Error = Unauthorized() };

        if (user.UserType != 's')
            return new StudentContext { Error = StatusCode(StatusCodes.Status403Forbidden, new { message = "This area is for student accounts." }) };

        var student = await _studentManager.GetStudentByUniqueIdAsync(user.ReferenceId.ToString())
                      ?? await _studentManager.GetByIdAsync(user.ReferenceId);
        if (student == null)
            return new StudentContext { Error = NotFound(new { message = "Student record not found." }) };

        return new StudentContext { User = user, Student = student };
    }

    private static string PhotoUrl(string photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : "/Images/Student/" + photo;

    private static string DocumentUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var value = url.Trim();
        if (value.StartsWith('/') ||
            value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }
        return "/Images/Student/" + value;
    }

    private static string Address(params string[] parts) => string.Join(
        ", ",
        parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
}
