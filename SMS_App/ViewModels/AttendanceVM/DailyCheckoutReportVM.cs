namespace SMS_App.ViewModels.AttendanceVM;

public class DailyCheckoutReportVM
{
    public string ClassOrDesignationName { get; set; }
    public string RollOrCard { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string AlternativePhone { get; set; }
    public string CheckOut { get; set; }
    public bool SMSSent { get; set; } = false;
}
