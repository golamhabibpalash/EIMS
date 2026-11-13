using System;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AttendanceVM
{
    public class MonthlyAttendanceFullClassDetails
    {
        public int Roll { get; set; }
        public string StudentName { get; set; }
        public IDictionary<int,bool> isPresents { get; set; }
        public int Total { get; set; }
        public int CountPercentage { get; set; }
        public List<DateTime> Holidays { get; set; }
    }
}
