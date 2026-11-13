using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{

    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AcademicSessionsController : Controller
    {
        private readonly IAcademicSessionManager _sessionManager;
        private readonly ILogger<AcademicSessionsController> logger;

        public AcademicSessionsController(IAcademicSessionManager sessionManager, ILogger<AcademicSessionsController> _Logger)
        {
            _sessionManager = sessionManager;
            logger = _Logger;
        }

        // GET: AcademicSessions
        [Authorize(Policy = "IndexAcademicSessionPolicy")]
        public async Task<IActionResult> Index()
        {
            //var aSession = await _context.AcademicSession.OrderBy(a => a.Name.Trim().Substring(0,4)).ToListAsync();
            var aSession = await _sessionManager.GetAllAsync();
            return View(aSession);
        }

        // GET: AcademicSessions/Details/5
        [Authorize(Policy = "DetailsAcademicSessionPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int myId = Convert.ToInt32(id);

            //var academicSession = await _context.AcademicSession
            //    .FirstOrDefaultAsync(m => m.Id == id);
            var academicSession = await _sessionManager.GetByIdAsync(myId);
            if (academicSession == null)
            {
                return NotFound();
            }

            return View(academicSession);
        }

        // GET: AcademicSessions/Create
        [Authorize(Policy = "CreateAcademicSessionPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "CreateAcademicSessionPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Status,CurrentSession,CreatedBy,CreatedAt,EditedBy,EditedAt")] AcademicSession academicSession)
        {
            string msg = "";
            if (academicSession.Name != null)
            {
                if (ModelState.IsValid)
                {
                    if (await _sessionManager.IsExistByNameAsync(academicSession.Name))
                    {
                        msg = "This name is already exists.";
                        TempData["error"] = msg;
                    }
                    else
                    {
                        academicSession.CreatedAt = DateTime.Now;
                        academicSession.CreatedBy = HttpContext.Session.GetString("UserId");
                        academicSession.EditedBy = HttpContext.Session.GetString("UserId");
                        academicSession.EditedAt = DateTime.Now;
                        bool isSaved = await _sessionManager.AddAsync(academicSession);
                        if (isSaved)
                        {
                            TempData["create"] = "Created Successfully";
                        }
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            else
            {
                msg = "Please Input a name first.";
            }
            ViewBag.msg = msg;
            return View(academicSession);
        }

        // GET: AcademicSessions/Edit/5
        [Authorize(Policy = "EditAcademicSessionPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var academicSession = await _context.AcademicSession.FindAsync(id);
            var academicSession = await _sessionManager.GetByIdAsync((int)id);
            if (academicSession == null)
            {
                return NotFound();
            }
            return View(academicSession);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAcademicSessionPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,CurrentSession,CreatedBy,CreatedAt,EditedBy,EditedAt")] AcademicSession academicSession)
        {
            string msg = "";
            if (id != academicSession.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    academicSession.EditedBy = HttpContext.Session.GetString("UserId");
                    academicSession.EditedAt = DateTime.Now;
                    await _sessionManager.UpdateAsync(academicSession);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _sessionManager.IsExistByIdAsync(academicSession.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["edit"] = "Updated Successfully";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.msg = msg;
            return View(academicSession);
        }

        // GET: AcademicSessions/Delete/5
        [Authorize(Policy = "DeleteAcademicSessionPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicSession = await _sessionManager.GetByIdAsync((int)id);
            if (academicSession == null)
            {
                return NotFound();
            }

            return View(academicSession);
        }

        // POST: AcademicSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteAcademicSessionPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var academicSession = await _sessionManager.GetByIdAsync(id);
                if (academicSession == null)
                {
                    TempData["error"] = "Session not found.";
                }
                else
                {
                    await _sessionManager.RemoveAsync(academicSession);
                    TempData["delete"] = "Deleted Successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
