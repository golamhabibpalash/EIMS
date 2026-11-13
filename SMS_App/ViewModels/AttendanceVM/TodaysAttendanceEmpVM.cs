using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AttendanceVM
{
    public class TodaysAttendanceEmpVM
    {
        public Designation Designation { get; set; }
        public List<Employee> AttendedEmployees { get; set; }
        public int TotalEmployee { get; set; }
    }
}
