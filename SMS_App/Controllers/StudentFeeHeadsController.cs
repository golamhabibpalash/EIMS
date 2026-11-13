using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class StudentFeeHeadsController : Controller
    {
        private readonly IStudentFeeHeadManager _studentFeeHeadManager;
        private readonly IClassFeeListManager _classFeeListManager;
        private readonly IAcademicSessionManager _academicSessionManager;
        private readonly IStudentFeeAllocationManager _feeAllocationManager;

        public StudentFeeHeadsController(IStudentFeeHeadManager studentFeeHeadManager, IClassFeeListManager classFeeListManager, IAcademicSessionManager academicSessionManager, IStudentFeeAllocationManager feeAllocationManager)
        {
            _studentFeeHeadManager = studentFeeHeadManager;
            _classFeeListManager = classFeeListManager;
            _academicSessionManager = academicSessionManager;
            _feeAllocationManager = feeAllocationManager;
        }

        // GET: StudentFeeHeads
        [Authorize(Policy = "IndexStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Index()
        {
            var feeHead = await _studentFeeHeadManager.GetAllAsync();
            return View(feeHead);
        }

        // GET: StudentFeeHeads/Details/5
        [Authorize(Policy = "DetailsStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentFeeHead = await _studentFeeHeadManager.GetByIdAsync((int)id);
            if (studentFeeHead == null)
            {
                return NotFound();
            }

            return View(studentFeeHead);
        }

        // GET: StudentFeeHeads/Create
        [Authorize(Policy = "CreateStudentFeeHeadsPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Create([Bind("Id,Name,Repeatedly,YearlyFrequency,CreatedBy,CreatedAt,EditedBy,EditedAt,ContraFeeheadId,IsResidential,SL")] StudentFeeHead studentFeeHead)
        {
            string msg = "";
            var sfhExist = await _studentFeeHeadManager.GetByNameAsync(studentFeeHead.Name);
            if (sfhExist != null)
            {
                msg = studentFeeHead.Name + " Fee Head is already exist.";
                ViewBag.msg = msg;
                TempData["crateFail"] = msg;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    studentFeeHead.CreatedBy = HttpContext.Session.GetString("UserId");
                    studentFeeHead.CreatedAt = DateTime.Now;
                    studentFeeHead.EditedBy = HttpContext.Session.GetString("UserId");
                    studentFeeHead.EditedAt = DateTime.Now;
                    studentFeeHead.MACAddress = MACService.GetMAC();
                    await _studentFeeHeadManager.AddAsync(studentFeeHead);

                    TempData["Create"] = "Successfully Created";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(studentFeeHead);
        }

        // GET: StudentFeeHeads/Edit/5
        [Authorize(Policy = "EditStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["StudentFeeHeadList"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Name");
            var studentFeeHead = await _studentFeeHeadManager.GetByIdAsync((int)id);
            if (studentFeeHead == null)
            {
                return NotFound();
            }
            return View(studentFeeHead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Repeatedly,YearlyFrequency,CreatedBy,CreatedAt,EditedBy,EditedAt,ContraFeeheadId,IsResidential,SL")] StudentFeeHead studentFeeHead)
        {
            string msg = "";
            int isChanged = 0;

            if (id != studentFeeHead.Id)
            {
                return NotFound();
            }
            if (studentFeeHead.Id == studentFeeHead.ContraFeeheadId)
            {
                ViewData["StudentFeeHeadList"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Name", studentFeeHead.ContraFeeheadId);
                msg = "Same Type and Contra Type will not be same.";
                ViewBag.msg = msg;
                TempData["editFail"] = msg;
                return View(studentFeeHead);
            }
            var sfhExist = await _studentFeeHeadManager.GetByIdAsync(id);

            if (sfhExist != null)
            {
                if (studentFeeHead.Name != sfhExist.Name || studentFeeHead.Repeatedly != sfhExist.Repeatedly || studentFeeHead.YearlyFrequency != sfhExist.YearlyFrequency || studentFeeHead.ContraFeeheadId != sfhExist.ContraFeeheadId || studentFeeHead.IsResidential != sfhExist.IsResidential || studentFeeHead.SL != sfhExist.SL)
                {
                    isChanged = 1;
                }
            }


            if (isChanged == 0)
            {
                msg = studentFeeHead.Name + " is already exists";
                ViewBag.msg = msg;
                TempData["editFail"] = msg;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        studentFeeHead.EditedAt = DateTime.Now;
                        studentFeeHead.EditedBy = HttpContext.Session.GetString("UserId");
                        studentFeeHead.MACAddress = MACService.GetMAC();
                        studentFeeHead.CreatedBy = sfhExist.CreatedBy;
                        studentFeeHead.CreatedAt = sfhExist.CreatedAt;

                        bool isSaved = await _studentFeeHeadManager.UpdateAsync(studentFeeHead);
                        if (isSaved)
                        {
                            if (studentFeeHead.ContraFeeheadId > 0)
                            {
                                StudentFeeHead existingFeeHead = await _studentFeeHeadManager.GetByIdAsync((int)studentFeeHead.ContraFeeheadId);
                                existingFeeHead.ContraFeeheadId = studentFeeHead.Id;
                                await _studentFeeHeadManager.UpdateAsync(existingFeeHead);
                            }
                            else
                            {
                                var feeHeads = await _studentFeeHeadManager.GetAllAsync();
                                StudentFeeHead existingFeeHead = feeHeads.FirstOrDefault(h => h.ContraFeeheadId == (int)studentFeeHead.Id);
                                if (existingFeeHead != null)
                                {
                                    existingFeeHead.ContraFeeheadId = null;
                                    await _studentFeeHeadManager.UpdateAsync(existingFeeHead);
                                }
                            }
                            TempData["edit"] = "Edited Successfully.";
                        }
                    }
                    catch (Exception)
                    {
                        var StudentFeeHeadExists = await _studentFeeHeadManager.GetByIdAsync(id);
                        if (StudentFeeHeadExists == null)
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
            }
            ViewData["StudentFeeHeadList"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Name", studentFeeHead.ContraFeeheadId);
            return View(studentFeeHead);
        }

        // GET: StudentFeeHeads/Delete/5
        [Authorize(Policy = "DeleteStudentFeeHeadsPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentFeeHead = await _studentFeeHeadManager.GetByIdAsync((int)id);
            if (studentFeeHead == null)
            {
                return NotFound();
            }

            return View(studentFeeHead);
        }

        // POST: StudentFeeHeads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteStudentFeeHeadsPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentFeeHead = await _studentFeeHeadManager.GetByIdAsync(id);
            await _studentFeeHeadManager.RemoveAsync(studentFeeHead);
            return RedirectToAction(nameof(Index));
        }
        public async Task<JsonResult> GetById(int id, int classId, int? sessionId, string uniqueId)
        {
            if (sessionId == null)
            {
                AcademicSession academicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
                sessionId = academicSession.Id;
            }
            var feeHead = await _studentFeeHeadManager.GetByIdAsync(id);
            var classFeeList = await _classFeeListManager.GetByClassIdAndFeeHeadIdAsync(classId, id, (int)sessionId);
            var existingFeeAllocation =await _feeAllocationManager.GetStudentFeeAllocationByUniqueIdClassFeeId(uniqueId,classFeeList.Id);
            if (existingFeeAllocation!=null)
            {
                classFeeList.Amount = existingFeeAllocation.AllocatedAmount;
            }
            return Json(classFeeList);
        }
        
        public async Task<JsonResult> GetFeeHeads(string isResidential)
        {
            var allFeeHeads = await _studentFeeHeadManager.GetAllAsync();
            try
            {
                if (isResidential == "all")
                {
                    allFeeHeads = await _studentFeeHeadManager.GetAllAsync();
                }
                else if (isResidential == "residential")
                {
                    allFeeHeads = allFeeHeads.Where(a => a.IsResidential == true).ToList();
                }
                else
                {
                    allFeeHeads = allFeeHeads.Where(a => a.IsResidential == false).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(allFeeHeads.OrderBy(s => s.SL));
        }

        [HttpGet]
        [Route("StudentFeeHeads/GetFeeHeadsBySessionIdClassIdStudentIdAsync")]
        public async Task<JsonResult> GetFeeHeadsBySessionIdClassIdStudentIdAsync(int sessionId, int classId, int studentId)
        {
                var feeHeads = await _studentFeeHeadManager.GetAllByClassIdSessionIdStudentIdAsync(classId, sessionId, studentId);
                return Json(feeHeads);
        }

        [HttpGet]
        [Route("StudentFeeHeads/GetClassFeeListBySession")]
        public async Task<JsonResult> GetClassFeeListBySession(int sessionId, int classId, bool isResidential)
        {
            var classFeeList = await _classFeeListManager.GetAllBySessionClassTypeAsync(sessionId, classId, isResidential);
            return Json(classFeeList.OrderBy(s => s.StudentFeeHead.SL));
        }
    }
}
