using System.Collections.Generic;

namespace SMS_App.Areas.API.Models;

// Staff-facing student directory projections for the mobile app (StudentsController).
// Unlike StudentModels.cs these describe *other* students, so every endpoint that
// returns them enforces the same policies as the web Students list/details.

/// <summary>One row of the paged student list.</summary>
public class StudentListItemDto
{
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public int ClassRoll { get; set; }
    public string Name { get; set; }
    public string NameBangla { get; set; }
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public string PhoneNo { get; set; }
    public string GuardianPhone { get; set; }
    public bool Status { get; set; }

    /// <summary>Relative path (e.g. "/Images/Student/x.jpg"); null when there is no photo.</summary>
    public string PhotoUrl { get; set; }
}

/// <summary>The paging envelope the mobile client's PagedResponse expects.</summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>Class/section choices for the list's filter bar (current session).</summary>
public class StudentFilterOptionsDto
{
    public List<StudentFilterClassDto> Classes { get; set; } = new();
}

public class StudentFilterClassDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<StudentFilterSectionDto> Sections { get; set; } = new();
}

public class StudentFilterSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
