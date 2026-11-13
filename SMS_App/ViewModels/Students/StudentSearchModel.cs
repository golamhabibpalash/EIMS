using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
namespace SMS_App.ViewModels.Students
{
    public class StudentSearchModel
    {

        public StudentSearchModel()
        {
            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
            CategoryList = new List<SelectListItem>();
            TypeList = new List<SelectListItem>();
            Students = new List<SMS.Entities.AdditionalModels.StudentListVM>();
        }
        //Filtering Options Start
        public int AcademicClassId { get; set; }
        public int AcademicSectionId { get; set; }
        public int StudentCatergoryId { get; set; }
        public int StudentTypeId { get; set; }
        public string SearchKeyword { get; set; }
        public int TotalStudentCount { get; set; } = 0;
        public int FilterStudentCount { get; set; } = 0;
        public List<SelectListItem> ClassList { get; set; }
        public List<SelectListItem> CategoryList { get; set; }
        public List<SelectListItem> TypeList { get; set; }
        public List<SelectListItem> SectionList { get; set; }
        public List<SMS.Entities.AdditionalModels.StudentListVM> Students { get; set; }
    }
}
