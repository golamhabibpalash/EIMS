using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.ExamVM;

public class AcademicExamVM
{
    public int Id { get; set; }
    [Display(Name = "Exam Name")]
    public string ExamName { get; set; }
    [Display(Name = "Exam Type")]
    public int AcademicExamTypeId { get; set; }
    [Display(Name = "Academic Session")]
    public int AcademicSessionId { get; set; }

    [Display(Name = "Academic Class")]
    public int AcademicClassId { get; set; }

    [Display(Name = "Academic Section")]
    public int? AcademicSectionId { get; set; }

    [Display(Name = "Academic Subject")]
    public int AcademicSubjectId { get; set; }
    [Display(Name = "Teacher")]
    public int EmployeeId { get; set; }

    [Display(Name = "Total Marks")]
    public double TotalMarks { get; set; }

    public bool IsActive { get; set; }
    [Display(Name = "Month")]
    public int MonthId { get; set; }

    public List<SelectListItem> AcademicSessionList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicClassList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicSectionList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicSubjectList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> TeacherList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> MonthList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> AcademicExamTypeList { get; set; } = new List<SelectListItem>();

}

