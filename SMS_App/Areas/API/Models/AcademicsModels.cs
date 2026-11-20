using System.Collections.Generic;

namespace SMS_App.Areas.API.Models;

// Lean, staff-facing projections of the academic structure for the mobile app's
// Academics module. Read-only; exposed by AcademicsController to non-student
// (staff) accounts. Mirrors the web app's Academic setup (Sessions, Classes,
// Sections, Subjects, Grading) without leaking entities or view models.

/// <summary>The class list for one session, plus the session switcher data.</summary>
public class AcademicsOverviewDto
{
    /// <summary>The session these classes belong to (the current one when the request omits it).</summary>
    public int SessionId { get; set; }
    public string SessionName { get; set; }

    /// <summary>All sessions, newest first, for the on-screen session picker.</summary>
    public List<AcademicSessionDto> Sessions { get; set; } = new();
    public List<AcademicClassSummaryDto> Classes { get; set; } = new();
}

public class AcademicSessionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCurrent { get; set; }
}

public class AcademicClassSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    /// <summary>Class position (1-12); the natural sort order.</summary>
    public int Serial { get; set; }
    public int SectionCount { get; set; }
    public int SubjectCount { get; set; }
}

public class AcademicClassDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Serial { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; }
    public List<AcademicSectionDto> Sections { get; set; } = new();
    public List<AcademicSubjectDto> Subjects { get; set; } = new();
}

public class AcademicSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class AcademicSubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public double TotalMarks { get; set; }
}

public class GradeBandDto
{
    public int Id { get; set; }
    public string LetterGrade { get; set; }
    public decimal GradePoint { get; set; }
    public int MarkFrom { get; set; }
    public int MarkTo { get; set; }
    public string Comments { get; set; }
}
