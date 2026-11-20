using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Areas.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Staff-facing academic structure for the mobile app's Academics module:
/// classes (for a chosen session) with their sections and subjects, plus the
/// grading scale. Read-only, and — unlike <see cref="StudentController"/>, which
/// is scoped to one student — this returns reference data the same for every
/// staff member. Student accounts are refused (they have their own self-service
/// endpoints). Mirrors the web app's Academic setup screens, reusing the same
/// managers.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AcademicsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAcademicClassManager _classManager;
    private readonly IAcademicSectionManager _sectionManager;
    private readonly IAcademicClassSubjectManager _classSubjectManager;
    private readonly IAcademicSessionManager _sessionManager;
    private readonly IGradingTableManager _gradingManager;
    private readonly ILogger<AcademicsController> _logger;

    public AcademicsController(
        UserManager<ApplicationUser> userManager,
        IAcademicClassManager classManager,
        IAcademicSectionManager sectionManager,
        IAcademicClassSubjectManager classSubjectManager,
        IAcademicSessionManager sessionManager,
        IGradingTableManager gradingManager,
        ILogger<AcademicsController> logger)
    {
        _userManager = userManager;
        _classManager = classManager;
        _sectionManager = sectionManager;
        _classSubjectManager = classSubjectManager;
        _sessionManager = sessionManager;
        _gradingManager = gradingManager;
        _logger = logger;
    }

    /// <summary>
    /// Active classes for a session (the current one if <paramref name="sessionId"/>
    /// is omitted), each with its section and subject counts, plus the full
    /// session list for the client's session picker.
    /// </summary>
    [HttpGet("classes")]
    public async Task<IActionResult> Classes(int? sessionId)
    {
        var guard = await StaffGuardAsync();
        if (guard != null) return guard;

        var sessions = (await _sessionManager.GetAllAsync()) ?? new List<AcademicSession>();
        var current = await _sessionManager.GetCurrentAcademicSessionAsync();

        // Resolve the effective session: requested → current → newest known.
        var effectiveId = sessionId
            ?? current?.Id
            ?? sessions.OrderByDescending(s => s.Name).FirstOrDefault()?.Id
            ?? 0;
        var effective = sessions.FirstOrDefault(s => s.Id == effectiveId) ?? current;

        var classes = (await _classManager.GetAllAsync()) ?? new List<AcademicClass>();
        var summaries = new List<AcademicClassSummaryDto>();
        foreach (var c in classes.Where(c => c.Status).OrderBy(c => c.ClassSerial))
        {
            summaries.Add(new AcademicClassSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Serial = c.ClassSerial,
                SectionCount = await CountSectionsAsync(c.Id, effectiveId),
                SubjectCount = await CountSubjectsAsync(c.Id),
            });
        }

        return Ok(new AcademicsOverviewDto
        {
            SessionId = effectiveId,
            SessionName = effective?.Name,
            Sessions = sessions
                .OrderByDescending(s => s.Name)
                .Select(s => new AcademicSessionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsCurrent = s.Id == current?.Id,
                })
                .ToList(),
            Classes = summaries,
        });
    }

    /// <summary>One class's sections (for the session) and subjects.</summary>
    [HttpGet("classes/{id:int}")]
    public async Task<IActionResult> ClassDetail(int id, int? sessionId)
    {
        var guard = await StaffGuardAsync();
        if (guard != null) return guard;

        var academicClass = await _classManager.GetByIdAsync(id);
        if (academicClass == null)
            return NotFound(new { message = "Class not found." });

        var current = await _sessionManager.GetCurrentAcademicSessionAsync();
        var effectiveId = sessionId ?? current?.Id ?? 0;
        var session = effectiveId == current?.Id
            ? current
            : await _sessionManager.GetByIdAsync(effectiveId);

        var sections = await _sectionManager.GetAllByClassWithSessionId(id, effectiveId)
                       ?? new List<AcademicSection>();
        var subjects = await _classSubjectManager.GetSubjectsByClassIdAsync(id)
                       ?? new List<AcademicSubject>();

        return Ok(new AcademicClassDetailDto
        {
            Id = academicClass.Id,
            Name = academicClass.Name,
            Serial = academicClass.ClassSerial,
            SessionId = effectiveId,
            SessionName = session?.Name,
            Sections = sections
                .OrderBy(s => s.Name)
                .Select(s => new AcademicSectionDto { Id = s.Id, Name = s.Name })
                .ToList(),
            Subjects = subjects
                .Select(sub => new AcademicSubjectDto
                {
                    Id = sub.Id,
                    Name = sub.SubjectName,
                    Code = sub.SubjectCode?.ToString(),
                    TotalMarks = sub.TotalMarks,
                })
                .ToList(),
        });
    }

    /// <summary>The institute's grading scale, best grade first.</summary>
    [HttpGet("grading")]
    public async Task<IActionResult> Grading()
    {
        var guard = await StaffGuardAsync();
        if (guard != null) return guard;

        var bands = (await _gradingManager.GetAllAsync()) ?? new List<GradingTable>();
        return Ok(bands
            .OrderByDescending(g => g.NumberRangeMax)
            .Select(g => new GradeBandDto
            {
                Id = g.Id,
                LetterGrade = g.LetterGrade,
                GradePoint = g.GradePoint,
                MarkFrom = g.NumberRangeMin,
                MarkTo = g.NumberRangeMax,
                Comments = g.gradeComments,
            })
            .ToList());
    }

    // --- helpers -----------------------------------------------------------

    /// <summary>
    /// Allows any signed-in staff member; refuses students (401 no user, 403 a
    /// student account). Returns null when the caller may proceed. Reference data
    /// is the same for every staff member, so there is no per-user scoping here.
    /// </summary>
    private async Task<IActionResult> StaffGuardAsync()
    {
        var userId = _userManager.GetUserId(User);
        var user = string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();
        if (user.UserType == 's')
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "This area is for staff accounts." });
        return null;
    }

    private async Task<int> CountSectionsAsync(int classId, int sessionId)
    {
        try
        {
            var sections = await _sectionManager.GetAllByClassWithSessionId(classId, sessionId);
            return sections?.Count ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "section count failed for class {ClassId} session {SessionId}", classId, sessionId);
            return 0;
        }
    }

    private async Task<int> CountSubjectsAsync(int classId)
    {
        try
        {
            var subjects = await _classSubjectManager.GetSubjectsByClassIdAsync(classId);
            return subjects?.Count ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "subject count failed for class {ClassId}", classId);
            return 0;
        }
    }
}
