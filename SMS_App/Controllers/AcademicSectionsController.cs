using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AcademicSectionsController : Controller
    {
        private readonly IAcademicSectionManager _academicSectionManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly ILogger<AcademicSectionsController> _Logger;

        private readonly IAcademicSessionManager _academicSessionManager;

        public AcademicSectionsController(IAcademicSectionManager academicSectionManager, IAcademicClassManager academicClassManager, IAcademicSessionManager academicSessionManager, ILogger<AcademicSectionsController> Logger)
        {
            _academicSectionManager = academicSectionManager;
            _academicClassManager = academicClassManager;
            _academicSessionManager = academicSessionManager;
            _Logger = Logger;
        }

        // GET: AcademicSections
        [Authorize(Policy = "IndexAcademicSectionPolicy")]
        public async Task<IActionResult> Index()
        {
            var aSectiion = await _academicSectionManager.GetAllAsync();
            return View(aSectiion);
        }

        // GET: AcademicSections/Details/5
        [Authorize(Policy = "DetailsAcademicSectionPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int myId = Convert.ToInt32(id);
            var academicSection = await _academicSectionManager.GetByIdAsync(myId);
            if (academicSection == null)
            {
                return NotFound();
            }

            return View(academicSection);
        }

        // GET: AcademicSections/Create
        [Authorize(Policy = "CreateAcademicSectionPolicy")]
        public async Task<IActionResult> Create()
        {
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            ViewData["AcademicSessionId"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateAcademicSectionPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Status,AcademicClassId,AcademicSessionId")] AcademicSection academicSection)
        {
            var academicClass =await _academicClassManager.GetByIdAsync(academicSection.AcademicClassId);
            if (ModelState.IsValid)
            {
                academicSection.CreatedBy = HttpContext.Session.GetString("UserId");
                academicSection.CreatedAt = DateTime.Now;
                academicSection.MACAddress = MACService.GetMAC();
                bool save = await _academicSectionManager.AddAsync(academicSection);
                if (save == true)
                {
                    TempData["saved"] = "New Section Added Successfully";
                    return RedirectToAction(nameof(Index));
                }
                TempData["failed"] = academicSection.Name + " is already exist for "+ academicClass.Name;
            }


            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicSection.AcademicClassId);
            ViewData["AcademicSessionId"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicSection.AcademicSessionId);
            return View(academicSection);
        }

        // GET: AcademicSections/Edit/5
        [Authorize(Policy = "EditAcademicSectionPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var academicSection = await _academicSectionManager.GetByIdAsync((int)id);
            
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicSection.AcademicClassId);
            ViewData["AcademicSessionId"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicSection.AcademicSessionId);
            return View(academicSection);
        }

        // POST: AcademicSections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAcademicSectionPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,AcademicClassId,AcademicSessionId,CreatedBy,CreatedAt,EditedBy,EditedAt")] AcademicSection academicSection)
        {
            if (id != academicSection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    academicSection.EditedBy = HttpContext.Session.GetString("UserId");
                    academicSection.EditedAt = DateTime.Now;
                    academicSection.MACAddress = MACService.GetMAC();

                    bool updated = await _academicSectionManager.UpdateAsync(academicSection);
                    if (updated==true)
                    {
                        TempData["update"] = "Academic Section Updated Successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicSectionExists(academicSection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["failed"] = academicSection.Name+ " is already exist";
            }
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicSection.AcademicClassId);
            ViewData["AcademicSessionId"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicSection.AcademicSessionId);
            var aClass = await _academicClassManager.GetByIdAsync(academicSection.AcademicClassId);
            var aSession = await _academicSessionManager.GetByIdAsync(academicSection.AcademicSessionId);
            academicSection.AcademicClass = aClass;
            academicSection.AcademicSession = aSession;
            return View(academicSection);
        }

        // GET: AcademicSections/Delete/5
        [Authorize(Policy = "DeleteAcademicSectionPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int myId = Convert.ToInt32(id);
            var academicSection = await _academicSectionManager.GetByIdAsync(myId);
            if (academicSection == null)
            {
                return NotFound();
            }
            return View(academicSection);
        }

        // POST: AcademicSections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteAcademicSectionPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var academicSection = await _academicSectionManager.GetByIdAsync(id);
            bool isDeleted =await _academicSectionManager.RemoveAsync(academicSection);
            if (isDeleted == true)
            {
                TempData["deleted"] = "Deleted Successfullly";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AcademicSectionExists(int id)
        {
            var isExist = _academicSectionManager.GetByIdAsync(id);
            if (isExist !=null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [Route("api/academicsections/getbyclasswithsessionId")]
        public async Task<IReadOnlyCollection<AcademicSection>> GetAllByClassWithSessionId(int classId, int sessionId)
        {
            if (sessionId<=0)
            {
                AcademicSession currentAcademicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
                if (currentAcademicSession!=null)
                {
                    sessionId = currentAcademicSession.Id;
                }
            }
            
            var Result = await _academicSectionManager.GetAllByClassWithSessionId(classId, sessionId);
            return Result;
        }
    }
}