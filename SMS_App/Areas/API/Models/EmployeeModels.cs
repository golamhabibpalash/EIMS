namespace SMS_App.Areas.API.Models;

// Lean, employee-facing projection for the mobile dashboard header. Scoped to
// the signed-in employee by EmployeeController.

public class EmployeeDashboardDto
{
    public string Name { get; set; }
    public string Designation { get; set; }
    public string PhoneNumber { get; set; }
    public string TodaysPresence { get; set; }
    public int TotalStudents { get; set; }
    public int TodayPresentStudents { get; set; }
    public int TotalEmployees { get; set; }
    public bool CanSeeCollection { get; set; }
    public decimal TodayCollection { get; set; }

    /// <summary>Relative path; the client prefixes the instance base URL.</summary>
    public string PhotoUrl { get; set; }
}
