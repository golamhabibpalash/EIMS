using SMS.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS.Entities;
using System.Linq;
using System.Threading.Tasks;
using SMS.BLL.Contracts;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class UpazilasController : Controller
    {
        private readonly IUpazilaManager _upazilaManager;
        private readonly IDistrictManager _districtManager;

        public UpazilasController(IUpazilaManager upazilaManager, IDistrictManager districtManager)
        {
            _upazilaManager = upazilaManager;
            _districtManager = districtManager;
        }

        // GET: Upazilas
        [Authorize(Policy = "IndexUpazilasPolicy")]
        public async Task<IActionResult> Index()
        {

            return View(await _upazilaManager.GetAllAsync());
        }

        // GET: Upazilas/Details/5
        [Authorize(Policy = "DetailsUpazilasPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Upazila = await _upazilaManager.GetByIdAsync((int)id);
            if (Upazila == null)
            {
                return NotFound();
            }

            return View(Upazila);
        }

        // GET: Upazilas/Create
        [Authorize(Policy = "CreateUpazilasPolicy")]
        public async Task<IActionResult> Create()
        {
            ViewData["DistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: Upazilas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost,ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateUpazilasPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,DistrictId,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Upazila Upazila)
        {
            if (ModelState.IsValid)
            {

                Upazila.CreatedAt = DateTime.Now;
                Upazila.CreatedBy = HttpContext.Session.GetString("UserId");

                await _upazilaManager.AddAsync(Upazila);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", Upazila.DistrictId);
            return View(Upazila);
        }

        // GET: Upazilas/Edit/5
        [Authorize(Policy = "EditUpazilasPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Upazila = await _upazilaManager.GetByIdAsync((int)id);
            if (Upazila == null)
            {
                return NotFound();
            }
            ViewData["DistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", Upazila.DistrictId);
            return View(Upazila);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditUpazilasPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DistrictId,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] Upazila Upazila)
        {
            if (id != Upazila.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Upazila.EditedAt = DateTime.Now;
                    Upazila.EditedBy = HttpContext.Session.GetString("UserId");

                    await _upazilaManager.UpdateAsync(Upazila);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UpazilaExists(Upazila.Id))
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
            ViewData["DistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", Upazila.DistrictId);
            return View(Upazila);
        }

        // GET: Upazilas/Delete/5
        [Authorize(Policy = "DeleteUpazilasPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Upazila = await _upazilaManager.GetByIdAsync((int)id);
            if (Upazila == null)
            {
                return NotFound();
            }

            return View(Upazila);
        }

        // POST: Upazilas/Delete/5
        [HttpPost, ActionName("Delete"),ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteUpazilasPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Upazila = await _upazilaManager.GetByIdAsync(id);

            await _upazilaManager.RemoveAsync(Upazila);
            return RedirectToAction(nameof(Index));
        }

        private bool UpazilaExists(int id)
        {
            var Upazila = _upazilaManager.GetByIdAsync((int)id);
            if (Upazila != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("api/upazilas/byDistrict")]
        public async Task<IReadOnlyCollection<Upazila>> GetByDistrictId(int id)
        {
            return await _upazilaManager.GetByDistrictId(id);
        }
    }
}
