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
    public class DistrictsController : Controller
    {
        private readonly IDistrictManager _districtManager;
        private readonly IDivisionManager _divisionManager;

        public DistrictsController(IDistrictManager districtManager, IDivisionManager divisionManager)
        {
            _districtManager = districtManager;
            _divisionManager = divisionManager;
        }

        // GET: Districts
        [Authorize(Policy = "IndexDistrictsPolicy")]
        public async Task<IActionResult> Index()
        {
            var districts =await _districtManager.GetAllAsync() ;
            return View(districts);
        }

        // GET: Districts/Details/5
        [Authorize(Policy = "DetailsDistrictsPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtManager.GetByIdAsync((int)id);
            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // GET: Districts/Create
        [Authorize(Policy = "CreateDistrictsPolicy")]
        public async Task<IActionResult> Create()
        {
            ViewData["DivisionId"] = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateDistrictsPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,DivisionId,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] District district)
        {
            if (ModelState.IsValid)
            {
                district.CreatedAt = DateTime.Now;
                district.CreatedBy = HttpContext.Session.GetString("UserId");

                await _districtManager.AddAsync(district);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DivisionId"] = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Id", district.DivisionId);
            return View(district);
        }

        // GET: Districts/Edit/5
        [Authorize(Policy = "EditDistrictsPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtManager.GetByIdAsync((int)id);
            if (district == null)
            {
                return NotFound();
            }
            ViewData["DivisionId"] = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Id", district.DivisionId);
            return View(district);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditDistrictsPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DivisionId,Status,CreatedBy,CreatedAt,EditedBy,EditedAt")] District district)
        {
            if (id != district.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    district.EditedAt = DateTime.Now;
                    district.EditedBy = HttpContext.Session.GetString("UserId");

                    await _districtManager.UpdateAsync(district);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistrictExists(district.Id))
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
            ViewData["DivisionId"] = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Id", district.DivisionId);
            return View(district);
        }

        // GET: Districts/Delete/5
        [Authorize(Policy = "DeleteDistrictsPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtManager.GetByIdAsync((int)id);
            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // POST: Districts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteDistrictsPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var district = await _districtManager.GetByIdAsync((int)id);

            await _districtManager.RemoveAsync(district);
            return RedirectToAction(nameof(Index));
        }

        private bool DistrictExists(int id)
        {
            var district = _districtManager.GetByIdAsync(id);
            if (district != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("api/Districts/byDivision")]
        public async Task<IReadOnlyCollection<District>> GetDistrictByDivisionId(int divId)
        {
            return await _districtManager.GetAllByDivId(divId);
        }
    }
}
