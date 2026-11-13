using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels;

public class ExamType
{
    public string Name { get; set; }  // CQ, MCQ, PR, Marks, GPA
    public int FullMarks { get; set; }
}

public class SubjectResult
{
    public string SubjectName { get; set; }
    public int HighestMarks { get; set; }
    public int OutOf { get; set; }
    public List<ExamType> ExamTypes { get; set; } = new();
}

public class StudentResult
{
    public string Roll { get; set; }
    public string Name { get; set; }
    public Dictionary<string, Dictionary<string, string>> SubjectScores { get; set; }
    // Subject -> ExamType -> Value
    public string GPA { get; set; }
    public string Grade { get; set; }
    public int TotalMarks { get; set; }
    public int Attendance { get; set; }
    public int Rank { get; set; }
}

public class MarksheetViewModel
{
    public string ExamTitle { get; set; }
    public List<SubjectResult> Subjects { get; set; } = new();
    public List<StudentResult> Students { get; set; } = new();
}
