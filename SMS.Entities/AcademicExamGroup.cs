using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities;

public class AcademicExamGroup : CommonProps
{
    [Display(Name ="Exam Group Name")]
    public string ExamGroupName { get; set; }
    [Display(Name = "Academic Session")]
    public int AcademicSessionId { get; set; }
    public AcademicSession AcademicSession { get; set; }
    [Display(Name = "Exam Type")]
    public int AcademicExamTypeId { get; set; }
    public AcademicExamType AcademicExamType { get; set; }
    [Display(Name = "Exam Month")]
    public int ExamMonthId { get; set; }
    public bool Status { get; set; }
    public List<AcademicExam> AcademicExams { get; set; }
}
