using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class StudentPaymentDetailsController : Controller
    {
        private readonly IStudentPaymentDetailsManager _studentPaymentDetailManager;
        private readonly IStudentFeeHeadManager _studentFeeHeadManager;
        private readonly IStudentPaymentManager _studentPaymentManager;

        public StudentPaymentDetailsController(IStudentPaymentDetailsManager studentPaymentDetailsManager, IStudentFeeHeadManager studentFeeHeadManager, IStudentPaymentManager studentPaymentManager)
        {
            _studentPaymentDetailManager = studentPaymentDetailsManager;
            _studentFeeHeadManager = studentFeeHeadManager;
            _studentPaymentManager = studentPaymentManager;
        }

        // GET: StudentPaymentDetails

        [Authorize(Policy = "IndexStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Index()
        {
            var paymentDetails = await _studentPaymentDetailManager.GetAllAsync(); 
            return View(paymentDetails);
        }

        // GET: StudentPaymentDetails/Details/5
        [Authorize(Policy = "DetailsStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync((int)id);

            if (studentPaymentDetails == null)
            {
                return NotFound();
            }

            return View(studentPaymentDetails);
        }

        // GET: StudentPaymentDetails/Create
        [Authorize(Policy = "CreateStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Create()
        {
            ViewData["StudentFeeHeadId"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Id");
            ViewData["StudentPaymentId"] = new SelectList(await _studentPaymentManager.GetAllAsync(), "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Create([Bind("Id,StudentPaymentId,StudentFeeHeadId,CreatedBy,CreatedAt,EditedBy,EditedAt")] StudentPaymentDetails studentPaymentDetails)
        {
            if (ModelState.IsValid)
            {
                studentPaymentDetails.CreatedAt = DateTime.Now;
                studentPaymentDetails.CreatedBy = HttpContext.Session.GetString("UserId");
                studentPaymentDetails.MACAddress = MACService.GetMAC();
                try
                {
                    bool isSaved = await _studentPaymentDetailManager.AddAsync(studentPaymentDetails);
                    if (isSaved)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            ViewData["StudentFeeHeadId"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Id",studentPaymentDetails.StudentFeeHeadId);
            ViewData["StudentPaymentId"] = new SelectList(await _studentPaymentManager.GetAllAsync(), "Id", "Id",studentPaymentDetails.StudentPaymentId);

            return View(studentPaymentDetails);
        }

        // GET: StudentPaymentDetails/Edit/5
        [Authorize(Policy = "EditStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync((int)id);
            if (studentPaymentDetails == null)
            {
                return NotFound();
            }
            ViewData["StudentFeeHeadId"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Id", studentPaymentDetails.StudentFeeHeadId);
            ViewData["StudentPaymentId"] = new SelectList(await _studentPaymentManager.GetAllAsync(), "Id", "Id", studentPaymentDetails.StudentPaymentId);
            return View(studentPaymentDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentPaymentId,StudentFeeHeadId,CreatedBy,CreatedAt,EditedBy,EditedAt")] StudentPaymentDetails studentPaymentDetails)
        {
            if (id != studentPaymentDetails.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    studentPaymentDetails.EditedAt = DateTime.Now;
                    studentPaymentDetails.EditedBy = HttpContext.Session.GetString("UserId");

                    bool isUpdated = await _studentPaymentDetailManager.UpdateAsync(studentPaymentDetails);
                    if (isUpdated)
                    {
                        TempData["updated"] = "Payment Details Updated";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["failed"] = "Update fail";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await StudentPaymentDetailsExists(studentPaymentDetails.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["StudentFeeHeadId"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Id", studentPaymentDetails.StudentFeeHeadId);
            ViewData["StudentPaymentId"] = new SelectList(await _studentPaymentManager.GetAllAsync(), "Id", "Id", studentPaymentDetails.StudentPaymentId);

            return View(studentPaymentDetails);
        }

        // GET: StudentPaymentDetails/Delete/5
        [Authorize(Policy = "DeleteStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync((int)id);
            if (studentPaymentDetails == null)
            {
                return NotFound();
            }

            return View(studentPaymentDetails);
        }

        // POST: StudentPaymentDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteStudentPaymentDetailsPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync(id);
            if (studentPaymentDetails!=null)
            {
                bool isDeleted = await _studentPaymentDetailManager.RemoveAsync(studentPaymentDetails);
                if (isDeleted)
                {
                    TempData["deleted"] = "Delete Successfully";
                }
                else
                {
                    TempData["failed"] = "Fail to Delete";
                    return View(id);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> StudentPaymentDetailsExists(int id)
        {
            StudentPaymentDetails studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync(id);
            if (studentPaymentDetails == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<JsonResult> GetPaymentDetailById(int paymentDetailId)
        {
           StudentPaymentDetails studentPaymentDetails = await _studentPaymentDetailManager.GetByIdAsync(paymentDetailId);
           return Json(studentPaymentDetails);
        }
    }
}
