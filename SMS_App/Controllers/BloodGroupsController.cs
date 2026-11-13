using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class BloodGroupsController : Controller
    {
        private readonly IBloodGroupManager _bloodGroupManager;
        private readonly ILogger<BloodGroupsController> _logger;

        public BloodGroupsController(IBloodGroupManager bloodGroupManager, ILogger<BloodGroupsController> logger)
        {
            _bloodGroupManager = bloodGroupManager;
            _logger = logger;
        }

        [Authorize(Policy = "IndexBloodGroupsPolicy")]
        public async Task<IActionResult> Index()
        {
            return View(await _bloodGroupManager.GetAllAsync());
        }

        [Authorize(Policy = "DetailsBloodGroupsPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodGroup = await _bloodGroupManager.GetByIdAsync((int)id);
            if (bloodGroup == null)
            {
                return NotFound();
            }

            return View(bloodGroup);
        }

        [Authorize(Policy = "CreateBloodGroupsPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateBloodGroupsPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] BloodGroup bloodGroup)
        {
            var allBloodGroups = await _bloodGroupManager.GetAllAsync();
            var existBG = allBloodGroups.FirstOrDefault(s => s.Name.Trim() == bloodGroup.Name.Trim());
            if (existBG != null)
            {
                ViewBag.msg = bloodGroup.Name + " is already exist.";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    bloodGroup.CreatedAt = DateTime.Now;
                    bloodGroup.CreatedBy = HttpContext.Session.GetString("UserId");

                    await _bloodGroupManager.AddAsync(bloodGroup);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(bloodGroup);
        }

        [Authorize(Policy = "EditBloodGroupsPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodGroup = await _bloodGroupManager.GetByIdAsync((int)id);
            if (bloodGroup == null)
            {
                return NotFound();
            }
            return View(bloodGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditBloodGroupsPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] BloodGroup bloodGroup)
        {
            if (id != bloodGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bloodGroup.EditedAt = DateTime.Now;
                    bloodGroup.EditedBy = HttpContext.Session.GetString("UserId");

                    await _bloodGroupManager.UpdateAsync(bloodGroup);
                }
                catch (Exception)
                {
                    if (!BloodGroupExists(bloodGroup.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bloodGroup);
        }

        [Authorize(Policy = "DeleteBloodGroupsPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodGroup = await _bloodGroupManager.GetByIdAsync((int)id);
            if (bloodGroup == null)
            {
                return NotFound();
            }

            return View(bloodGroup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteBloodGroupsPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloodGroup = await _bloodGroupManager.GetByIdAsync(id);
            await _bloodGroupManager.RemoveAsync(bloodGroup);
            return RedirectToAction(nameof(Index));
        }

        private bool BloodGroupExists(int id)
        {
            var bloodGroup = _bloodGroupManager.GetByIdAsync(id).Result;
            return bloodGroup != null;
        }
    }
}
