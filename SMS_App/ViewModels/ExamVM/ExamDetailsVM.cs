using SMS.Entities;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace SMS_App.ViewModels.ExamVM
{
    public class ExamDetailsVM
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int AcademicClassId { get; set; }
        public string AcademicClassName { get; set; }
        public int AcademicSessionId { get; set; }
        public string AcademicSessionName { get; set; }

        public int? AcademicSectionId { get; set; }
        public string AcademicSectionName { get; set; }
        public int TeacherId { get; set; }
        public string Teacher { get; set; }
        public string ExamMonth { get; set; }
        public int ExamMonthId { get; set; }
        public int AcademicSubjectId { get; set; }
        public string AcademicSubjectName { get; set; }
        public string AcademicSubjectCode { get; set; }
        public double TotalMarks { get; set; }
        public double TotalTime { get; set; }
        public bool IsActive { get; set; }
        public List<AcademicExamDetail> AcademicExamDetails { get; set; }
    }
}
