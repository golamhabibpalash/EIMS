using System;

namespace SMS.Entities.RptModels.Results
{
    public class StudentWiseMarkSheetVM
    {
        public string ExamGroupName { get; set; }
        public string ClassName { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public int ClassRoll { get; set; }
        public string SectionName { get; set; }
        public int? AcademicSectionId { get; set; }
        public string GenderName { get; set; }
        public string SubjectName { get; set; }
        public double TotalMark { get; set; }
        public double ObtainMark { get; set; }
        public double GPA { get; set; }
        public string Grade { get; set; }
        public double MaxNumber { get; set; }
        public double FinalGPA { get; set; }
        public string FinalGrade { get; set; }
        public double AttendancePercentage { get; set; }
        public double TotalObtainMarks { get; set; }
        public int TotalFails { get; set; }
        public int MeritPosition { get; set; }
        public string GradeComments { get; set; }
        public int ExamGroupId { get; set; }
        public int AcademicClassId { get; set; }
        public int StudentId { get; set; }
        public DateTime DOB { get; set; }
        public string ReligionName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
