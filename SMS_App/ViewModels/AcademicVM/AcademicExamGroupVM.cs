using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS_App.ViewModels.ExamVM;

namespace SMS_App.ViewModels.AcademicVM;

public class AcademicExamGroupVM : AcademicExamGroup
{
    public List<AcademicExamGroupIndexVM> AcademicExamGroupIndexVMList { get; set; } = new List<AcademicExamGroupIndexVM> { };
    public List<SelectListItem> ExamTypeList { get; set; }
    public List<SelectListItem> AcademicSessionList { get; set; }
    public bool? IsAttendance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
