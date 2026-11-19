using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Areas.API.Models;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Employee self-service endpoints for the mobile app. The signed-in account is
/// resolved from the bearer token, then its linked employee record supplies the
/// real name and photo shown on the staff dashboard.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class EmployeeController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeManager _employeeManager;

    public EmployeeController(
        UserManager<ApplicationUser> userManager,
        IEmployeeManager employeeManager)
    {
        _userManager = userManager;
        _employeeManager = employeeManager;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        var user = string.IsNullOrEmpty(userId)
            ? null
            : await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (user.UserType != 'e')
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "This area is for employee accounts." });
        }

        var employee = await _employeeManager.GetByIdAsync(user.ReferenceId);
        if (employee == null)
        {
            return NotFound(new { message = "Employee record not found." });
        }

        return Ok(new EmployeeDashboardDto
        {
            Name = employee.EmployeeName,
            Designation = employee.Designation?.DesignationName,
            PhoneNumber = employee.Phone,
            PhotoUrl = PhotoUrl(employee.Image),
        });
    }

    private static string PhotoUrl(string photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : "/Images/Employee/photo/" + photo;
}
