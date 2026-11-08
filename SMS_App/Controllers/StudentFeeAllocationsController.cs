using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.Students;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

public class StudentFeeAllocationsController : Controller
{
    #region Fields
    private readonly IStudentFeeAllocationManager _studentFeeAllocationManager;
    private readonly IStudentFeeHeadManager _studentFeeHeadManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IStudentManager _student;
    private readonly IClassFeeListManager _classFeeListManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly UserManager<ApplicationUser> _userManager;

    #endregion Fields

    #region Constructor
    public StudentFeeAllocationsController(IStudentFeeAllocationManager studentFeeAllocationManager, IStudentFeeHeadManager studentFeeHeadManager, IAcademicClassManager academicClassManager, IStudentManager student, IClassFeeListManager classFeeListManager, IAcademicSessionManager academicSessionManager = null, UserManager<ApplicationUser> userManager = null)
    {
        _studentFeeAllocationManager = studentFeeAllocationManager;
        _studentFeeHeadManager = studentFeeHeadManager;
        _academicClassManager = academicClassManager;
        _student = student;
        _classFeeListManager = classFeeListManager;
        _academicSessionManager = academicSessionManager;
        _userManager = userManager;
    }
    #endregion Constructor

    #region Methods
    // GET: StudentFeeAllocationsController

    [Authorize(Policy = "IndexStudentFeeAllocationsPolicy")]
    public async Task<ActionResult> Index()
    {
        StudentFeeAllocationVM studentFeeAllocationVM = new StudentFeeAllocationVM();

        var allClasses = await _academicClassManager.GetAllAsync();
        studentFeeAllocationVM.AcademicClassList = new SelectList(allClasses.Where(s => s.Status == true), "Id", "Name");
        studentFeeAllocationVM.CurrentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();

        return View(studentFeeAllocationVM);
    }

    [HttpGet]
    [Route("GetDataTableData")]
    [Authorize(Policy = "IndexStudentFeeAllocationsPolicy")]
    public async Task<IActionResult> GetDataTableData([FromQuery] int draw, [FromQuery] int start, [FromQuery] int length,
        [FromQuery] string searchValue, [FromQuery] string orderColumn, [FromQuery] string orderDirection)
    {
        if (string.IsNullOrEmpty(orderColumn))
            orderColumn = "classroll";
        if (string.IsNullOrEmpty(orderDirection))
            orderDirection = "asc";

        try
        {
            var result = await _studentFeeAllocationManager.GetDataTableDataAsync(
                searchValue, orderColumn, orderDirection, start, length);

            var response = new
            {
                draw = draw,
                recordsTotal = result.totalRecord,
                recordsFiltered = result.filteredRecord,
                data = result.data.Select(s => new
                {
                    id = s.Id,
                    studentId = s.StudentId,
                    student = s.Student != null ? new
                    {
                        id = s.Student.Id,
                        name = s.Student.Name,
                        classRoll = s.Student.ClassRoll,
                        academicClassId = s.Student.AcademicClassId
                    } : null,
                    studentFeeHeadId = s.StudentFeeHeadId,
                    studentFeeHead = s.StudentFeeHead != null ? new
                    {
                        id = s.StudentFeeHead.Id,
                        name = s.StudentFeeHead.Name
                    } : null,
                    allocatedAmount = s.AllocatedAmount,
                    isActive = s.IsActive,
                    feeAllocationApplication = s.FeeAllocationApplication,
                    editedAt = s.EditedAt,
                    editedBy = s.EditedBy,
                    classFeeList = s.ClassFeeList != null ? new
                    {
                        id = s.ClassFeeList.Id,
                        academicSession = s.ClassFeeList.AcademicSession != null ? new
                        {
                            id = s.ClassFeeList.AcademicSession.Id,
                            name = s.ClassFeeList.AcademicSession.Name
                        } : null
                    } : null
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to load fee allocations." });
        }
    }

    [Authorize(Policy = "GroupStudentFeeAllocationsPolicy")]
    public async Task<ActionResult> FeeAllocationByGroup()
    {
        StudentFeeAllocationGroupVM studentFeeAllocationGroupVM = new StudentFeeAllocationGroupVM();
        var allSessions = await _academicSessionManager.GetAllAsync();
        studentFeeAllocationGroupVM.AcademicSessionList = new SelectList(allSessions.Where(s => s.Status == true), "Id", "Name");

        var allClasses = await _academicClassManager.GetAllAsync();
        studentFeeAllocationGroupVM.AcademicClassList = new SelectList(allClasses.Where(s => s.Status == true), "Id", "Name");

        return View(studentFeeAllocationGroupVM);
    }

    [Authorize(Policy = "GroupStudentFeeAllocationsPolicy")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> FeeAllocationByGroup(StudentFeeAllocationGroupVM studentFeeAllocationGroupVM)
    {
        var notifications = string.Empty;
        int updated = 0;
        int inserted = 0;
        int inactive = 0;
        var classFeeList = await _classFeeListManager.GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(studentFeeAllocationGroupVM.AcademicClassId, studentFeeAllocationGroupVM.FeeHeadId, studentFeeAllocationGroupVM.AcademicSessionId);
        if (studentFeeAllocationGroupVM.AllocationAmount < 0)
        {
            TempData["notifications"] = "Allocated amount cannot be negative. Use 0 to make the fee free.";
            return RedirectToAction("FeeAllocationByGroup");
        }
        var classFee = classFeeList?.FirstOrDefault();
        if (classFee == null)
        {
            TempData["notifications"] = "No class fee is set up for the selected class, fee and session. Add it to the class fee list first.";
            return RedirectToAction("FeeAllocationByGroup");
        }
        if (studentFeeAllocationGroupVM.Students != null && studentFeeAllocationGroupVM.Students.Count > 0)
        {
            List<StudentFeeAllocation> studentFeeAllocations = new List<StudentFeeAllocation>();    
            foreach (var student in studentFeeAllocationGroupVM.Students)
            {
                var existingAllocation =await _studentFeeAllocationManager.GetStudentFeeAllocationByUniqueIdClassFeeId(student.Student.UniqueId, classFee.Id);
                if (student.IsChecked)
                {
                    if (existingAllocation!=null)
                    {
                        existingAllocation.AllocatedAmount = studentFeeAllocationGroupVM.AllocationAmount;
                        existingAllocation.EditedAt = DateTime.Now;
                        existingAllocation.EditedBy = HttpContext.Session.GetString("UserId");
                        existingAllocation.IsActive = true;
                        existingAllocation.MACAddress = MACService.GetMAC();
                        await _studentFeeAllocationManager.UpdateAsync(existingAllocation);
                        updated++;
                    }
                    else
                    {
                        StudentFeeAllocation studentFeeAllocation = new StudentFeeAllocation
                        {
                            StudentId = student.Student.Id,
                            UniqueId = student.Student.UniqueId,
                            StudentFeeHeadId = studentFeeAllocationGroupVM.FeeHeadId,
                            AllocatedAmount = studentFeeAllocationGroupVM.AllocationAmount,
                            ClassFeeListId = classFee.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = HttpContext.Session.GetString("UserId"),
                            MACAddress = MACService.GetMAC()
                        };
                        studentFeeAllocations.Add(studentFeeAllocation);
                        inserted++;
                    }
                }
                else
                {
                    if (existingAllocation!=null)
                    {
                        existingAllocation.AllocatedAmount = studentFeeAllocationGroupVM.AllocationAmount;
                        existingAllocation.IsActive = false;
                        await _studentFeeAllocationManager.UpdateAsync(existingAllocation);
                        inactive++;
                    }
                }
            }
            await _studentFeeAllocationManager.AddRangeAsync(studentFeeAllocations);
        }

        var students = await _academicSessionManager.GetAllAsync();
        notifications = $"Total Added {inserted}; update {updated} and disabled {inactive}";
        TempData["notifications"] = notifications;
        return RedirectToAction("FeeAllocationByGroup");
    }

    // GET: StudentFeeAllocationsController/Details/5
    [Authorize(Policy = "DetailsStudentFeeAllocationsPolicy")]
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: StudentFeeAllocationsController/Create
    [Authorize(Policy = "CreateStudentFeeAllocationsPolicy")]
    public ActionResult Create()
    {
        return View();
    }

    // POST: StudentFeeAllocationsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateStudentFeeAllocationsPolicy")]
    public async Task<ActionResult> Create(StudentFeeAllocationVM studentFeeAllocationVM)
    {
        StudentFeeAllocation studentFeeAllocation = new StudentFeeAllocation();
        studentFeeAllocation = studentFeeAllocationVM.SFAllocation;
        if (ModelState.IsValid)
        {
            if (studentFeeAllocation.AllocatedAmount < 0)
            {
                TempData["failed"] = "Allocated amount cannot be negative. Use 0 to make the fee free.";
                return RedirectToAction("Index");
            }

            var student = await _student.GetStudentByUniqueIdAsync(studentFeeAllocation.UniqueId);
            if (student == null)
            {
                TempData["failed"] = "Student not found.";
                return RedirectToAction("Index");
            }

            // Use the class fee for the session chosen on the form; only fall back to the
            // student's current session when the form didn't supply one.
            ClassFeeList classFeeList = null;
            if (studentFeeAllocation.ClassFeeListId is int postedClassFeeId && postedClassFeeId > 0)
            {
                classFeeList = await _classFeeListManager.GetByIdAsync(postedClassFeeId);
                if (classFeeList != null && classFeeList.StudentFeeHeadId != studentFeeAllocation.StudentFeeHeadId)
                {
                    classFeeList = null;
                }
            }
            classFeeList ??= await _classFeeListManager.GetByClassIdAndFeeHeadIdAsync(student.AcademicClassId, studentFeeAllocation.StudentFeeHeadId, student.AcademicSessionId);
            if (classFeeList == null)
            {
                TempData["failed"] = "No class fee is set up for this fee in the selected session. Add it to the class fee list first.";
                return RedirectToAction("Index");
            }
            studentFeeAllocation.ClassFeeListId = classFeeList.Id;

            //check condition is duplicate or not?
            var existingFeeAllocation = await _studentFeeAllocationManager.GetStudentFeeAllocationByUniqueIdClassFeeId(studentFeeAllocation.UniqueId, classFeeList.Id);

            if (existingFeeAllocation != null)
            {
                var feeHead = await _studentFeeHeadManager.GetByIdAsync(existingFeeAllocation.StudentFeeHeadId);
                TempData["failed"] = student.Name + " is already allocated for " + feeHead?.Name;
                return RedirectToAction("Index");
            }
            var appUer = HttpContext.Session.GetString("UserId");
            if (appUer == null)
            {
                TempData["deleted"] = "User Not found! please login again";
                return RedirectToAction("Index");
            }
            studentFeeAllocation.CreatedAt = DateTime.Now;
            studentFeeAllocation.CreatedBy = HttpContext.Session.GetString("UserId");
            studentFeeAllocation.EditedAt = DateTime.Now;
            studentFeeAllocation.EditedBy = HttpContext.Session.GetString("UserId");
            studentFeeAllocation.MACAddress = MACService.GetMAC();
            try
            {
                bool isSaved = await _studentFeeAllocationManager.AddAsync(studentFeeAllocation);
                if (isSaved)
                {
                    TempData["created"] = "New Fee allocation added successfully";
                }
                else
                {
                    TempData["error"] = "There have some issue, try again";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        return RedirectToAction("Index");

    }

    // GET: StudentFeeAllocationsController/Edit/5
    [Authorize(Policy = "EditStudentFeeAllocationsPolicy")]
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: StudentFeeAllocationsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditStudentFeeAllocationsPolicy")]
    public async Task<ActionResult> Edit(int id, StudentFeeAllocationVM studentFeeAllocationVM)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (studentFeeAllocationVM.SFAllocation.AllocatedAmount < 0)
                {
                    TempData["failed"] = "Allocated amount cannot be negative. Use 0 to make the fee free.";
                    return RedirectToAction("Index");
                }
                var existingAllocation = await _studentFeeAllocationManager.GetByIdAsync(studentFeeAllocationVM.SFAllocation.Id);
                if (existingAllocation == null || existingAllocation.Id != studentFeeAllocationVM.SFAllocation.Id)
                {
                    TempData["deleted"] = "Data is miss matched";
                    return RedirectToAction("Index");
                }
                if (existingAllocation.StudentId == studentFeeAllocationVM.SFAllocation.StudentId && existingAllocation.IsActive == studentFeeAllocationVM.SFAllocation.IsActive && existingAllocation.AllocatedAmount == studentFeeAllocationVM.SFAllocation.AllocatedAmount && existingAllocation.StudentFeeHeadId == studentFeeAllocationVM.SFAllocation.StudentFeeHeadId)
                {
                    TempData["error"] = "Nothing Change";
                }
                else
                {
                    existingAllocation.EditedAt = DateTime.Now;
                    existingAllocation.EditedBy = HttpContext.Session.GetString("UserId");
                    existingAllocation.MACAddress = MACService.GetMAC();
                    existingAllocation.StudentId = studentFeeAllocationVM.SFAllocation.StudentId;
                    existingAllocation.IsActive = studentFeeAllocationVM.SFAllocation.IsActive;
                    existingAllocation.AllocatedAmount = studentFeeAllocationVM.SFAllocation.AllocatedAmount;
                    existingAllocation.StudentFeeHeadId = studentFeeAllocationVM.SFAllocation.StudentFeeHeadId;
                    bool isUpdated = await _studentFeeAllocationManager.UpdateAsync(existingAllocation);
                    if (isUpdated)
                    {
                        TempData["updated"] = "Successfully updated";
                    }
                }
            }
            catch
            {
                return View();
            }
        }
        return RedirectToAction("index");
    }

    // GET: StudentFeeAllocationsController/Delete/5
    [Authorize(Policy = "DeleteStudentFeeAllocationsPolicy")]
    public ActionResult Delete(int id)
    {
        return View();
    }

    // GET: /StudentFeeAllocations/CanDelete?id=5 — lets the delete dialog explain up front
    // why an allocation can't be removed. The POST re-checks, so this is only a convenience.
    [HttpGet]
    [Authorize(Policy = "DeleteStudentFeeAllocationsPolicy")]
    public async Task<JsonResult> CanDelete(int id)
    {
        var (canDelete, message) = await _studentFeeAllocationManager.CanDeleteAsync(id);
        return Json(new { canDelete, message });
    }

    // POST: StudentFeeAllocationsController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteStudentFeeAllocationsPolicy")]
    public async Task<ActionResult> Delete(int id, IFormCollection formCollection)
    {
        if (!int.TryParse(formCollection["StudentFeeAllocationId"], out var allocationId) || allocationId <= 0)
        {
            TempData["failed"] = "No allocation was selected to delete.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var (deleted, message) = await _studentFeeAllocationManager.DeleteWithValidationAsync(allocationId);
            if (deleted)
            {
                TempData["deleted"] = message;
            }
            else
            {
                TempData["failed"] = message;
            }
        }
        catch (Exception)
        {
            TempData["failed"] = "Something went wrong while deleting the allocation. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    #endregion Methods
}

