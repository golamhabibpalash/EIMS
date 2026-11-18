namespace SMS_App.Areas.API.Models;

// Lean, employee-facing projection for the mobile dashboard header. Scoped to
// the signed-in employee by EmployeeController.

public class EmployeeDashboardDto
{
    public string Name { get; set; }

    /// <summary>Relative path; the client prefixes the instance base URL.</summary>
    public string PhotoUrl { get; set; }
}
