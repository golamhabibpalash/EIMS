using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Entities;

public class AcademicExam : CommonProps
{
    [Display(Name = "Exam Group")]
    public int AcademicExamGroupId { get; set; }
    public AcademicExamGroup AcademicExamGroup { get; set; }
    [Display(Name = "Academic Class")]
    public int AcademicClassId { get; set; }
    public AcademicClass AcademicClass { get; set; }
    [Display(Name = "Academic Section")]
    public int? AcademicSectionId { get; set; }
    public AcademicSection AcademicSection { get; set; }
    [Display(Name = "Exam Teacher")]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    [Display(Name = "Academic Subject")]
    public int AcademicSubjectId { get; set; }
    public AcademicSubject AcademicSubject { get; set; }
    [Display(Name = "Total Marks")]
    public int TotalMarks { get; set; }
    public bool Status { get; set; }
    public string ExamCategory { get; set; } //Written,MCQ,Practical
    public virtual List<AcademicExamDetail> AcademicExamDetails { get; set; }
    
    [NotMapped]
    public List<int> AcademicSectionIdList { get; set; }
}
