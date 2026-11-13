using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ExamVM
{
    public class MergedExamDetailVM
    {
        public bool IsMergedView { get; set; }
        public int PrimaryExamId { get; set; }
        public List<MergedExamInfo> MergedExams { get; set; } = new();
        public List<MergedExamDetailItem> MergedExamDetails { get; set; } = new();
        public int TotalMarks { get; set; }
        public string ExamGroupName { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string ExamCategory { get; set; }
        public string ClassName { get; set; }
        public int ClassId { get; set; }
        public string SessionName { get; set; }
        public string MonthName { get; set; }
        public string TeacherName { get; set; }
        public bool IsLocked { get; set; }
        public List<SelectListItem> StudentList { get; set; } = new();
        public List<SelectListItem> MissingStudentList { get; set; } = new();
    }

    public class MergedExamInfo
    {
        public int ExamId { get; set; }
        public string SectionName { get; set; }
        public int? SectionId { get; set; }
        public int TotalStudents { get; set; }
        public bool Status { get; set; }
    }

    public class MergedExamDetailItem
    {
        public int ExamDetailId { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int ClassRoll { get; set; }
        public string SectionName { get; set; }
        public int? SectionId { get; set; }
        public double ObtainMark { get; set; }
        public bool Status { get; set; }
        public string Remarks { get; set; }
        public string EditedBy { get; set; }
    }
}
