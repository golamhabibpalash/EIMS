using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using SchoolManagementSystem;
using SMS.BLL.Contracts;
using SMS.BLL.Managers;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.Finance;
using SMS_App.Utilities.Export;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.Others;
using SMS_App.Utilities.Reports;
using SMS_App.Utilities.ShortMessageService;
using SMS_App.ViewModels;
using SMS_App.ViewModels.PaymentVM;
using SessionWisePaymentVM = SMS.Entities.AdditionalModels.SessionWisePaymentVM;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class StudentPaymentsController : Controller
{
    #region Fields
    private readonly IStudentPaymentManager _studentPaymentManager;
    private readonly IStudentManager _studentManager;
    private readonly IClassFeeListManager _classFeeListManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IStudentFeeHeadManager _studentFeeHeadManager;
    private readonly IStudentPaymentDetailsManager _studentPaymentDetailsManager;
    private readonly ISetupMobileSMSManager _setupMobileSMSManager;
    private readonly IPhoneSMSManager _phoneSMSManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly IStudentFeeAllocationManager _studentFeeAllocationManager;
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudentPaymentsController> _logger;

    #endregion Fields

    #region ctor
    public StudentPaymentsController(IStudentPaymentManager studentPaymentManager, IStudentManager studentManager, IClassFeeListManager classFeeListManager, IAcademicClassManager academicClassManager, IStudentFeeHeadManager studentFeeHeadManager, IStudentPaymentDetailsManager studentPaymentDetailsManager, ISetupMobileSMSManager setupMobileSMSManager, IPhoneSMSManager phoneSMSManager, IInstituteManager instituteManager, IAcademicSessionManager academicSessionManager, IAcademicSectionManager academicSectionManager, IStudentFeeAllocationManager studentFeeAllocationManager, HttpClient httpClient, ILogger<StudentPaymentsController> logger)
    {
        _studentPaymentManager = studentPaymentManager;
        _studentManager = studentManager;
        _classFeeListManager = classFeeListManager;
        _academicClassManager = academicClassManager;
        _studentFeeHeadManager = studentFeeHeadManager;
        _studentPaymentDetailsManager = studentPaymentDetailsManager;
        _setupMobileSMSManager = setupMobileSMSManager;
        _phoneSMSManager = phoneSMSManager;
        _instituteManager = instituteManager;
        _academicSessionManager = academicSessionManager;
        _academicSectionManager = academicSectionManager;
        _studentFeeAllocationManager = studentFeeAllocationManager;
        _httpClient = httpClient;
        _logger = logger;
    }

    #endregion ctor

    #region Action Methods

    [HttpGet]
    [Authorize(Policy = "IndexStudentPaymentsPolicy")]
    public async Task<IActionResult> Index()
    {
        try
        {
            SetTempDataMessages();
            ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading StudentPayments Index");
            throw;
        }

        return View();
    }


    [HttpGet]
    [Authorize(Policy = "PaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> Payment(int? stRoll)
    {
        if (!IsValidRoll(stRoll))
        {
            return RedirectToAction("Index");
        }
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        SetTempDataMessages();

        var student = await _studentManager.GetStudentByClassRollAsync((int)stRoll);
        if (student == null)
        {
            TempData["studentNotFound"] = "Student Not Found";
            return RedirectToAction("Index");
        }
        var spvm = await CreateStudentPaymentVM(student);

        ViewBag.roll = stRoll;

        return View(spvm);

    }


    [HttpPost]
    [Authorize(Policy = "PaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> Payment(StudentPaymentVM paymentObject)
    {
        if (paymentObject.SelectedSessionId > 0)
        {
            paymentObject.CurrentAcademicSession = await _academicSessionManager.GetByIdAsync(paymentObject.SelectedSessionId);
        }
        else
        {
            paymentObject.CurrentAcademicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        }
        try
        {
            await ProcessPayment(paymentObject);
        }
        catch (Exception)
        {
            throw;
        }

        var student = await _studentManager.GetByIdAsync(paymentObject.StudentPayment.StudentId);
        return RedirectToAction("Payment", new { stRoll = student.ClassRoll });
    }

    // GET: StudentPayments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentPayment = await _studentPaymentManager.GetByIdAsync((int)id);

        if (studentPayment == null)
        {
            return NotFound();
        }

        return View(studentPayment);
    }

    // GET: StudentPayments/Create
    [Authorize(Policy = "CreateStudentPaymentsPolicy")]
    public async Task<IActionResult> Create()
    {
        ViewData["StudentId"] = new SelectList(await _studentManager.GetAllAsync(), "Id", "Name");
        return View();
    }

    // POST: StudentPayments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateStudentPaymentsPolicy")]
    public async Task<IActionResult> Create(StudentPaymentVM studentyPaymentVM, IFormFile waiverAttachment)
    {
        Student student = new();
        if (studentyPaymentVM != null)
        {
            student = await _studentManager.GetByIdAsync(studentyPaymentVM.StudentPayment.StudentId);

        }
        if (ModelState.IsValid)
        {
            try
            {
                studentyPaymentVM.StudentPayment.CreatedAt = DateTime.Now;
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                {
                    return RedirectToAction("login", "Accounts");
                }
                studentyPaymentVM.StudentPayment.CreatedBy = HttpContext.Session.GetString("UserId");

                StudentPayment sPayment = new();
                sPayment = studentyPaymentVM.StudentPayment;

                if (studentyPaymentVM.StudentPayment.StudentPaymentDetails != null)
                {
                    sPayment.StudentPaymentDetails = studentyPaymentVM.StudentPayment.StudentPaymentDetails;
                }

                sPayment.MACAddress = MACService.GetMAC();
                bool isSaved = await _studentPaymentManager.AddAsync(sPayment);
                if (isSaved)
                {
                    TempData["Saved"] = "Successfully Saved";
                    return RedirectToAction("Payment", "StudentPayments", new { stRoll = student.ClassRoll });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        ViewData["StudentId"] = new SelectList(await _studentManager.GetAllAsync(), "Id", "Name", studentyPaymentVM.StudentPayment.StudentId);
        return RedirectToAction(nameof(Index));
    }

    // GET: StudentPayments/Edit/5

    [Authorize(Policy = "EditStudentPaymentsPolicy")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentPayment = await _studentPaymentManager.GetByIdAsync((int)id);
        if (studentPayment == null)
        {
            return NotFound();
        }
        var classfeelist = await _classFeeListManager.GetAllByClassIdAsync(studentPayment.Student.AcademicClassId);
        List<StudentFeeHead> feeHeadList = (List<StudentFeeHead>)await _studentFeeHeadManager.GetAllAsync();
        AcademicSession currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();

        feeHeadList = (from f in feeHeadList
                       join t in classfeelist on f.Id equals t.StudentFeeHeadId
                       where t.AcademicSessionId == currentSession.Id
                       select f).ToList();

        ViewData["FeeList"] = new SelectList(feeHeadList, "Id", "Name", studentPayment.StudentPaymentDetails[0].StudentFeeHeadId);
        ViewData["StudentId"] = new SelectList(await _studentManager.GetAllAsync(), "Id", "Name", studentPayment.StudentId);
        return View(studentPayment);
    }

    // POST: StudentPayments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditStudentPaymentsPolicy")]
    public async Task<IActionResult> Edit(int id, StudentPayment studentPayment)
    {
        if (id != studentPayment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                StudentPayment existingStudentPayment = await _studentPaymentManager.GetByIdAsync(id);
                existingStudentPayment.EditedAt = DateTime.Now;
                existingStudentPayment.EditedBy = HttpContext.Session.GetString("UserId");
                existingStudentPayment.TotalPayment = studentPayment.StudentPaymentDetails.Sum(s => s.PaidAmount);
                existingStudentPayment.PaidDate = studentPayment.PaidDate;
                existingStudentPayment.MACAddress = MACService.GetMAC();
                existingStudentPayment.Student = studentPayment.Student = await _studentManager.GetByIdAsync(studentPayment.StudentId);
                existingStudentPayment.Remarks = studentPayment.Remarks;
                bool isUpdated = await _studentPaymentManager.UpdateAsync(existingStudentPayment);
                if (isUpdated)
                {
                    foreach (var item in studentPayment.StudentPaymentDetails)
                    {
                        StudentPaymentDetails existingDetails = await _studentPaymentDetailsManager.GetByIdAsync(item.Id);
                        existingDetails.EditedAt = DateTime.Now;
                        existingDetails.EditedBy = HttpContext.Session.GetString("UserId");
                        existingDetails.MACAddress = MACService.GetMAC();
                        existingDetails.StudentPaymentId = studentPayment.Id;
                        existingDetails.PaidAmount = item.PaidAmount;
                        existingDetails.StudentPayment = studentPayment;
                        await _studentPaymentDetailsManager.UpdateAsync(existingDetails);
                    }
                    TempData["updated"] = "Payment updated successfull";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentPaymentExists(studentPayment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        else
        {
            ViewData["FeeList"] = new SelectList(await _studentFeeHeadManager.GetAllAsync(), "Id", "Name", studentPayment.StudentPaymentDetails[0].StudentFeeHeadId);
            ViewData["StudentId"] = new SelectList(await _studentManager.GetAllAsync(), "Id", "Name", studentPayment.StudentId);
        }
        return RedirectToAction("Payment", new { stRoll = studentPayment.Student.ClassRoll });
    }

    // GET: StudentPayments/Delete/5
    [Authorize(Policy = "DeleteStudentPaymentsPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentPayment = await _studentPaymentManager.GetByIdAsync((int)id);
        if (studentPayment == null)
        {
            return NotFound();
        }

        return View(studentPayment);
    }

    // POST: StudentPayments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteStudentPaymentsPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var studentPayment = await _studentPaymentManager.GetByIdAsync((int)id);
        await _studentPaymentManager.RemoveAsync(studentPayment);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "DuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePayment()
    {
        GlobalUI.PageTitle = "Due Payment List";
        var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var model = new DuePaymentVM { AcademicSessionId = currentSession?.Id ?? 0 };
        await FillDuePaymentListsAsync(model);
        ViewBag.isFromPost = false;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePayment(DuePaymentVM filter)
    {
        GlobalUI.PageTitle = "Due Payment List";
        ViewBag.isFromPost = true;

        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            ModelState.AddModelError(nameof(DuePaymentVM.ToDate), "The 'To' date must be on or after the 'From' date.");
        }
        if (!ModelState.IsValid)
        {
            await FillDuePaymentListsAsync(filter);
            ViewBag.isFromPost = false;
            return View(filter);
        }

        var model = await BuildDuePaymentModelAsync(filter);
        model.DuePayments = DueListExport.Sort(model.DuePayments, DueListExport.DefaultSortKey, DueListExport.DefaultSortDirection);
        return View(model);
    }

    // GET: StudentPayments/DuePaymentExport?academicSessionId=..&fromDate=yyyy-MM-dd&format=xlsx|csv|pdf
    // Re-runs the page's query and applies the grid's current quick-search and sort, so the file
    // holds exactly the rows, in the order, that the user is looking at.
    [HttpGet]
    [Authorize(Policy = "DuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePaymentExport(DuePaymentVM filter, string search = null, string sortBy = null,
        string sortDir = null, string format = "xlsx")
    {
        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            return BadRequest("The 'To' date must be on or after the 'From' date.");
        }

        var model = await BuildDuePaymentModelAsync(filter, includeSelectLists: false);
        var rows = DueListExport.Sort(DueListExport.Search(model.DuePayments, search), sortBy, sortDir);
        var subtitle = $"Due Payments - {string.Join(" - ", model.DescribeFilters())} - {rows.Count} student(s) - "
            + $"Total due: {DueListExport.Amount(rows.Sum(r => r.TotalDue))} - {DateTime.Now:dd MMM yyyy}";
        return DueListFile(rows, format, model.Institute, subtitle, "Due_Payments");
    }

    [HttpGet]
    [Authorize(Policy = "PreviousDuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePaymentPrevious()
    {
        GlobalUI.PageTitle = "Previous Due Payment List";
        // Defaults reproduce the list's old result set: active students who still owe money.
        DuePaymentPreviousDto previousDuePaymentVM = new()
        {
            Status = "1",
            DueOnly = true,
            AcademicClassList = await GetAcademicClassListAsync(0),
            AcademicSectionList = new List<SelectListItem>(),
            StudentList = new List<SelectListItem>()
        };
        ViewBag.isFromPost = false;
        return View(previousDuePaymentVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PreviousDuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePaymentPrevious(DuePaymentPreviousDto modelObject)
    {
        GlobalUI.PageTitle = "Previous Due Payment List";
        var previousDuePaymentVM = await BuildPreviousDueModelAsync(modelObject);
        previousDuePaymentVM.DuePayments = DueListExport.Sort(previousDuePaymentVM.DuePayments,
            DueListExport.DefaultSortKey, DueListExport.DefaultSortDirection);
        ViewBag.isFromPost = true;
        return View(previousDuePaymentVM);
    }

    // GET: StudentPayments/DuePaymentPreviousExport?academicClassId=..&format=xlsx|csv|pdf
    // Re-runs the same query as the page and applies the grid's current quick-search and sort, so
    // the file holds exactly the rows, in the order, that the user is looking at.
    [HttpGet]
    [Authorize(Policy = "PreviousDuePaymentStudentPaymentsPolicy")]
    public async Task<IActionResult> DuePaymentPreviousExport(int academicClassId, int academicSectionId, int studentId,
        string status, bool dueOnly = true, string search = null, string sortBy = null, string sortDir = null, string format = "xlsx")
    {
        var filter = new DuePaymentPreviousDto
        {
            AcademicClassId = academicClassId,
            AcademicSectionId = academicSectionId,
            StudentId = studentId,
            Status = status,
            DueOnly = dueOnly
        };
        var model = await BuildPreviousDueModelAsync(filter, includeSelectLists: false);
        var rows = DueListExport.Sort(DueListExport.Search(model.DuePayments, search), sortBy, sortDir);

        var subtitle = $"Previous Due Payments (all sessions) - {PreviousDueScopeLabel(model)} - {rows.Count} student(s) - "
            + $"Total due: {DueListExport.Amount(rows.Sum(r => r.TotalDue))} - {DateTime.Now:dd MMM yyyy}";
        return DueListFile(rows, format, model.Institute, subtitle, "Previous_Due_Payments");
    }

    public async Task<string> GetReceiptNo(int studentId, int feeHeadId)
    {
        if (studentId >= 0)
        {
            Student stu = await _studentManager.GetByIdAsync(studentId);
            if (stu == null)
            {
                return "";
            }
        }
        else
        {
            TempData["errorMsg"] = "Student Id is not provide due to Get Receipt No";
            return "";
        }
        string receiptNo;
        try
        {
            receiptNo = await _studentPaymentManager.GetNewReceipt(studentId, feeHeadId);

        }
        catch (Exception)
        {
            throw;
        }
        return receiptNo;
    }

    public JsonResult GetTextByAmount(string amount)
    {
        string amountText = NumberToWords.ConvertAmount(Convert.ToDouble(amount));
        return Json("Taka " + amountText);
    }
    #endregion Action Methods

    #region Helper Methods
    private async Task<List<SessionWisePaymentVM>> GetSessionWisePaymentVMs(int academicSessionId, string studentUniqueId)
    {
        List<SessionWisePaymentVM> sessionWisePaymentVMs = new();
        var student = await _studentManager.GetStudentByUniqueIdAsync(studentUniqueId);
        var classFees = await _classFeeListManager.GetAllByClassIdAsync(student.AcademicClassId);
        classFees = classFees.Where(s => s.AcademicSessionId == academicSessionId && s.StudentFeeHead.IsResidential == student.IsResidential).ToList();
        if (classFees != null)
        {
            foreach (var item in classFees.OrderBy(s => s.StudentFeeHead.SL))
            {
                var pAmount = await GetPaidAmount(studentUniqueId, academicSessionId, item.StudentFeeHeadId);
                SessionWisePaymentVM sessionWisePaymentVM = new()
                {
                    FeeHeadName = item.StudentFeeHead.Name,
                    Amount = item.Amount,
                    PaidAmount = pAmount,
                    Balance = pAmount - item.Amount,
                    Status = pAmount == item.Amount ? "Paid" : pAmount < item.Amount ? "Patial Paid" : "Unpaid",
                };
                sessionWisePaymentVMs.Add(sessionWisePaymentVM);
            }
        }
        return sessionWisePaymentVMs;
    }

    private async Task<double> GetPaidAmount(string studentUniqueId, int aSessionId, int feeHeadId)
    {
        var paidAmount = 0.0;
        var student = await _studentManager.GetStudentByUniqueIdAsync(studentUniqueId);
        int isResidential = student.IsResidential ? 1 : 0;
        var result = await _studentPaymentManager.GetPaidAmountByFeeHeadAsync(studentUniqueId, aSessionId, isResidential, student.AcademicClassId, feeHeadId);
        if (result.Count > 0)
        {
            paidAmount = Convert.ToDouble(result.FirstOrDefault().PaidAmount);
        }
        return paidAmount;
    }

    private bool StudentPaymentExists(int id)
    {
        var r = _studentPaymentManager.GetById(id);
        if (r != null)
        {
            return true;
        }
        return false;
    }

    private void SetTempDataMessages()
    {
        var msg = "";
        if (TempData["success"] != null)
        {
            msg = TempData["success"].ToString();
            TempData["created"] = msg;
        }
        if (TempData["msg"] != null)
        {
            msg = TempData["msg"].ToString();
        }
        if (TempData["studentNotFound"] != null)
        {
            msg = TempData["studentNotFound"].ToString();
            TempData["failed"] = msg;
        }
        ViewBag.msg = msg;
    }

    private bool IsValidRoll(int? stRoll)
    {
        return stRoll > 0;
    }

    private async Task<StudentPaymentVM> CreateStudentPaymentVM(Student student)
    {
        var currentAcademicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var allSessions = await _academicSessionManager.GetAllAsync();
        var allClasses = await _academicClassManager.GetAllAsync();

        var spvm = new StudentPaymentVM
        {
            CurrentAcademicSession = currentAcademicSession,
            SelectedSessionId = currentAcademicSession.Id,
            SelectedClassId = student.AcademicClassId,
            AvailableSessions = allSessions.OrderByDescending(s => s.Name).ToList(),
            AvailableClasses = allClasses.OrderBy(c => c.ClassSerial).ToList(),
            StudentPayment = new StudentPayment
            {
                UniqueId = student.UniqueId,
                Student = student,
                StudentPaymentDetails = new List<StudentPaymentDetails> { new StudentPaymentDetails() }
            },
            StudentPreviousPayments = (List<StudentPayment>)await _studentPaymentManager.GetAllByStudentIdAsync(student.Id),
            StudentCurrentPayments = (List<StudentPayment>)await _studentPaymentManager.GetAllByStudentIdAsync(student.Id),
            StudentId = student.Id,
            ClassFeeLists = await GetClassFeeList(student)
        };

        var feeHeadList = await GetFeeHeadList(student, currentAcademicSession.Id);
        //check admission fee or session fee
        StudentFeeHead removeStudentFeeHead;
        var sessionYear = currentAcademicSession.Name.Substring(currentAcademicSession.Name.Length - 4, 4).ToString();
        var admissionYear = student.AdmissionDate.Year.ToString();

        if (student.IsResidential)
        {
            if (admissionYear == sessionYear)
            {
                removeStudentFeeHead = await _studentFeeHeadManager.GetByNameAsync("Session Fee Residential");
            }
            else
            {
                removeStudentFeeHead = await _studentFeeHeadManager.GetByNameAsync("Admission Fee Residential");
            }
        }
        else
        {

            if (admissionYear == sessionYear)
            {
                removeStudentFeeHead = await _studentFeeHeadManager.GetByNameAsync("Session Fee");
            }
            else
            {
                removeStudentFeeHead = await _studentFeeHeadManager.GetByNameAsync("Admission Fee");
            }
        }
        if (removeStudentFeeHead != null)
        {
            feeHeadList.RemoveAll(s => s.Name == removeStudentFeeHead.Name);
        }

        ViewData["FeeList"] = new SelectList(feeHeadList.OrderBy(s => s.SL), "Id", "Name");

        var studentPaymentDetailVM = await _studentPaymentManager.GetAllDetailPaymentByUniqueId(student.UniqueId);
        spvm.PaymentVM = studentPaymentDetailVM ?? new StudentPaymentDetailVM();

        return spvm;
    }

    private async Task<List<ClassFeeList>> GetClassFeeList(Student student, int? sessionId = null)
    {
        var sessionToUse = sessionId ?? student.AcademicSessionId;
        var allFees = await _classFeeListManager.GetAllByClassIdAsync(student.AcademicClassId);
        allFees = allFees.Where(s => s.AcademicSessionId == sessionToUse).ToList();

        return allFees;
    }

    private async Task<List<StudentFeeHead>> GetFeeHeadList(Student student, int? sessionId = null)
    {
        var sessionToUse = sessionId ?? student.AcademicSessionId;
        var feeHeadList = (List<StudentFeeHead>)await _studentFeeHeadManager.GetAllAsync();
        var classfeelist = await GetClassFeeList(student, sessionToUse);

        feeHeadList = (from f in feeHeadList
                       join t in classfeelist on f.Id equals t.StudentFeeHeadId
                       where t.AcademicSessionId == sessionToUse
                       select f).ToList();

        if (student.IsResidential)
        {
            feeHeadList = feeHeadList.Where(s => s.IsResidential).ToList();
        }
        else
        {
            feeHeadList = feeHeadList.Where(s => s.IsResidential == false).ToList();
        }

        return feeHeadList;
    }

    private async Task ProcessPayment(StudentPaymentVM paymentObject)
    {
        paymentObject.StudentPayment.ReceiptNo = await GetReceiptNo(paymentObject.StudentPayment.StudentId, paymentObject.ClassFeeHeadId);
        var studentPaymentObject = await CreateStudentPaymentObject(paymentObject);

        if (paymentObject.StudentPayment.StudentPaymentDetails != null)
        {
            foreach (var paymentDetails in paymentObject.StudentPayment.StudentPaymentDetails)
            {
                paymentDetails.CreatedAt = DateTime.Now;
                paymentDetails.CreatedBy = HttpContext.Session.GetString("UserId");
                paymentDetails.MACAddress = MACService.GetMAC();
                studentPaymentObject.StudentPaymentDetails.Add(paymentDetails);
            }

            bool isSaved = await _studentPaymentManager.AddAsync(studentPaymentObject);
            if (isSaved)
            {
                TempData["success"] = ViewBag.msg = "New payment added successfully!";
                if (paymentObject.IsSMSSend)
                {
                    await SendPaymentSMS(paymentObject, studentPaymentObject);
                }
            }
            else
            {
                TempData["fail"] = ViewBag.msg = "Failed to payment";
            }
        }
    }

    private async Task<StudentPayment> CreateStudentPaymentObject(StudentPaymentVM paymentObject)
    {
        return new StudentPayment
        {
            StudentId = paymentObject.StudentPayment.StudentId,
            TotalPayment = paymentObject.StudentPayment.TotalPayment,
            PaidDate = paymentObject.StudentPayment.PaidDate,
            Remarks = paymentObject.StudentPayment.Remarks,
            AcademicSessionId = paymentObject.CurrentAcademicSession.Id,
            UniqueId = await _studentManager.GetUniqueIdByStudentId(paymentObject.StudentPayment.StudentId),
            ReceiptNo = paymentObject.StudentPayment.ReceiptNo,
            CreatedAt = DateTime.Now,
            CreatedBy = HttpContext.Session.GetString("UserId"),
            MACAddress = MACService.GetMAC(),
            StudentPaymentDetails = new List<StudentPaymentDetails>()
        };
    }

    private async Task SendPaymentSMS(StudentPaymentVM paymentObject, StudentPayment studentPaymentObject)
    {
        var smsSetup = await _setupMobileSMSManager.GetByIdAsync(1);
        if (smsSetup.SMSService)
        {
            var studentObject = await _studentManager.GetByIdAsync(paymentObject.StudentPayment.StudentId);
            foreach (var item in studentPaymentObject.StudentPaymentDetails)
            {
                if (item.PaidAmount <= 0)
                {
                    item.PaidAmount = paymentObject.StudentPayment.TotalPayment;
                }
                var feeHead = await _studentFeeHeadManager.GetByIdAsync(item.StudentFeeHeadId);
                var instituteInfo = await _instituteManager.GetAllAsync();

                string smsText = $"{studentObject.Name} has Paid {item.PaidAmount}Tk as {feeHead.Name} - {instituteInfo.FirstOrDefault().Name}";
                string phoneNo = studentObject.GuardianPhone;
                bool isSend = await MobileSMS.SendSMS(phoneNo, smsText);
                if (isSend)
                {
                    await SaveSMSToDatabase(smsText, phoneNo);
                }
            }
        }
    }

    private async Task SaveSMSToDatabase(string smsText, string phoneNo)
    {
        var sms = new PhoneSMS
        {
            SMSType = "payment",
            MACAddress = MACService.GetMAC(),
            Text = smsText,
            MobileNumber = phoneNo,
            CreatedAt = DateTime.Now,
            CreatedBy = HttpContext.Session.GetString("UserId"),
            EditedAt = DateTime.Now,
            EditedBy = HttpContext.Session.GetString("UserId")
        };

        await _phoneSMSManager.AddAsync(sms);
    }

    private List<Student> FilterByResidentialStatus(List<Student> students, string isResidential)
    {
        return isResidential switch
        {
            "residential" => students.Where(s => s.IsResidential).ToList(),
            "nonResidential" => students.Where(s => !s.IsResidential).ToList(),
            _ => students
        };
    }

    private List<Student> FilterByStatus(List<Student> students, string status)
    {
        return status switch
        {
            "1" => students.Where(s => s.Status).ToList(),
            "0" => students.Where(s => !s.Status).ToList(),
            _ => students
        };
    }

    /// <summary>Builds the Excel (default), CSV or PDF download shared by both due lists.</summary>
    private FileContentResult DueListFile(List<DuePayment> rows, string format, Institute institute, string subtitle, string fileStem)
    {
        var headers = DueListExport.Headers;
        var data = DueListExport.Rows(rows);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmm");

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdf = new TabularPdfBuilder(institute?.Name ?? "Due Payments", subtitle, headers, data).GeneratePdf();
            return File(pdf, "application/pdf", $"{stamp}_{fileStem}.pdf");
        }
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return File(ExportWriter.ToCsv(headers, data), ExportWriter.CsvContentType, $"{stamp}_{fileStem}.csv");
        }
        var xlsx = ExportWriter.ToXlsx("Dues", headers, data, DueListExport.AmountColumnIndexes);
        return File(xlsx, ExportWriter.XlsxContentType, $"{stamp}_{fileStem}.xlsx");
    }

    /// <summary>
    /// Shared by the Due Payment page and its export: loads the session's dues for the filters
    /// (optionally limited to a fee period) plus what the view needs to redisplay them.
    /// </summary>
    private async Task<DuePaymentVM> BuildDuePaymentModelAsync(DuePaymentVM filter, bool includeSelectLists = true)
    {
        var model = new DuePaymentVM
        {
            AcademicSessionId = filter.AcademicSessionId,
            AcademicClassId = filter.AcademicClassId,
            AcademicSectionId = filter.AcademicSectionId,
            Status = filter.Status,
            Category = filter.Category,
            FromDate = filter.FromDate?.Date,
            ToDate = filter.ToDate?.Date,
            DueOnly = filter.DueOnly,
            Institute = await _instituteManager.GetFirstOrDefaultAsync()
        };

        if (model.AcademicSessionId <= 0)
        {
            var current = await _academicSessionManager.GetCurrentAcademicSessionAsync();
            model.AcademicSessionId = current?.Id ?? 0;
        }
        model.AcademicSession = await _academicSessionManager.GetByIdAsync(model.AcademicSessionId);
        model.SessionYear = DuePeriodRules.SessionYear(model.AcademicSession, DateTime.Today);
        if (model.AcademicClassId > 0)
            model.AcademicClass = await _academicClassManager.GetByIdAsync(model.AcademicClassId);
        if (model.AcademicSectionId > 0)
            model.AcademicSection = await _academicSectionManager.GetByIdAsync(model.AcademicSectionId);

        var students = await _studentManager.GetStudentsByClassSessionSectionAsync(model.AcademicSessionId, model.AcademicClassId, model.AcademicSectionId);
        students = FilterByResidentialStatus(students, model.Category);
        students = FilterByStatus(students, model.Status);

        var dues = await _studentPaymentManager.GetBulkDuePaymentsAsync(model.AcademicSessionId, students, model.FromDate, model.ToDate);
        model.DuePayments = dues
            .Where(d => !model.DueOnly || d.TotalDue > 0)
            .Select(d => new DuePayment { Student = d.Student, PayableAmount = d.PayableAmount, PaidAmount = d.PaidAmount, TotalDue = d.TotalDue })
            .ToList();

        if (includeSelectLists)
            await FillDuePaymentListsAsync(model);
        return model;
    }

    private async Task FillDuePaymentListsAsync(DuePaymentVM model)
    {
        var sessions = await _academicSessionManager.GetAllAsync();
        model.AcademicSessionList = sessions.OrderByDescending(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.AcademicSessionId))
            .ToList();
        model.SessionYear = DuePeriodRules.SessionYear(sessions.FirstOrDefault(s => s.Id == model.AcademicSessionId), DateTime.Today);
        model.AcademicClassList = await GetAcademicClassListAsync(model.AcademicClassId);
        model.AcademicSectionList = model.AcademicClassId > 0
            ? await GetSectionListAsync(model.AcademicClassId, model.AcademicSessionId, model.AcademicSectionId)
            : new List<SelectListItem>();
    }

    /// <summary>
    /// Shared by the Previous Due page and its export: loads the filtered dues plus everything the
    /// view needs to redisplay the chosen filters (names for the report header, dropdown lists).
    /// </summary>
    private async Task<DuePaymentPreviousDto> BuildPreviousDueModelAsync(DuePaymentPreviousDto filter, bool includeSelectLists = true)
    {
        var model = new DuePaymentPreviousDto
        {
            AcademicClassId = filter.AcademicClassId,
            AcademicSectionId = filter.AcademicSectionId,
            StudentId = filter.StudentId,
            Status = filter.Status,
            DueOnly = filter.DueOnly,
            Institute = await _instituteManager.GetFirstOrDefaultAsync()
        };

        if (filter.AcademicClassId > 0)
            model.AcademicClass = await _academicClassManager.GetByIdAsync(filter.AcademicClassId);
        if (filter.AcademicSectionId > 0)
            model.AcademicSection = await _academicSectionManager.GetByIdAsync(filter.AcademicSectionId);
        if (filter.StudentId > 0)
            model.Student = await _studentManager.GetByIdAsync(filter.StudentId);

        model.DuePayments = await _studentPaymentManager.GetPreviousDuesAsync(
            filter.StudentId, filter.AcademicSectionId, filter.AcademicClassId, ParseStudentStatus(filter.Status), filter.DueOnly);

        if (includeSelectLists)
        {
            model.AcademicClassList = await GetAcademicClassListAsync(filter.AcademicClassId);
            model.AcademicSectionList = new List<SelectListItem>();
            model.StudentList = new List<SelectListItem>();
            if (filter.AcademicClassId > 0)
            {
                // The dropdowns are cascaded for the current session, as the page's AJAX does.
                var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
                int sessionId = currentSession?.Id ?? 0;
                model.AcademicSectionList = await GetSectionListAsync(filter.AcademicClassId, sessionId, filter.AcademicSectionId);
                var students = await _studentManager.GetStudentsByClassSessionSectionAsync(sessionId, filter.AcademicClassId, filter.AcademicSectionId);
                model.StudentList = students.OrderBy(st => st.ClassRoll)
                    .Select(st => new SelectListItem($"{st.ClassRoll} - {st.Name}", st.Id.ToString(), st.Id == filter.StudentId))
                    .ToList();
            }
        }

        return model;
    }

    /// <summary>"1" = active, "0" = inactive, anything else = both.</summary>
    private static bool? ParseStudentStatus(string status) => status switch
    {
        "1" => true,
        "0" => false,
        _ => null
    };

    private static string PreviousDueScopeLabel(DuePaymentPreviousDto model)
    {
        var parts = new List<string> { model.AcademicClass?.Name ?? "All classes" };
        if (model.AcademicSection != null) parts.Add($"Section {model.AcademicSection.Name}");
        if (model.Student != null) parts.Add($"{model.Student.ClassRoll} - {model.Student.Name}");
        parts.Add(ParseStudentStatus(model.Status) switch
        {
            true => "Active students",
            false => "Inactive students",
            _ => "Active and inactive students"
        });
        return string.Join(", ", parts);
    }

    private async Task<List<SelectListItem>> GetAcademicClassListAsync(int selectedId)
    {
        var classes = await _academicClassManager.GetAllAsync();
        return new SelectList(classes.Where(c => c.Status), "Id", "Name", selectedId).ToList();
    }

    private async Task<List<SelectListItem>> GetSectionListAsync(int classId, int sessionId, int selectedId)
    {
        var sections = await _academicSectionManager.GetAllByClassWithSessionId(classId, sessionId);
        return new SelectList(sections, "Id", "Name", selectedId).ToList();
    }

    #endregion Helper Methods
}