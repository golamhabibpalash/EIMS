using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.ReportVM;

public class RptStudentDynamicReportVM
{
    public string ReportType { get; set; }
    [Display(Name = "Academic Session")]
    public int AcademicSessionId { get; set; }

    [Display(Name = "Academic Class"), Required]
    public int AcademicClassId { get; set; }

    [Display(Name = "Academic Section")]
    public int AcademicSectionId { get; set; }
    public Dictionary<string, string> ColumnMap { get; set; } = new();

    public List<SelectListItem> AcademicSessionList { get; set; }
    public List<SelectListItem> AcademicClassList { get; set; }
    public List<SelectListItem> AcademicSectionList { get; set; }
}
