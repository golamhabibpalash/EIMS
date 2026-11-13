using SMS.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using SMS.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class DesignationTypesController : Controller
    {
        private readonly IDesignationTypeManager _designationTypeManager;

        public DesignationTypesController(IDesignationTypeManager designationTypeManager)
        {
            _designationTypeManager = designationTypeManager;
        }

        // GET: DesignationTypes
        [Authorize(Policy = "IndexDesignationTypesPolicy")]
        public async Task<IActionResult> Index()
        {
            return View(await _designationTypeManager.GetAllAsync());
        }

        // GET: DesignationTypes/Details/5
        [Authorize(Policy = "DetailsDesignationTypesPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var designationType = await _designationTypeManager.GetByIdAsync((int)id);
            if (designationType == null)
            {
                return NotFound();
            }

            return View(designationType);
        }

        // GET: DesignationTypes/Create
        [Authorize(Policy = "CreateDesignationTypesPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateDesignationTypesPolicy")]
        public async Task<IActionResult> Create([Bind("Id,DesignationTypeName,CreatedBy,CreatedAt,EditedBy,EditedAt")] DesignationType designationType)
        {

            if (ModelState.IsValid)
            {
                designationType.CreatedAt = DateTime.Now;
                designationType.CreatedBy = HttpContext.Session.GetString("UserId");

                await _designationTypeManager.AddAsync(designationType);
                return RedirectToAction(nameof(Index));
            }
            return View(designationType);
        }

        // GET: DesignationTypes/Edit/5
        [Authorize(Policy = "EditDesignationTypesPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var designationType = await _designationTypeManager.GetByIdAsync((int)id);
            if (designationType == null)
            {
                return NotFound();
            }
            return View(designationType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditDesignationTypesPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DesignationTypeName,CreatedBy,CreatedAt,EditedBy,EditedAt")] DesignationType designationType)
        {
            if (id != designationType.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    designationType.EditedAt = DateTime.Now;
                    designationType.EditedBy = HttpContext.Session.GetString("UserId");

                    await _designationTypeManager.UpdateAsync(designationType);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DesignationTypeExists(designationType.Id))
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
            return View(designationType);
        }

        // GET: DesignationTypes/Delete/5
        [Authorize(Policy = "DeleteDesignationTypesPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var designationType = await _designationTypeManager.GetByIdAsync((int)id);
            if (designationType == null)
            {
                return NotFound();
            }

            return View(designationType);
        }

        // POST: DesignationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteDesignationTypesPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var designationType = await _designationTypeManager.GetByIdAsync(id);
            await _designationTypeManager.RemoveAsync(designationType);
            return RedirectToAction(nameof(Index));
        }

        private bool DesignationTypeExists(int id)
        {
            var designationType = _designationTypeManager.GetByIdAsync(id);

            if (designationType !=null)
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
