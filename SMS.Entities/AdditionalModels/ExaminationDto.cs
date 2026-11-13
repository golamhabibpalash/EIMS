using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels;

public class ExaminationDetailsDto()
{
    public int Id { get; set; }
    public string SubjectName { get; set; }
    public int SubjectId { get; set; }
    public string SectionName { get; set; }
    public int? SectionId { get; set; }
    public string EmployeeName { get; set; }
    public int EmployeeId { get; set; }
    public int TotalStudents { get; set; }
    public int TotalMarks { get; set; }
    public bool IsLocked { get; set; }
    public int ExaminationDtoId { get; set; }
    public ExaminationDto ExaminationDto { get; set; }
}

public class ExaminationDto
{
    public int Id { get; set; }
    public string ClassName { get; set; }
    public int ClassId { get; set; }
    public int ExamGroupVMId { get; set; }
    public string ExamCategory { get; set; }
    public ExamGroupDto ExamGroupDto { get; set; } = default!;
    public ICollection<ExaminationDetailsDto> ExaminationDetailsDtos { get; set; } = new List<ExaminationDetailsDto>();
}
public class ExamGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int ExamSessionVMId { get; set; }
    public ExamSessionDto ExamSessionDto { get; set; } = default!;

    public ICollection<ExaminationDto> ExaminationDtos { get; set; } = new List<ExaminationDto>();
}
public class ExamSessionDto
{
    public int Id { get; set; }
    public string SessionName { get; set; }
    public ICollection<ExamGroupDto> ExamGroupDtos { get; set; }
}
