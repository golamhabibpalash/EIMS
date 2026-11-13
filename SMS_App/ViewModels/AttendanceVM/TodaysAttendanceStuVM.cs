using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AttendanceVM
{
    public class TodaysAttendanceStuVM
    {
        public AcademicClass AcademicClass { get; set; }
        public List<Student> AttendedStudents { get; set; }
        public int TotalStudent { get; set; }
    }
}
