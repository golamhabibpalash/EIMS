using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.Entities;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AttendanceMachineDevicesController : Controller
    {
        private readonly IAttendanceMachineService _attendanceMachineService;

        public AttendanceMachineDevicesController(IAttendanceMachineService attendanceMachineService)
        {
            _attendanceMachineService = attendanceMachineService;
        }

        // GET: AttendanceMachineDevices
        [Authorize(Policy = "IndexAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Index()
        {
            return View(await _attendanceMachineService.GetAllAsync());
        }

        // GET: AttendanceMachineDevices/Details/5
        [Authorize(Policy = "DetailsAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _attendanceMachineService.GetByIdAsync((int)id);
            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.HealthStatus = await _attendanceMachineService.GetMachineHealthAsync(machine.Id);
            return View(machine);
        }

        // GET: AttendanceMachineDevices/Create
        [Authorize(Policy = "CreateAttendanceMachineDevicesPolicy")]
        public IActionResult Create()
        {
            PopulateBrandList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,IPAddress,Port,SerialNumber,Model,Brand,Location,IsActive,UsePushMode,PushEndpoint,Username,Password")] AttendanceMachine machine)
        {
            if (ModelState.IsValid)
            {
                machine.CreatedAt = DateTime.Now;
                machine.EditedAt = DateTime.Now;
                machine.CreatedBy = HttpContext.Session.GetString("UserId");

                await _attendanceMachineService.AddAsync(machine);
                TempData["success"] = $"Machine '{machine.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            PopulateBrandList(machine.Brand);
            return View(machine);
        }

        // GET: AttendanceMachineDevices/Edit/5
        [Authorize(Policy = "EditAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _attendanceMachineService.GetByIdAsync((int)id);
            if (machine == null)
            {
                return NotFound();
            }

            PopulateBrandList(machine.Brand);
            return View(machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IPAddress,Port,SerialNumber,Model,Brand,Location,IsActive,UsePushMode,PushEndpoint,Username,Password,CreatedBy,CreatedAt")] AttendanceMachine machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    machine.EditedAt = DateTime.Now;
                    machine.EditedBy = HttpContext.Session.GetString("UserId");
                    await _attendanceMachineService.UpdateAsync(machine);
                    TempData["success"] = $"Machine '{machine.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _attendanceMachineService.GetByIdAsync(id) != null;
                    if (!exists)
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateBrandList(machine.Brand);
            return View(machine);
        }

        // GET: AttendanceMachineDevices/Delete/5
        [Authorize(Policy = "DeleteAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _attendanceMachineService.GetByIdAsync((int)id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // POST: AttendanceMachineDevices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _attendanceMachineService.DeleteAsync(id);
            TempData["success"] = "Machine deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AttendanceMachineDevices/TestConnection/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> TestConnection(int id)
        {
            var connected = await _attendanceMachineService.TestConnectionAsync(id);
            TempData["success"] = connected ? "Connection successful." : "Connection failed. Check IP/port and network.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AttendanceMachineDevices/SyncUsers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> SyncUsers(int id)
        {
            var result = await _attendanceMachineService.SyncUsersToMachineAsync(id);
            TempData["success"] = result.Success ? result.Message : $"Sync failed: {result.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AttendanceMachineDevices/PullAttendance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAttendanceMachineDevicesPolicy")]
        public async Task<IActionResult> PullAttendance(int id)
        {
            var result = await _attendanceMachineService.PullAttendanceFromMachineAsync(id);
            TempData["success"] = result.Success ? result.Message : $"Pull failed: {result.Message}";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateBrandList(MachineBrand selected = MachineBrand.ZKTeco)
        {
            ViewData["BrandList"] = new SelectList(
                Enum.GetValues(typeof(MachineBrand)).Cast<MachineBrand>()
                    .Select(b => new { Id = (int)b, Name = b.ToString() }),
                "Id", "Name", (int)selected);
        }
    }
}