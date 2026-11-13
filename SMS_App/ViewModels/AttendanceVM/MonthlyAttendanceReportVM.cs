using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AttendanceVM
{
    public class MonthlyAttendanceReportVM
    {
        public List<SelectListItem> AcademicClassList { get; set; }
    }
}
