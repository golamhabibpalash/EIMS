using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ClassFeeListsController : Controller
    {
        private readonly IClassFeeListManager _classFeeListManager;
        private readonly IStudentFeeHeadManager _studentFeeHeadManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IAcademicSessionManager academicSessionManager;

        public ClassFeeListsController(IClassFeeListManager classFeeListManager, IStudentFeeHeadManager studentFeeHeadManager, IAcademicClassManager academicClassManager, IAcademicSessionManager academicSessionManager)
        {
            _classFeeListManager = classFeeListManager;
            _studentFeeHeadManager = studentFeeHeadManager;
            _academicClassManager = academicClassManager;
            this.academicSessionManager = academicSessionManager;
        }

        // GET: StudentFeeLists
        [Authorize(Policy = "IndexClassFeeListsPolicy")]
        public async Task<IActionResult> Index()
        {
            string msg = "";
            if (TempData["success"] != null)
            {
                msg = TempData["success"].ToString();
                ViewBag.msg = msg;
            }
            AcademicSession currentSession = await academicSessionManager.GetCurrentAcademicSessionAsync();
            ViewBag.currentSessionId = currentSession.Id;
            var result = await _classFeeListManager.GetAllAsync();

            return View(result);
        }

        // GET: StudentFeeLists/Details/5
        [Authorize(Policy = "DetailsClassFeeListsPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentFeeList = await _classFeeListManager.GetByIdAsync((int)id);
            if (studentFeeList == null)
            {
                return NotFound();
            }

            return View(studentFeeList);
        }

        // GET: StudentFeeLists/Create
        [Authorize(Policy = "CreateClassFeeListsPolicy")]
        public async Task<IActionResult> Create()
        {
            var feeHeads = await _studentFeeHeadManager.GetAllAsync();
            ViewData["StudentFeeHeadId"] = new SelectList(feeHeads.OrderBy(s => s.SL), "Id", "Name");
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            ViewData["AcademicSessionId"] = new SelectList(await academicSessionManager.GetAllAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateClassFeeListsPolicy")]
        public async Task<IActionResult> Create([Bind("Id,StudentFeeHeadId,AcademicSessionId,Amount,AcademicClassId,CreatedBy,CreatedAt,EditedBy,EditedAt, StartDate, EndDate,SL")] ClassFeeList classFeeList)
        {
            string msg = "";

            var feeListExist = await _classFeeListManager.GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(classFeeList.AcademicClassId, classFeeList.StudentFeeHeadId, classFeeList.AcademicSessionId);

            if (feeListExist != null && feeListExist.Count > 0)
            {
                msg = "Fee list for this class is already exists.";
                TempData["crateFail"] = msg;
                ViewBag.msg = msg;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    classFeeList.CreatedAt = DateTime.Now;
                    classFeeList.CreatedBy = HttpContext.Session.GetString("UserId");
                    classFeeList.EditedAt = DateTime.Now;
                    classFeeList.EditedBy = HttpContext.Session.GetString("UserId");
                    classFeeList.MACAddress = MACService.GetMAC();
                    bool isSaved = await _classFeeListManager.AddAsync(classFeeList);
                    if (isSaved)
                    {
                        msg = "Saved Successfully.";
                        TempData["success"] = msg;
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            var feeHeads = await _studentFeeHeadManager.GetAllAsync();
            ViewData["AcademicSessionId"] = new SelectList(await academicSessionManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicSessionId);
            ViewData["StudentFeeHeadId"] = new SelectList(feeHeads.OrderBy(s => s.SL), "Id", "Name", classFeeList.StudentFeeHeadId);
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicClassId);

            return View(classFeeList);
        }

        // GET: StudentFeeLists/Edit/5
        [Authorize(Policy = "EditClassFeeListsPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classFeeList = await _classFeeListManager.GetByIdAsync((int)id);
            if (classFeeList == null)
            {
                return NotFound();
            }
            var feeHeads = await _studentFeeHeadManager.GetAllAsync();
            ViewData["AcademicSessionId"] = new SelectList(await academicSessionManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicSessionId);
            ViewData["StudentFeeHeadId"] = new SelectList(feeHeads.OrderBy(s => s.SL), "Id", "Name", classFeeList.StudentFeeHeadId);
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicClassId);
            return View(classFeeList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditClassFeeListsPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentFeeHeadId,AcademicSessionId,Amount,AcademicClassId,CreatedBy,CreatedAt,EditedBy,EditedAt, StartDate, EndDate,SL")] ClassFeeList classFeeList)
        {
            if (id != classFeeList.Id)
            {
                return NotFound();
            }

            var feeHeads = await _studentFeeHeadManager.GetAllAsync();
            ViewData["AcademicSessionId"] = new SelectList(await academicSessionManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicSessionId);
            ViewData["StudentFeeHeadId"] = new SelectList(feeHeads.OrderBy(s => s.SL), "Id", "Name", classFeeList.StudentFeeHeadId);
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classFeeList.AcademicClassId);

            if (ModelState.IsValid)
            {
                try
                {
                    var classFeeLists = await _classFeeListManager.GetAllAsync();

                    var feeList = classFeeLists.FirstOrDefault(s => s.Id != classFeeList.Id && (s.AcademicClassId == classFeeList.AcademicClassId && s.AcademicSessionId == classFeeList.AcademicSessionId && s.StudentFeeHeadId == classFeeList.StudentFeeHeadId));

                    var existingData = await _classFeeListManager.GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(classFeeList.AcademicClassId, classFeeList.StudentFeeHeadId, classFeeList.AcademicSessionId);
                    if (existingData != null)
                    {
                        var existingSingleData = existingData.FirstOrDefault();
                        if (existingSingleData.Id != id)
                        {
                            TempData["failed"] = "This data is exist";
                            return RedirectToAction(nameof(Edit), new { id = classFeeList.Id });
                        }
                    }

                    classFeeList.EditedAt = DateTime.Now;
                    classFeeList.EditedBy = HttpContext.Session.GetString("UserId");
                    classFeeList.MACAddress = MACService.GetMAC();
                    bool isUpdated = await _classFeeListManager.UpdateAsync(classFeeList);
                    if (isUpdated == true)
                    {
                        TempData["edit"] = "Updated Successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentFeeListExists(classFeeList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["failed"] = "Fail to Update";
                return View(classFeeList);
            }

            return View(classFeeList);
        }

        // GET: StudentFeeLists/Delete/5
        [Authorize(Policy = "DeleteClassFeeListsPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentFeeList = await _classFeeListManager.GetByIdAsync((int)id);
            if (studentFeeList == null)
            {
                return NotFound();
            }

            return View(studentFeeList);
        }

        // POST: StudentFeeLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteClassFeeListsPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classFeeList = await _classFeeListManager.GetByIdAsync(id);
            bool isDeleted = await _classFeeListManager.RemoveAsync(classFeeList);

            if (isDeleted)
            {
                TempData["delete"] = "Successfully Deleted";
                return RedirectToAction(nameof(Index));
            }
            return View();

        }

        private bool StudentFeeListExists(int id)
        {
            var classFeeList = _classFeeListManager.GetByIdAsync(id);
            if (classFeeList != null)
            {
                return true;
            }
            return false;
        }

        [HttpGet]
        public async Task<JsonResult> GetClassListByUniqueId(string uniqueId)
        {
            var result = await _classFeeListManager.GetClassFeeByUniquId(uniqueId);
            return new JsonResult(result.OrderBy(s => s.SL));
        }

        [HttpGet]
        public async Task<JsonResult> GetClassListBySessionClassType(int academicSessionId, int academicClassId, bool isResidential)
        {
            var result = await _classFeeListManager.GetAllBySessionClassTypeAsync(academicSessionId, academicClassId, isResidential);
            return new JsonResult(result.OrderBy(s => s.SL));
        }
        public async Task<JsonResult> GetClassFeeListBySessionIdFeeHeadIdAndClassId(int sessionId,int feeHeadId, int classId)
        {
            var result = await _classFeeListManager.GetByClassIdAndFeeHeadIdAsync(classId, feeHeadId, sessionId);

            return new JsonResult(result);
        }

        public async Task<JsonResult> GetClassFeeWithHeadBySessionClassStudentId(int sessionId, int classId, int studentId)
        {
            var result =await _classFeeListManager.GetByClassIdSessionIdStudentIdAsync(classId, sessionId, studentId);
            return new JsonResult(result);
        }
    }
}
