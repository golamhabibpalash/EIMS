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
    public class NationalitiesController : Controller
    {
        private readonly INationalityManager _nationalityManager;

        public NationalitiesController(INationalityManager nationalityManager)
        {
           _nationalityManager = nationalityManager;
        }

        // GET: Nationalities
        [Authorize(Policy = "IndexNationalitiesPolicy")]
        public async Task<IActionResult> Index()
        {
            return View(await _nationalityManager.GetAllAsync());
        }

        // GET: Nationalities/Details/5
        [Authorize(Policy = "DetailsNationalitiesPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nationality = await _nationalityManager.GetByIdAsync((int)id);
            if (nationality == null)
            {
                return NotFound();
            }

            return View(nationality);
        }

        // GET: Nationalities/Create
        [Authorize(Policy = "CreateNationalitiesPolicy")]
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateNationalitiesPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Nationality nationality)
        {
            
                if (ModelState.IsValid)
                {
                    nationality.CreatedAt = DateTime.Now;
                    nationality.CreatedBy = HttpContext.Session.GetString("UserId");


                    await _nationalityManager.AddAsync(nationality);
                    return RedirectToAction(nameof(Index));
                }
            
            return View(nationality);
        }

        // GET: Nationalities/Edit/5
        [Authorize(Policy = "EditNationalitiesPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nationality = await _nationalityManager.GetByIdAsync((int)id);
            if (nationality == null)
            {
                return NotFound();
            }
            return View(nationality);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditNationalitiesPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Nationality nationality)
        {
            if (id != nationality.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    nationality.EditedAt = DateTime.Now;
                    nationality.EditedBy = HttpContext.Session.GetString("UserId");

                    
                    await _nationalityManager.UpdateAsync(nationality);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NationalityExists(nationality.Id))
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
            return View(nationality);
        }

        // GET: Nationalities/Delete/5
        [Authorize(Policy = "DeleteNationalitiesPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nationality = await _nationalityManager.GetByIdAsync((int)id);
            if (nationality == null)
            {
                return NotFound();
            }

            return View(nationality);
        }

        // POST: Nationalities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteNationalitiesPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nationality = await _nationalityManager.GetByIdAsync((int)id);

            await _nationalityManager.RemoveAsync(nationality);
            return RedirectToAction(nameof(Index));
        }

        private bool NationalityExists(int id)
        {
            var nationality =  _nationalityManager.GetByIdAsync((int)id);
            if (nationality!=null)
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
