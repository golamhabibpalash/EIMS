using System;

namespace SMS.Entities
{
    public class ClassPeriods : CommonProps
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PeriodName { get; set; }
    }
}
