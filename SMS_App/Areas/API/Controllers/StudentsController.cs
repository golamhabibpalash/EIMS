using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS_App.Areas.API.Models;
using SMS_App.Utilities.Students;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Staff-facing student directory for the mobile app: a paged, searchable list
/// and per-student details. Guarded by the same roles and policies as the web
/// Students grid (<c>IndexStudentsPolicy</c> → "View Student List") and details
/// page (<c>DetailsStudentsPolicy</c> → "Details Student"). Distinct from
/// <see cref="StudentController"/>, which is a student's own self-service API.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class StudentsController : ControllerBase
{
    private readonly IStudentManager _studentManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IAcademicSectionManager _academicSectionManager;

    public StudentsController(
        IStudentManager studentManager,
        IAcademicSessionManager academicSessionManager,
        IAcademicClassManager academicClassManager,
        IAcademicSectionManager academicSectionManager)
    {
        _studentManager = studentManager;
        _academicSessionManager = academicSessionManager;
        _academicClassManager = academicClassManager;
        _academicSectionManager = academicSectionManager;
    }

    /// <summary>GET /API/Students?page=1&amp;pageSize=20&amp;search=&amp;classId=&amp;sectionId= — current session.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexStudentsPolicy")]
    public async Task<IActionResult> List(
        int page = 1,
        int pageSize = StudentDirectoryQuery.DefaultPageSize,
        string search = null,
        int? classId = null,
        int? sectionId = null)
    {
        var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var students = await _studentManager.GetAllAsync();

        var (rows, total) = StudentDirectoryQuery.Apply(
            students, currentSession?.Id, classId, sectionId, search, page, pageSize);

        return Ok(new PagedResultDto<StudentListItemDto>
        {
            Page = StudentDirectoryQuery.ClampPage(page),
            PageSize = StudentDirectoryQuery.ClampPageSize(pageSize),
            TotalCount = total,
            Items = rows.Select(s => new StudentListItemDto
            {
                Id = s.Id,
                UniqueId = s.UniqueId,
                ClassRoll = s.ClassRoll,
                Name = s.Name,
                NameBangla = s.NameBangla,
                ClassName = s.AcademicClass?.Name,
                SectionName = s.AcademicSection?.Name,
                PhoneNo = StudentDirectoryQuery.Phone(s.PhoneNo),
                GuardianPhone = StudentDirectoryQuery.Phone(s.GuardianPhone),
                Status = s.Status,
                PhotoUrl = StudentProfileMapper.PhotoUrl(s.Photo),
            }).ToList(),
        });
    }

    /// <summary>GET /API/Students/filters — active classes and their current-session sections.</summary>
    [HttpGet("filters")]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexStudentsPolicy")]
    public async Task<IActionResult> Filters()
    {
        var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var classes = await _academicClassManager.GetAllAsync();
        var sections = await _academicSectionManager.GetAllAsync();

        var sessionSections = sections
            .Where(s => s.Status && (currentSession == null || s.AcademicSessionId == currentSession.Id))
            .ToList();

        return Ok(new StudentFilterOptionsDto
        {
            Classes = classes
                .Where(c => c.Status)
                .OrderBy(c => c.ClassSerial)
                .ThenBy(c => c.Name)
                .Select(c => new StudentFilterClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Sections = sessionSections
                        .Where(s => s.AcademicClassId == c.Id)
                        .OrderBy(s => s.Name)
                        .Select(s => new StudentFilterSectionDto { Id = s.Id, Name = s.Name })
                        .ToList(),
                })
                .ToList(),
        });
    }

    /// <summary>GET /API/Students/5 — the full profile of one student.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "DetailsStudentsPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var student = await _studentManager.GetByIdAsync(id);
        if (student == null) return NotFound(new { message = "Student not found." });
        return Ok(StudentProfileMapper.ToProfileDto(student));
    }
}
