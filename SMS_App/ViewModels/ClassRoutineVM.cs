using System;
using System.Collections.Generic;

namespace SMS_App.ViewModels
{
    public class ClassRoutineVM
    {
        public List<DaysVM> Days { get; set; }
    }
    public class DaysVM
    {
        public string DaysName { get; set; }
        public List<PeriodsVM> Periods { get; set; }
    }
    public class PeriodsVM
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SL { get; set; }
        public List<ClassPeriodsVM> RoutineCell { get; set; }
    }
    public class ClassPeriodsVM
    {
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Room { get; set; }
    }
}
