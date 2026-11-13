using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System.Collections.Generic;

namespace SMS_App.ViewModels.Students
{
    public class DashboardIndexVM
    {
        public int TotalStudents { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSections { get; set; }

        public int TodayPresentStudents { get; set; }
        public int TodayAbsentStudents { get; set; }
        public int TodayPresentEmployees { get; set; }
        public int TodayAbsentEmployees { get; set; }

        public decimal TodayCollection { get; set; }
        public decimal MonthlyCollection { get; set; }
        public decimal TotalDueAmount { get; set; }

        public string CurrentSessionName { get; set; }
        public AcademicSession CurrentSession { get; set; }

        public ICollection<StudentPaymentSummeryVM> TodayCollections { get; set; } = new List<StudentPaymentSummeryVM>();
        public ICollection<ClassWiseStudentCount> ClassWiseStudentCounts { get; set; } = new List<ClassWiseStudentCount>();
        public ICollection<ClassWiseCollection> ClassWiseCollections { get; set; } = new List<ClassWiseCollection>();
        public ICollection<Student> RecentStudents { get; set; } = new List<Student>();
        public ICollection<Student> TodayAbsentStudentList { get; set; } = new List<Student>();
    }

    public class ClassWiseStudentCount
    {
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
    }

    public class ClassWiseCollection
    {
        public string ClassName { get; set; }
        public decimal Amount { get; set; }
    }
}
