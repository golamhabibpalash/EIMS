using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentVM;

namespace SMS_App.ViewModels.Students
{
    public class StudentDashboardVM
    {
        public Student Student { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public ProfileAttendance Attendance { get; set; } = new ProfileAttendance();
        public ProfileResult Result { get; set; } = new ProfileResult();
        public ProfilePayment Payment { get; set; } = new ProfilePayment();
    }
}
