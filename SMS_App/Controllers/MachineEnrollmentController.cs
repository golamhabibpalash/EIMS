using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS_App.ViewModels.MachineVM;
using SMS.BLL.Contracts;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class MachineEnrollmentController : Controller
    {
        private readonly IAttendanceMachineService _attendanceMachineService;
        private readonly IEmployeeManager _employeeManager;
        private readonly IStudentManager _studentManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IDesignationManager _designationManager;

        public MachineEnrollmentController(IAttendanceMachineService attendanceMachineService, IEmployeeManager employeeManager, IStudentManager studentManager, IAcademicClassManager academicClassManager, IDesignationManager designationManager)
        {
            _attendanceMachineService = attendanceMachineService;
            _employeeManager = employeeManager;
            _studentManager = studentManager;
            _academicClassManager = academicClassManager;
            _designationManager = designationManager;
        }

        // GET: MachineEnrollment
        [Authorize(Policy = "ManageMachineUsersPolicy")]
        public async Task<IActionResult> Index(string search, string userType)
        {
            var vm = new MachineEnrollmentIndexVM();

            var employees = await _employeeManager.GetAllAsync();
            var designations = (await _designationManager.GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);

            vm.Employees = employees
                .Where(e => string.IsNullOrEmpty(search)
                            || (e.EmployeeName != null && e.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .Select(e => new EmployeeMachineEnrollmentVM
                {
                    Id = e.Id,
                    EmployeeName = e.EmployeeName,
                    Designation = designations.TryGetValue(e.DesignationId, out var desigName) ? desigName : "",
                    Phone = e.Phone,
                    MachineUserId = e.MachineUserId,
                    Status = e.Status
                })
                .OrderBy(e => e.EmployeeName)
                .ToList();

            if (userType != "employees")
            {
                var students = await _studentManager.GetAllAsync();
                var classes = (await _academicClassManager.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);

                vm.Students = students
                    .Where(s => s.Status && (string.IsNullOrEmpty(search)
                                             || (s.Name != null && s.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                                             || s.ClassRoll.ToString().Contains(search)))
                    .Select(s => new StudentMachineEnrollmentVM
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ClassName = classes.TryGetValue(s.AcademicClassId, out var className) ? className : "",
                        ClassRoll = s.ClassRoll,
                        UniqueId = s.UniqueId,
                        MachineUserId = s.MachineUserId,
                        Status = s.Status
                    })
                    .OrderBy(s => s.ClassName).ThenBy(s => s.ClassRoll)
                    .ToList();
            }

            ViewBag.Search = search;
            ViewBag.UserType = userType ?? "all";
            return View(vm);
        }

        // POST: MachineEnrollment/UpdateEmployeeMachineId
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManageMachineUsersPolicy")]
        public async Task<IActionResult> UpdateEmployeeMachineId(int id, string machineUserId)
        {
            var employee = await _employeeManager.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.MachineUserId = string.IsNullOrWhiteSpace(machineUserId) ? null : machineUserId.Trim();
            employee.EditedAt = DateTime.Now;
            await _employeeManager.UpdateAsync(employee);
            TempData["success"] = $"Machine User ID updated for '{employee.EmployeeName}'.";
            return RedirectToAction(nameof(Index));
        }

        // POST: MachineEnrollment/UpdateStudentMachineId
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManageMachineUsersPolicy")]
        public async Task<IActionResult> UpdateStudentMachineId(int id, string machineUserId)
        {
            var student = await _studentManager.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.MachineUserId = string.IsNullOrWhiteSpace(machineUserId) ? null : machineUserId.Trim();
            student.EditedAt = DateTime.Now;
            await _studentManager.UpdateAsync(student);
            TempData["success"] = $"Machine User ID updated for '{student.Name}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}