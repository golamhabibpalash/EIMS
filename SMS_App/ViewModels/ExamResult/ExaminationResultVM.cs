using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ExamResult
{
    public class ExaminationResultVM
    {
        public int ClassRoll { get; set; }
        public string StudentName { get; set; }
        public List<ExaminationResultDetailsVMs> ExaminationResultDetailsVMs { get; set; } = new List<ExaminationResultDetailsVMs>();
        public double TotalMarks { get; set; }
        public double CGPA { get; set; }
        public string Grade { get; set; }
        public int Rank { get; set; }
        public string GradeComment { get; set; }
        public int FailSubCount { get; set; }
        public int PreviousMonthRank { get; set; }
        public double AttendancePercent { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public Gender Gender { get; set; }

    }
    public class ExaminationResultDetailsVMs
    {
        public string SubjectName { get; set; }
        public bool IsReligion { get; set; }
        public double ObtainMarks { get; set; }
        public double ObtainPoint { get; set; }
        public string ObtainGrade { get; set; }
        public double ExamMarks { get; set; }
        public double HighestObtainMark { get; set; }
    }
}
