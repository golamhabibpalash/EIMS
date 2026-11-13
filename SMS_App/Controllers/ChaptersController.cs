using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly IChapterManager _chapterManager;
        private readonly IAcademicClassManager _academicClassManager;
        public ChaptersController(IChapterManager _chapterManager, IAcademicClassManager academicClassManager)
        {
            this._chapterManager = _chapterManager;
            _academicClassManager = academicClassManager;
        }

        [Authorize(Policy = "ViewChaptersPolicy")]
        public async Task<IActionResult> Index()
        {
            var chapters = await _chapterManager.GetAllAsync();

            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            return View(chapters);
        }

        [HttpPost]
        [Authorize(Policy = "CreateChaptersPolicy")]
        public async Task<JsonResult> Create(Chapter chData)
        {
            try
            {
                chData.CreatedAt = DateTime.Now;
                chData.CreatedBy = HttpContext.Session.GetString("UserId");
                chData.MACAddress = HttpContext.Session.GetString("macAddress");
                var isSaved =await _chapterManager.AddAsync(chData);
                if (!isSaved)
                {
                    TempData["error"] = "Already Exist";
                    return Json(new {errorMsg ="Not Saved"});
                }
                Chapter newChapter = await _chapterManager.GetByIdAsync(chData.Id);
                TempData["created"] = "Created Successful";
                return Json(newChapter);
            }
            catch (System.Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        public async Task<JsonResult> GetChapterBySubject(int subjectId)
        {
            var chapters = await _chapterManager.GetAllAsync();
            chapters = chapters.Where(c => c.AcademicSubjectId == subjectId).ToList();
            return Json(chapters);
        }
    }
}
