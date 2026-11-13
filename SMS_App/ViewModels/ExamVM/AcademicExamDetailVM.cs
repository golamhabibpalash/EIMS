using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ExamVM
{
    public class AcademicExamDetailVM : AcademicExam
    {
        public AcademicExamDetailVM()
        {
            StudentList = new List<SelectListItem>();
        }
        public int NewStudentId { get; set; }
        public double NewObtainMark { get; set; }
        public string NewRemarks { get; set; }
        public List<SelectListItem> StudentList { get; set; }
        public List<SelectListItem> MissingStudentList { get; set; }
    }
}
