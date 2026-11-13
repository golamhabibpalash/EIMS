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
using SMS.DB;
using SMS.Entities;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class GendersController : Controller
    {
        private readonly IGenderManager _genderManager;

        public GendersController(IGenderManager genderManager)
        {
            _genderManager = genderManager;
        }

        // GET: Genders
        [Authorize(Policy = "IndexGendersPolicy")]
        public async Task<IActionResult> Index()
        {
            return View(await _genderManager.GetAllAsync());
        }

        // GET: Genders/Details/5
        [Authorize(Policy = "DetailsGendersPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gender = await _genderManager.GetByIdAsync((int)id);
            if (gender == null)
            {
                return NotFound();
            }

            return View(gender);
        }

        // GET: Genders/Create
        [Authorize(Policy = "CreateGendersPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateGendersPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Gender gender)
        {
            if (ModelState.IsValid)
            {
                gender.CreatedAt = DateTime.Now;
                gender.CreatedBy = HttpContext.Session.GetString("UserId");

                await _genderManager.AddAsync(gender);
                return RedirectToAction(nameof(Index));
            }
            
            return View(gender);
        }

        // GET: Genders/Edit/5
        [Authorize(Policy = "EditGendersPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gender = await _genderManager.GetByIdAsync((int)id);
            if (gender == null)
            {
                return NotFound();
            }
            return View(gender);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditGendersPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Gender gender)
        {
            if (id != gender.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    gender.EditedAt = DateTime.Now;
                    gender.EditedBy = HttpContext.Session.GetString("UserId");

                    await _genderManager.UpdateAsync(gender);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenderExists(gender.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gender);
        }

        // GET: Genders/Delete/5
        [Authorize(Policy = "DeleteGendersPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gender = await _genderManager.GetByIdAsync((int)id);
            if (gender == null)
            {
                return NotFound();
            }

            return View(gender);
        }

        // POST: Genders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteGendersPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gender = await _genderManager.GetByIdAsync(id);
            await _genderManager.RemoveAsync(gender);
            return RedirectToAction(nameof(Index));
        }

        private bool GenderExists(int id)
        {
            var gender = _genderManager.GetByIdAsync(id);
            if (gender!=null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
