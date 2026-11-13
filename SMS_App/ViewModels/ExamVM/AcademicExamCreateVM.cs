using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ExamVM
{
    public class AcademicExamCreateVM
    {
        public int AcademicExamTypeId { get; set; }
        public AcademicExamType AcademicExamType { get; set; }
        public string ExamName { get; set; }
        public int AcademicSessionId { get; set; }
        public AcademicSession AcademicSession { get; set; }
        public int AcademicClassId { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public int? AcademicSectionId { get; set; }
        public AcademicSection AcademicSection { get; set; }
        public int MonthId { get; set; }
        public List<SubjectInfo> SubjectInfos { get; set; }
        public class SubjectInfo
        {
            public int SubjectId { get; set; }
            public string SubjectName { get; set; }
            public int EmployeeId { get; set; }
            public string TeacherName { get; set; }
            public double Marks { get; set; }
        }

    }
}
