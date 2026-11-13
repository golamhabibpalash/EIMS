using SMS.Entities;

namespace SMS_App.ViewModels.Students.Profile;

public class StudentProfileVM
{
    public Student Student { get; set; }
    public ProfileAttendance Attendances { get; set; } = new ProfileAttendance();
    public ProfileResult Results { get; set; } = new ProfileResult();
    public ProfilePayment Payments { get; set; } = new ProfilePayment();
    public ProfileDocument Documents { get; set; } = new ProfileDocument();
}

public class ProfileAttendance
{
    public double OverallAttendance { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int LateArrivals { get; set; }
}
public class ProfileResult {}
public class ProfilePayment {}
public class ProfileDocument {}

