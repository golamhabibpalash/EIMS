using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SMS.Entities.AdditionalModels;

public class LiveResultVM
{
    public int TotalColumn { get; set; }
    public string ExamTitle { get; set; }
    public int ExamGroupId { get; set; }
    public int AcademicClassId { get; set; }
    public int? AcademicSectionId { get; set; }
    public int AcademicSessionId { get; set; }
    public List<SelectListItem> AcademicSessionList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicExamGroupList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicClassList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicSectionList { get; set; } = new List<SelectListItem>();
    public List<TableHeaderSubjects> Subjects { get; set; } = new List<TableHeaderSubjects>();
    public List<LiveResultDetailsVM> ResultDetails { get; set; } = new List<LiveResultDetailsVM>();
}

public class LiveResultDetailsVM
{
    public string ClassRoll { get; set; }
    public string StudentName { get; set; }
    public double FinalGPA { get; set; }
    public string FinalGrade { get; set; }
    public double TotalMarks { get; set; }
    public double Attendance { get; set; }
    public int Rank { get; set; }
    public int PreviousRank { get; set; }
    public string Status { get; set; }
    public int Fails { get; set; }
    public List<LiveResultSubjectWise> LiveResultSubjectWises { get; set; } = new List<LiveResultSubjectWise>();

}

public class LiveResultSubjectWise
{
    public int TotalColumn { get; set; } = 0;
    public string SubjectName { get; set; }
    public double GPA { get; set; }
    public double Marks { get; set; }
    public double ObtainMarks { get; set; }
    public List<LiveResultSubjectType> SubjectTypes { get; set; }= new List<LiveResultSubjectType>();
}
public class LiveResultSubjectType
{
    public string SubjectTypeName { get; set; }
    public double TotalMarks { get; set; }
    public double GetMarks { get; set; }
    public double PassMark { get; set; } = 0.00;
}

public class TableHeaderSubjects
{
    public string SubjectName { get; set; }
    public double HighestMark { get; set; }
    public double TotalMarks { get; set; }
    public List<TableHeaderExamTypes> ExamTypes { get; set; } = new List<TableHeaderExamTypes>();
}
public class TableHeaderExamTypes
{
    public string ExamType { get; set; }
    public double TotalMarks { get; set; }
}
