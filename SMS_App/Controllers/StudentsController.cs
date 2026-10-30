using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentImport;
using SMS.Entities.AdditionalModels.StudentVM;
using SMS.Entities.Enums;
using SMS.Entities.Utilities;
using QuestPDF.Fluent;
using SMS_App.Utilities.Export;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.Reports;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.Pagination;
using SMS_App.Utilities.ShortMessageService;
using SMS_App.ViewModels;
using SMS_App.ViewModels.Students;

namespace SMS_App.Controllers;

[Authorize]
public class StudentsController : Controller
{
    #region Initialization
    private readonly IWebHostEnvironment _host;
    private readonly IStudentManager _studentManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IMapper _mapper;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IStudentPaymentManager _studentPaymentManager;
    private readonly IDivisionManager _divisionManager;
    private readonly IDistrictManager _districtManager;
    private readonly IUpazilaManager _upazilaManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly IBloodGroupManager _bloodGroupManager;
    private readonly INationalityManager _nationalityManager;
    private readonly IGenderManager _genderManager;
    private readonly IReligionManager _religionManager;
    private readonly IStudentFeeHeadManager _studentFeeHeadManager;
    private readonly IClassFeeListManager _classFeeListManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneSMSManager _phoneSMSManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IStudentActivateHistManager _studentActivateHistManager;
    private readonly IOffDayManager _offDayManager;
    private readonly IStudentFeeAllocationManager _studentFeeAllocationManager;
    private readonly IAppLogger _appLogger;
    private readonly IAcademicExamManager _academicExamManager;
    private readonly IAttachDocManager _attachDocManager;
    private readonly IStudentBulkImportManager _studentBulkImportManager;
    #endregion

    #region Constructor
    public StudentsController(IStudentManager studentManager, IAcademicClassManager academicClassManager, IWebHostEnvironment host, IMapper mapper, IAcademicSessionManager academicSessionManager, IStudentPaymentManager studentPaymentManager, IDistrictManager districtManager, IUpazilaManager upazilaManager, IAcademicSectionManager academicSectionManager, IBloodGroupManager bloodGroupManager, IDivisionManager divisionManager, INationalityManager nationalityManager, IGenderManager genderManager, IReligionManager religionManager, IStudentFeeHeadManager studentFeeHeadManager, IClassFeeListManager classFeeListManager, UserManager<ApplicationUser> userManager, IPhoneSMSManager phoneSMSManager, IAttendanceMachineManager attendanceMachineManager, IInstituteManager instituteManager, IStudentActivateHistManager studentActivateHistManager, IOffDayManager offDayManager, IStudentFeeAllocationManager studentFeeAllocationManager, IAppLogger appLogger, IStudentBulkImportManager studentBulkImportManager, IAcademicExamManager academicExamManager = null, IAttachDocManager attachDocManager = null)
    {
        _academicClassManager = academicClassManager;
        _host = host;
        _studentManager = studentManager;
        _academicSessionManager = academicSessionManager;
        _mapper = mapper;
        _studentPaymentManager = studentPaymentManager;
        _districtManager = districtManager;
        _upazilaManager = upazilaManager;
        _academicSectionManager = academicSectionManager;
        _bloodGroupManager = bloodGroupManager;
        _divisionManager = divisionManager;
        _nationalityManager = nationalityManager;
        _genderManager = genderManager;
        _religionManager = religionManager;
        _studentFeeHeadManager = studentFeeHeadManager;
        _classFeeListManager = classFeeListManager;
        _userManager = userManager;
        _phoneSMSManager = phoneSMSManager;
        _attendanceMachineManager = attendanceMachineManager;
        _instituteManager = instituteManager;
        _studentActivateHistManager = studentActivateHistManager;
        _offDayManager = offDayManager;
        _studentFeeAllocationManager = studentFeeAllocationManager;
        _appLogger = appLogger;
        _academicExamManager = academicExamManager;
        _attachDocManager = attachDocManager;
        _studentBulkImportManager = studentBulkImportManager;
    }
    #endregion Constructor

    #region Index
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexStudentsPolicy")]
    public async Task<IActionResult> Index(int? academicSessionId, int? academicClassId, int? academicSectionId, string aStatus, string sortOrder, string searchString, int? pageNumber, int? pageSize, string aCategory, bool reset = false)
    {
        // --- Remember the user's list context across navigation --------------------
        // The list is fully described by its query string. We stash that query string in
        // session so that returning to the list by ANY route (the left-nav menu, browser
        // Back, or a redirect after an edit) restores the same filters/search/page,
        // not just via the "Back to List" button. "Reset" clears it.
        const string filterSessionKey = "StudentsListFilter";

        if (reset)
        {
            HttpContext.Session.Remove(filterSessionKey);
            return RedirectToAction(nameof(Index));
        }

        bool hasRequestFilter = academicSessionId != null || academicClassId != null || academicSectionId != null
            || !string.IsNullOrEmpty(aStatus) || !string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(aCategory)
            || !string.IsNullOrEmpty(sortOrder) || pageNumber != null || pageSize != null;

        if (!hasRequestFilter)
        {
            // Bare /Students (sidebar menu, browser Back, etc.) - restore the last filter
            // the user worked with, if any, so their context is not silently lost.
            var savedFilter = HttpContext.Session.GetString(filterSessionKey);
            if (!string.IsNullOrEmpty(savedFilter))
            {
                return LocalRedirect(Url.Action(nameof(Index)) + savedFilter);
            }
        }
        else
        {
            // Remember exactly the list the user is looking at.
            HttpContext.Session.SetString(filterSessionKey, Request.QueryString.Value ?? string.Empty);
        }
        // ---------------------------------------------------------------------------

        // The grid itself is fed asynchronously by the List action (server-side
        // DataTables processing). Index only renders the shell and the filter controls,
        // pre-selected from the query string so a restored/bookmarked URL shows the same
        // filters and the grid's first AJAX call picks them up from the dropdowns.
        var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        if (academicSessionId == null)
        {
            academicSessionId = currentSession?.Id;
        }

        ViewData["searchString"] = searchString;
        ViewData["selectedAcademicClassId"] = academicClassId != null ? academicClassId.ToString() : "";
        ViewData["selectedAcademicSectionId"] = academicSectionId;
        ViewData["selectedAcademicSessionId"] = academicSessionId;
        ViewData["categoryId"] = aCategory;
        ViewData["statusId"] = aStatus;
        ViewData["pageSize"] = pageSize ?? 20;

        if (academicClassId != null)
        {
            var sections = await _academicSectionManager.GetAllByClassWithSessionId(Convert.ToInt32(academicClassId), currentSession.Id);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name", academicSectionId);
            ViewBag.selectedClassId = academicClassId;
        }
        else
        {
            ViewBag.selectedClassId = "";
        }

        var isActiveVMs = new List<IsActiveVM>
        {
            new IsActiveVM { Id = 0, sName = "Inactive" },
            new IsActiveVM { Id = 1, sName = "Active" },
            new IsActiveVM { Id = 2, sName = "All" },
        };

        ViewBag.academicSessionId = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicSessionId);
        var classes = await _academicClassManager.GetAllAsync();
        ViewBag.academicClassId = new SelectList(classes.Where(s => s.Status == true), "Id", "Name", academicClassId).ToList();
        ViewBag.aStatus = new SelectList(isActiveVMs, "Id", "sName", aStatus);

        var studentCategory = new List<SelectListItem>
        {
            new SelectListItem { Text = "all", Value = "all" },
            new SelectListItem { Text = "residential", Value = "residential" },
            new SelectListItem { Text = "nonResidential", Value = "nonResidential" }
        };
        ViewBag.aCategory = new SelectList(studentCategory, "Value", "Text", aCategory);

        return View();
    }

    /// <summary>
    /// Server-side DataTables endpoint for the student grid. Applies the filter dropdowns,
    /// the global search box, column sorting, and paging on the server so only one page of
    /// rows is ever serialized to the browser. Returns the shape the DataTables client
    /// expects: { draw, recordsTotal, recordsFiltered, data }.
    /// </summary>
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexStudentsPolicy")]
    public async Task<IActionResult> List(int? academicSessionId, int? academicClassId, int? academicSectionId,
        string aStatus, string aCategory)
    {
        // DataTables posts its state (draw/start/length/search/order + the per-column entries)
        // in the request BODY so the request isn't rejected by IIS's default query-string length
        // limit (~2 KB) once there are many columns. Read from the form on POST, else the query.
        string Param(string key) => Request.HasFormContentType ? Request.Form[key].ToString() : Request.Query[key].ToString();

        int draw = int.TryParse(Param("draw"), out var d) ? d : 0;
        int start = int.TryParse(Param("start"), out var s0) ? s0 : 0;
        int length = int.TryParse(Param("length"), out var len) ? len : 20;
        string search = Param("search[value]");
        int orderColIdx = int.TryParse(Param("order[0][column]"), out var oc) ? oc : -1;
        string orderDir = Param("order[0][dir]");
        string orderCol = orderColIdx >= 0 ? Param($"columns[{orderColIdx}][data]") : null;

        // Persist the live filter state so a bare /Students (menu, Back) restores it.
        var restoreQs = BuildStudentListQueryString(academicSessionId, academicClassId, academicSectionId, aStatus, aCategory, search);
        HttpContext.Session.SetString("StudentsListFilter", restoreQs);

        var allStudent = await _studentManager.GetAllAsync();
        var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();

        int? effectiveSessionId = academicSessionId ?? currentSession?.Id;
        allStudent = (allStudent ?? Enumerable.Empty<Student>())
            .Where(s => s.AcademicSessionId == effectiveSessionId).ToList();

        if (academicClassId != null)
            allStudent = allStudent.Where(s => s.AcademicClassId == academicClassId.Value).ToList();
        if (academicSectionId != null)
            allStudent = allStudent.Where(s => s.AcademicSectionId == academicSectionId.Value).ToList();
        if (aCategory == "residential")
            allStudent = allStudent.Where(s => s.IsResidential).ToList();
        else if (aCategory == "nonResidential")
            allStudent = allStudent.Where(s => !s.IsResidential).ToList();
        if (aStatus == "0" || aStatus == "1")
        {
            bool isActive = aStatus == "1";
            allStudent = allStudent.Where(s => s.Status == isActive).ToList();
        }

        var rows = allStudent.Select(s => new SMS.Entities.AdditionalModels.StudentListVM
        {
            Id = s.Id,
            ClassRoll = s.ClassRoll,
            Photo = s.Photo,
            StudentName = s.Name,
            NameBangla = s.NameBangla,
            ClassName = s.AcademicClass?.Name,
            SectionName = s.AcademicSection?.Name,
            PhoneNo = s.PhoneNo,
            GuardianPhone = s.GuardianPhone,
            SessionName = s.AcademicSession?.Name,
            Gender = s.Gender?.Name,
            Status = s.Status,
            ClassSerial = s.AcademicClass?.ClassSerial,
            IsResidential = s.IsResidential,
            UniqueId = s.UniqueId,
            AcademicSessionId = s.AcademicSessionId
        }).ToList();

        int recordsTotal = rows.Count;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            rows = rows.Where(r =>
                (r.StudentName?.ToLowerInvariant().Contains(term) ?? false)
                || (r.NameBangla?.Contains(search) ?? false)
                || r.ClassRoll.ToString().Contains(term)
                || (r.UniqueId?.ToLowerInvariant().Contains(term) ?? false)
                || (r.PhoneNo?.Contains(term) ?? false)
                || (r.GuardianPhone?.Contains(term) ?? false)).ToList();
        }

        int recordsFiltered = rows.Count;

        bool desc = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase);
        IEnumerable<SMS.Entities.AdditionalModels.StudentListVM> sorted = orderCol switch
        {
            "uniqueId" => desc ? rows.OrderByDescending(r => r.UniqueId) : rows.OrderBy(r => r.UniqueId),
            "classRoll" => desc ? rows.OrderByDescending(r => r.ClassRoll) : rows.OrderBy(r => r.ClassRoll),
            "studentName" => desc ? rows.OrderByDescending(r => r.StudentName) : rows.OrderBy(r => r.StudentName),
            "className" => desc ? rows.OrderByDescending(r => r.ClassSerial) : rows.OrderBy(r => r.ClassSerial),
            "sectionName" => desc ? rows.OrderByDescending(r => r.SectionName) : rows.OrderBy(r => r.SectionName),
            "phoneNo" => desc ? rows.OrderByDescending(r => r.PhoneNo) : rows.OrderBy(r => r.PhoneNo),
            "guardianPhone" => desc ? rows.OrderByDescending(r => r.GuardianPhone) : rows.OrderBy(r => r.GuardianPhone),
            "sessionName" => desc ? rows.OrderByDescending(r => r.SessionName) : rows.OrderBy(r => r.SessionName),
            "gender" => desc ? rows.OrderByDescending(r => r.Gender) : rows.OrderBy(r => r.Gender),
            "status" => desc ? rows.OrderByDescending(r => r.Status) : rows.OrderBy(r => r.Status),
            // Default: current session first, then by class order and roll (the historical order).
            _ => rows.OrderByDescending(r => r.AcademicSessionId).ThenBy(r => r.ClassSerial).ThenBy(r => r.ClassRoll)
        };

        var paged = (length > 0 ? sorted.Skip(start).Take(length) : sorted).ToList();

        var textInfo = new CultureInfo("en-US", false).TextInfo;
        var data = paged.Select(r => new
        {
            id = r.Id,
            photoUrl = ResolveStudentPhotoUrl(r.Photo),
            uniqueId = r.UniqueId,
            classRoll = r.ClassRoll,
            studentName = textInfo.ToTitleCase(r.StudentName ?? string.Empty),
            nameBangla = r.NameBangla,
            className = r.ClassName,
            sectionName = r.SectionName ?? "N/A",
            phoneNo = r.PhoneNo?.PadLeft(11, '0') ?? "",
            guardianPhone = r.GuardianPhone?.PadLeft(11, '0') ?? "",
            sessionName = r.SessionName,
            gender = r.Gender,
            status = r.Status,
            isResidential = r.IsResidential
        });

        return Json(new { draw, recordsTotal, recordsFiltered, data });
    }

    /// <summary>Resolves a student's avatar URL, cache-busting replaced photos by last-write time.</summary>
    private string ResolveStudentPhotoUrl(string photo)
    {
        if (string.IsNullOrEmpty(photo)) return "/Images/noProfileImage.png";
        var path = System.IO.Path.Combine(_host.WebRootPath, "Images/Student/", photo);
        if (!System.IO.File.Exists(path)) return "/Images/noProfileImage.png";
        return $"/Images/Student/{photo}?v={System.IO.File.GetLastWriteTimeUtc(path).Ticks}";
    }

    /// <summary>Builds the Index query string that reproduces a given filter state.</summary>
    private static string BuildStudentListQueryString(int? academicSessionId, int? academicClassId,
        int? academicSectionId, string aStatus, string aCategory, string searchString)
    {
        var parts = new List<string>();
        if (academicSessionId != null) parts.Add($"academicSessionId={academicSessionId}");
        if (academicClassId != null) parts.Add($"academicClassId={academicClassId}");
        if (academicSectionId != null) parts.Add($"academicSectionId={academicSectionId}");
        if (!string.IsNullOrEmpty(aStatus)) parts.Add($"aStatus={Uri.EscapeDataString(aStatus)}");
        if (!string.IsNullOrEmpty(aCategory)) parts.Add($"aCategory={Uri.EscapeDataString(aCategory)}");
        if (!string.IsNullOrEmpty(searchString)) parts.Add($"searchString={Uri.EscapeDataString(searchString)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    #endregion Index

    #region Details
    // GET: Students/Details/5
    [Authorize, AllowAnonymous]
    [Authorize(Policy = "DetailsStudentsPolicy")]
    public async Task<IActionResult> Details(int? id, string tabName)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user.UserType == 's')
        {
            if (user.ReferenceId != id)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }

        var student = await _studentManager.GetByIdAsync((int)id);
        if (student == null)
        {
            return NotFound();
        }
        StudentDetailsVM sd = new();

        //personal details
        sd.PersonalDetails = GetPersonalData(student);

        //documents details
        sd.Documents = await _attachDocManager.GetAllDocumentsByStudentId(student.Id);

        #region Payment==========================================================================
        var stuPayments = await _studentPaymentManager.GetAllByStudentIdAsync((int)id);

        List<StudentPaymentScheduleVM> paymentSchedule = await _studentPaymentManager.GetStudentPaymentSchedule(student.Id);

        paymentSchedule = paymentSchedule.Where(s => s.IsResidential == student.IsResidential && s.Amount > 0).ToList();
        List<StudentPaymentSchedulePaidVM> studentPaymentSchedulePaidVMs = await _studentPaymentManager.GetStudentPaymentSchedulePaid(student.Id);

        sd.StudentPayments = stuPayments;
        sd.Student = student;

        sd.StudentPaymentSchedules = paymentSchedule;
        sd.StudentPaymentSchedulePaidVMs = studentPaymentSchedulePaidVMs;

        sd.TotalDue = await _studentPaymentManager.GetStudentCurrentDue(student.Id); /* await GetTotalDue(student.Id);*/
        sd.CurrentDue = await _studentPaymentManager.GetStudentCurrentDue(student.Id);
        #endregion Payment============================================================================

        #region Attendance =============================================================================
        try
        {
            int startingMonth = Convert.ToInt32(student.AdmissionDate.Date.ToString("MM"));
            if (startingMonth < 12)
            {
                startingMonth = 1;
            }
            int presentMonth = Convert.ToInt32(DateTime.Now.Date.ToString("MM"));
            List<AttendanceIndivisualVM> attendanceIndivisualVMs = new List<AttendanceIndivisualVM>();
            for (int i = startingMonth; i <= presentMonth; i++)
            {
                var monthYear = i.ToString() + DateTime.Now.Year;
                AttendanceIndivisualVM attendanceIndivisualVM = new AttendanceIndivisualVM();
                var monthlyAttendance = await _attendanceMachineManager.GetAttendanceByMonthSingleStudent(student.Id, monthYear);
                attendanceIndivisualVM.AttendanceCount = monthlyAttendance.Count;
                var holidays = await _offDayManager.GetMonthlyHolidaysAsync(i.ToString().PadLeft(2, '0') + DateTime.Now.Year.ToString());
                attendanceIndivisualVM.TotalDays = DateTime.DaysInMonth(DateTime.Now.Year, i) - holidays.Count;
                if (monthlyAttendance.Count > 0)
                {
                    attendanceIndivisualVM.PresentPercentage = (monthlyAttendance.Count * 100) / (attendanceIndivisualVM.TotalDays);
                }
                else
                {
                    attendanceIndivisualVM.PresentPercentage = 0;
                }
                attendanceIndivisualVM.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                attendanceIndivisualVMs.Add(attendanceIndivisualVM);
            }
            sd.AttendanceDetails = attendanceIndivisualVMs;
        }
        catch (Exception)
        {

            throw;
        }
        #endregion Attendance =====================================================================================================================

        ViewBag.districts = await _districtManager.GetAllAsync();
        ViewBag.Upazila = await _upazilaManager.GetAllAsync();
        ViewBag.tabName = "";
        if (!string.IsNullOrEmpty(tabName))
        {
            ViewBag.tabName = tabName;
        }
        var activityHist = await _studentActivateHistManager.GetActivityListByUniqueId(student.UniqueId);
        List<StudentActivateHistModel> activ = new List<StudentActivateHistModel>();
        foreach (var item in activityHist)
        {
            var a = new StudentActivateHistModel()
            {
                StudentId = item.StudentId,
                IsActive = item.IsActive,
                ActionDateTime = item.ActionDateTime,
            };
            activ.Add(a);
        }
        sd.StatusActivity = activ;
        return View(sd);
    }
    #endregion

    #region Create
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateStudentsPolicy")]
    public async Task<IActionResult> Create(string returnUrl = null)
    {
        // Carry the originating list context so a successful create / Back returns to it.
        ViewData["ReturnUrl"] = returnUrl;
        StudentCreateVM student = new();
        student.AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name").ToList();
        student.AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList();
        student.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name").ToList();
        student.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name").ToList();
        student.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name").ToList();
        student.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name").ToList();
        student.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name").ToList();

        return View(student);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateStudentsPolicy")]
    public async Task<IActionResult> Create([Bind("Id,Name,NameBangla,ClassRoll,FatherName,MotherName,AdmissionDate,Email,PhoneNo,Photo,DOB,BirthCertificateNo,BirthCertificateImage,ReligionId,GenderId,BloodGroupId,NationalityId,PresentAddressArea,PresentAddressPO,PresentUpazilaId,PresentDistrictId,PresentDivisionId,PermanentAddressArea,PermanentAddressPO,PermanentUpazilaId,PermanentDistrictId,PermanentDivisionId,AcademicSessionId,AcademicClassId,AcademicSectionId,AddressInfo,PreviousSchool,Status,CreatedBy,CreatedAt,EditedBy,EditedAt,GuardianPhone,MACAddress,IsResidential,SMSService, UniqueId")] StudentCreateVM newStudent, IFormFile sPhoto, IFormFile DOBFile, string returnUrl = null)
    {
        // Keep the originating list context available to the view if we have to re-render.
        ViewData["ReturnUrl"] = returnUrl;
        newStudent.ClassRoll = await CreateRoll(newStudent.AcademicSessionId, newStudent.AcademicClassId, newStudent.ClassRoll);
        var rollIsExist = await _studentManager.GetStudentByClassRollAsync(newStudent.ClassRoll);
        if (rollIsExist == null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (HttpContext.Session.GetString("UserId") == null)
                    {
                        return RedirectToAction("Login", "Accounts");
                    }

                    await _appLogger.InfoAsync($"Model state is valid");
                    newStudent.CreatedBy = HttpContext.Session.GetString("UserId");
                    newStudent.CreatedAt = DateTime.Now;
                    newStudent.EditedAt = DateTime.Now;
                    newStudent.EditedBy = HttpContext.Session.GetString("UserId");

                    var student = _mapper.Map<Student>(newStudent);
                    student.UniqueId = await GenerateUniquId(student);
                    if (sPhoto != null && sPhoto.Length > 0)
                    {
                        await _appLogger.InfoAsync($"Image Processing Started");
                        // Allowed extensions
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                        // Get extension in lowercase
                        string fileExt = Path.GetExtension(sPhoto.FileName).ToLowerInvariant();

                        // Validate extension
                        if (!allowedExtensions.Contains(fileExt))
                        {

                            await _appLogger.InfoAsync($"file extension not allowed: {fileExt}");
                            throw new InvalidOperationException("Only .jpg, .jpeg, and .png files are allowed.");
                        }

                        // Validate size (1 MB = 1 * 1024 * 1024 bytes)
                        const long maxFileSize = 1 * 1024 * 1024;
                        if (sPhoto.Length > maxFileSize)
                        {

                            await _appLogger.InfoAsync($"file size exceeded: {sPhoto.Length} bytes");
                            throw new InvalidOperationException("File size must not exceed 1 MB.");
                        }

                        await _appLogger.InfoAsync($"File validation passed");
                        // Prepare paths
                        string root = _host.WebRootPath;
                        string folder = Path.Combine("Images", "Student");

                        // Ensure folder exists
                        string fullFolderPath = Path.Combine(root, folder);
                        if (!Directory.Exists(fullFolderPath))
                        {
                            Directory.CreateDirectory(fullFolderPath);
                        }

                        // Create file name
                        var session = await _academicSessionManager.GetByIdAsync(student.AcademicSessionId);
                        string sessionYear = session.Name.ToString();
                        string year = sessionYear.Split('-').Last();
                        string fileName = $"S_{year}_{student.UniqueId}{fileExt}";

                        // Combine final path
                        string filePath = Path.Combine(fullFolderPath, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await sPhoto.CopyToAsync(stream);
                        }

                        // Save file name in DB
                        newStudent.Photo = fileName;
                    }

                    if (DOBFile != null && DOBFile.Length > 0)
                    {
                        //Allowed extensions
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

                        //Get extension in lowercase
                        string fileExt = Path.GetExtension(DOBFile.FileName).ToLowerInvariant();

                        // Validate extension
                        if (!allowedExtensions.Contains(fileExt))
                        {
                            throw new InvalidOperationException("Only .jpg, .jpeg, .png, .pdf files are allowed.");
                        }

                        // Validate size (1 MB = 1 * 1024 * 1024 bytes)
                        const long maxFileSize = 1 * 1024 * 1024;
                        if (DOBFile.Length > maxFileSize)
                        {
                            throw new InvalidOperationException("File size must not exceed 1 MB.");
                        }

                        string root = _host.WebRootPath;
                        string folder = "Images/Student/";

                        // Ensure folder exists
                        string fullFolderPath = Path.Combine(root, folder);
                        if (!Directory.Exists(fullFolderPath))
                        {
                            Directory.CreateDirectory(fullFolderPath);
                        }

                        string fileName = $"S_DOB_{newStudent.DOB.ToString("ddMMyyyy")}_{newStudent.UniqueId}{fileExt}";
                        string pathCombine = Path.Combine(fullFolderPath, fileName);

                        using (var stream = new FileStream(pathCombine, FileMode.Create))
                        {
                            await DOBFile.CopyToAsync(stream);
                        }
                        newStudent.BirthCertificateImage = fileName;
                    }
                    student.MACAddress = MACService.GetMAC();
                    bool saveStudent = await _studentManager.AddAsync(student);
                    if (saveStudent == true)
                    {
                        StudentActivateHist studentActivateHist = new()
                        {
                            StudentId = student.Id,
                            IsActive = true,
                            ActionDateTime = DateTime.Now,
                            LastAction = "Add",
                            CreatedAt = DateTime.Now,
                            CreatedBy = HttpContext.Session.GetString("UserId"),
                            MACAddress = MACService.GetMAC()
                        };
                        await _studentActivateHistManager.AddAsync(studentActivateHist);

                        TempData["create"] = "Created Successfully";
                        ApplicationUser newStudentUser = new()
                        {
                            UserName = student.UniqueId,
                            Email = student.Email,
                            EmailConfirmed = true,
                            PhoneNumberConfirmed = true,
                            PhoneNumber = student.PhoneNo,
                            NormalizedUserName = student.Name,
                            UserType = 's',
                            ReferenceId = Convert.ToInt32(student.UniqueId)
                        };

                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                        var random = new Random();
                        string autoGeneratedPassword = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());

                        var result = await _userManager.CreateAsync(newStudentUser, autoGeneratedPassword);
                        if (result.Succeeded)
                        {
                            var roleAssignResult = await _userManager.AddToRoleAsync(newStudentUser, "Student");
                            if (roleAssignResult.Succeeded)
                            {
                                var instituteInfo = await _instituteManager.GetAllAsync();
                                string text = "Dear,\n" + student.Name + ",\nYour User: " + newStudentUser.UserName + "\nPassword:" + autoGeneratedPassword + "\n-" + instituteInfo.FirstOrDefault().Name;
                                bool smsSend = await MobileSMS.SendSMS(student.PhoneNo, text);
                                if (true)
                                {
                                    PhoneSMS phoneSMS = new()
                                    {
                                        Text = text,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = "System",
                                        EditedAt = DateTime.Now,
                                        EditedBy = "System",
                                        MobileNumber = student.PhoneNo,
                                        MACAddress = MACService.GetMAC(),
                                        SMSType = "NewUser"
                                    };
                                    await _phoneSMSManager.AddAsync(phoneSMS);
                                }

                                if (newStudent.Email != null)
                                {

                                }
                            }
                        }
                        return RedirectToList(returnUrl);
                    }
                }
            }
            catch (Exception e)
            {
                await _appLogger.ErrorAsync(e.Message, e.StackTrace);
                throw;
            }
        }
        else
        {
            ViewBag.msg = "Roll number is already exist";
        }

        newStudent.AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", newStudent.AcademicSessionId).ToList();
        newStudent.AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", newStudent.AcademicClassId).ToList();
        newStudent.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name", newStudent.BloodGroupId).ToList();
        newStudent.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", newStudent.GenderId).ToList();
        newStudent.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", newStudent.NationalityId).ToList();
        newStudent.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", newStudent.ReligionId).ToList();
        newStudent.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name").ToList();



        return View(newStudent);
    }
    #endregion Create

    #region Bulk Upload

    /// <summary>Folder under App_Data where an uploaded roster waits between preview and confirm.</summary>
    private string BulkUploadTempFolder => Path.Combine(_host.ContentRootPath, "App_Data", "BulkImports");

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "BulkUploadStudentsPolicy")]
    public IActionResult BulkUpload()
    {
        PurgeStaleUploads();
        return View(new StudentBulkUploadVM
        {
            TemplateColumns = _studentBulkImportManager.GetTemplateColumns().ToList()
        });
    }

    /// <summary>
    /// Removes preview files left behind when a user uploaded but never confirmed.
    /// Without this the temp folder grows for ever on a long-running site.
    /// </summary>
    private void PurgeStaleUploads()
    {
        try
        {
            if (!Directory.Exists(BulkUploadTempFolder)) return;

            var cutoff = DateTime.Now.AddHours(-24);
            foreach (var file in Directory.GetFiles(BulkUploadTempFolder))
            {
                if (System.IO.File.GetLastWriteTime(file) < cutoff)
                    TryDeleteTempFile(file);
            }
        }
        catch
        {
            // Housekeeping only - never block the page over it.
        }
    }

    /// <summary>
    /// Step 1: read the uploaded roster and show the user exactly what will be
    /// created. Nothing is written to the database here.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "BulkUploadStudentsPolicy")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> BulkUpload(IFormFile uploadFile)
    {

        var model = new StudentBulkUploadVM
        {
            TemplateColumns = _studentBulkImportManager.GetTemplateColumns().ToList()
        };

        if (uploadFile == null || uploadFile.Length == 0)
        {
            ViewBag.msg = "Please choose a .csv or .xlsx file to upload.";
            return View(model);
        }

        string extension = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".xlsx")
        {
            ViewBag.msg = "Only .csv and .xlsx files are supported.";
            return View(model);
        }

        try
        {
            // Keep the file so the confirm step does not need a second upload.
            Directory.CreateDirectory(BulkUploadTempFolder);
            string token = Guid.NewGuid().ToString("N") + extension;
            string tempPath = Path.Combine(BulkUploadTempFolder, token);

            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await uploadFile.CopyToAsync(fileStream);
            }

            using (var readStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
            {
                model.Result = await _studentBulkImportManager.ParseAndValidateAsync(readStream, uploadFile.FileName);
            }

            model.FileToken = token;
            model.OriginalFileName = uploadFile.FileName;

            await _appLogger.InfoAsync(
                $"Bulk student upload previewed: {uploadFile.FileName}, " +
                $"{model.Result.ValidRows} valid / {model.Result.TotalRows} rows.");
        }
        catch (Exception ex)
        {
            await _appLogger.ErrorAsync(ex.Message, ex.StackTrace);
            ViewBag.msg = "The file could not be read. Please check the format and try again.";
        }

        return View(model);
    }

    /// <summary>
    /// Step 2: the user confirmed the preview. The file is re-read and re-validated
    /// so that anything added by another user in the meantime is still caught, then
    /// the valid rows are saved and a login is created for each - no SMS is sent.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "BulkUploadStudentsPolicy")]
    public async Task<IActionResult> BulkUploadConfirm(string fileToken, string originalFileName)
    {

        var model = new StudentBulkUploadVM
        {
            TemplateColumns = _studentBulkImportManager.GetTemplateColumns().ToList()
        };

        if (HttpContext.Session.GetString("UserId") == null)
            return RedirectToAction("Login", "Accounts");

        // Reject anything that is not a token this action itself issued.
        if (string.IsNullOrWhiteSpace(fileToken) || fileToken.Contains("..") ||
            fileToken.Contains('/') || fileToken.Contains('\\'))
        {
            ViewBag.msg = "The upload session is no longer valid. Please upload the file again.";
            return View(nameof(BulkUpload), model);
        }

        string tempPath = Path.Combine(BulkUploadTempFolder, fileToken);
        if (!System.IO.File.Exists(tempPath))
        {
            ViewBag.msg = "The uploaded file has expired. Please upload it again.";
            return View(nameof(BulkUpload), model);
        }

        try
        {
            StudentImportResult result;
            using (var readStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
            {
                result = await _studentBulkImportManager.ParseAndValidateAsync(readStream, originalFileName ?? fileToken);
            }

            if (!result.CanImport)
            {
                model.Result = result;
                model.FileToken = fileToken;
                model.OriginalFileName = originalFileName;
                ViewBag.msg = "Nothing could be imported. Please fix the errors listed below and upload again.";
                return View(nameof(BulkUpload), model);
            }

            string userId = HttpContext.Session.GetString("UserId");
            string mac = MACService.GetMAC();

            var savedStudents = await _studentBulkImportManager.CommitAsync(result, userId, mac);

            var commit = new StudentImportCommitResult { StudentsCreated = savedStudents.Count };

            // Give every imported student a login, exactly as Create does, but stay
            // silent - a bulk import must not text hundreds of parents.
            foreach (var student in savedStudents)
            {
                var studentUser = new ApplicationUser
                {
                    UserName = student.UniqueId,
                    Email = student.Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    PhoneNumber = student.PhoneNo,
                    NormalizedUserName = student.Name,
                    UserType = 's',
                    ReferenceId = Convert.ToInt32(student.UniqueId)
                };

                string password = GenerateStudentPassword();
                var createResult = await _userManager.CreateAsync(studentUser, password);

                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(studentUser, "Student");
                    commit.AccountsCreated++;
                    commit.Credentials.Add(new StudentImportCredential
                    {
                        Name = student.Name,
                        ClassRoll = student.ClassRoll,
                        UserName = student.UniqueId,
                        Password = password,
                        PhoneNo = student.PhoneNo
                    });
                }
                else
                {
                    commit.Failures.Add(new StudentImportIssue
                    {
                        Column = "Login",
                        Value = student.UniqueId,
                        Message = $"Student '{student.Name}' was saved but the login could not be created: "
                                + string.Join("; ", createResult.Errors.Select(e => e.Description)),
                        Level = ImportIssueLevel.Warning
                    });
                }
            }

            // Rows that failed validation are still worth showing after the import.
            model.Result = result;
            model.CommitResult = commit;
            model.OriginalFileName = originalFileName;

            TempData["create"] = $"{commit.StudentsCreated} student(s) imported successfully.";
            await _appLogger.InfoAsync(
                $"Bulk student import committed: {commit.StudentsCreated} students, {commit.AccountsCreated} logins.");

            // Hold the credentials so they can be downloaded once from the result page.
            HttpContext.Session.SetString("BulkImportCredentials", BuildCredentialsCsv(commit.Credentials));
        }
        catch (Exception ex)
        {
            await _appLogger.ErrorAsync(ex.Message, ex.StackTrace);
            ViewBag.msg = "The import failed. Please check the log and try again.";
        }
        finally
        {
            TryDeleteTempFile(tempPath);
        }

        return View(nameof(BulkUpload), model);
    }

    /// <summary>Downloads the logins generated by the most recent import in this session.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "BulkUploadStudentsPolicy")]
    public IActionResult DownloadImportCredentials()
    {
        string csv = HttpContext.Session.GetString("BulkImportCredentials");
        if (string.IsNullOrEmpty(csv))
        {
            TempData["failed"] = "No credentials are available to download.";
            return RedirectToAction(nameof(BulkUpload));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", $"student-logins-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    /// <summary>Downloads an empty roster template with the expected header and one sample row.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "BulkUploadStudentsPolicy")]
    public IActionResult DownloadTemplate()
    {
        var columns = _studentBulkImportManager.GetTemplateColumns();
        var sample = _studentBulkImportManager.GetTemplateSampleRow();

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", columns.Select(CsvEscape)));
        builder.AppendLine(string.Join(",", sample.Select(CsvEscape)));

        // The BOM makes Excel open the Bangla sample text correctly.
        var bytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(builder.ToString()))
            .ToArray();

        return File(bytes, "text/csv", "student-import-template.csv");
    }

    private static string BuildCredentialsCsv(List<StudentImportCredential> credentials)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Name,ClassRoll,UserName,Password,PhoneNo");
        foreach (var c in credentials)
        {
            builder.AppendLine(string.Join(",", new[]
            {
                CsvEscape(c.Name),
                CsvEscape(c.ClassRoll.ToString()),
                CsvEscape(c.UserName),
                CsvEscape(c.Password),
                CsvEscape(c.PhoneNo)
            }));
        }
        return builder.ToString();
    }

    private static string CsvEscape(string value)
    {
        value ??= string.Empty;
        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static string GenerateStudentPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private void TryDeleteTempFile(string path)
    {
        try
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
        catch
        {
            // A leftover temp file is harmless; never fail the import over it.
        }
    }

    #endregion Bulk Upload

    #region Edit
    [HttpGet, Authorize(Roles = "SuperAdmin, Admin", Policy = "EditStudentsPolicy")]
    public async Task<IActionResult> Edit(int? id, string returnUrl = null)
    {
        if (id == null)
        {
            return NotFound();
        }
        var student = await _studentManager.GetByIdAsync((int)id);
        if (student == null)
        {
            return NotFound();
        }
        // Carry the originating list context so Save/Back return to the same filtered list.
        ViewData["ReturnUrl"] = returnUrl;
        List<AcademicSection> academicSections = (List<AcademicSection>)await _academicSectionManager.GetAllAsync();

        var newStudent = _mapper.Map<StudentEditVM>(student);
        newStudent.AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", newStudent.AcademicSessionId).ToList();
        newStudent.AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", newStudent.AcademicClassId).ToList();

        newStudent.AcademicSectionList = new SelectList(academicSections.Where(m => m.AcademicClassId == student.AcademicClassId && m.AcademicSessionId == student.AcademicSessionId), "Id", "Name", newStudent.AcademicSectionId).ToList();
        newStudent.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name", newStudent.BloodGroupId).ToList();
        newStudent.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", newStudent.GenderId).ToList();
        newStudent.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", newStudent.NationalityId).ToList();
        newStudent.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", newStudent.ReligionId).ToList();
        newStudent.PresentDivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name", newStudent.PresentDivisionId).ToList();
        newStudent.PermanentDivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name", newStudent.PermanentDivisionId).ToList();
        ViewData["DistrictList"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", newStudent.PresentDistrictId);
        ViewData["UpazilaList"] = new SelectList(await _upazilaManager.GetAllAsync(), "Id", "Name", newStudent.PresentDistrictId);

        return View(newStudent);
    }


    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditStudentsPolicy")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,NameBangla,ClassRoll,FatherName,MotherName,AdmissionDate,Email,PhoneNo,Photo,DOB,BirthCertificateNo,BirthCertificateImage,ReligionId,GenderId,BloodGroupId,NationalityId,PresentAddressArea,PresentAddressPO,PresentUpazilaId,PresentDistrictId,PresentDivisionId,PermanentAddressArea,PermanentAddressPO,PermanentUpazilaId,PermanentDistrictId,PermanentDivisionId,AcademicSessionId,AcademicClassId,AcademicSectionId,AddressInfo,PreviousSchool,CreatedBy,CreatedAt,EditedBy,EditedAt,GuardianPhone,Status,MACAddress,IsResidential,SMSService,UniqueId")] Student student, IFormFile sPhoto, IFormFile DOBFile, string returnUrl = null)
    {
        // Keep the originating list context available to the view if we have to re-render.
        ViewData["ReturnUrl"] = returnUrl;
        if (id != student.Id)
        {
            return NotFound();
        }
        var rollIsExist = await _studentManager.GetStudentByClassRollAsync(id, student.ClassRoll);
        if (rollIsExist == null)
        {

            if (ModelState.IsValid)
            {
                bool isUpdated = false;
                try
                {
                    if (sPhoto != null && sPhoto.Length > 0)
                    {
                        // Allowed extensions
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                        // Get extension in lowercase
                        string fileExt = Path.GetExtension(sPhoto.FileName).ToLowerInvariant();

                        // Validate extension
                        if (!allowedExtensions.Contains(fileExt))
                        {
                            throw new InvalidOperationException("Only .jpg, .jpeg, and .png files are allowed.");
                        }

                        // Validate size (1 MB = 1 * 1024 * 1024 bytes)
                        const long maxFileSize = 1 * 1024 * 1024;
                        if (sPhoto.Length > maxFileSize)
                        {
                            throw new InvalidOperationException("File size must not exceed 1 MB.");
                        }

                        // Prepare paths
                        string root = _host.WebRootPath;
                        string folder = Path.Combine("Images", "Student");

                        // Ensure folder exists
                        string fullFolderPath = Path.Combine(root, folder);
                        if (!Directory.Exists(fullFolderPath))
                        {
                            Directory.CreateDirectory(fullFolderPath);
                        }

                        // Create file name
                        var session = await _academicSessionManager.GetByIdAsync(student.AcademicSessionId);
                        string sessionYear = session.Name.ToString();
                        string year = sessionYear.Split('-').Last();
                        string fileName = $"S_{year}_{student.UniqueId}{fileExt}";

                        // Combine final path
                        string filePath = Path.Combine(fullFolderPath, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await sPhoto.CopyToAsync(stream);
                        }

                        // Save file name in DB
                        student.Photo = fileName;
                    }
                    if (DOBFile != null && DOBFile.Length > 0)
                    {
                        string fileExt = Path.GetExtension(DOBFile.FileName);
                        string root = _host.WebRootPath;
                        string folder = "Images/Student/";
                        string fileName = "S_" + student.DOB.ToString("ddMMyyyy") + "_" + student.ClassRoll + fileExt;
                        string pathCombine = Path.Combine(root, folder, fileName);
                        using (var stream = new FileStream(pathCombine, FileMode.Create))
                        {
                            await DOBFile.CopyToAsync(stream);
                        }
                        student.BirthCertificateImage = fileName;
                    }

                    student.EditedBy = HttpContext.Session.GetString("UserId");
                    student.EditedAt = DateTime.Now;
                    student.MACAddress = MACService.GetMAC();

                    isUpdated = await _studentManager.UpdateAsync(student);
                    if (isUpdated == true)
                    {
                        TempData["updated"] = "Updated Successfully";
                        return RedirectToList(returnUrl);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
        }
        else
        {
            ViewBag.msg = "Roll Number is already exist";
        }
        var exitStudent = _mapper.Map<StudentEditVM>(student);
        exitStudent.AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", exitStudent.AcademicSessionId).ToList();
        exitStudent.AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", exitStudent.AcademicClassId).ToList();
        exitStudent.AcademicSectionList = new SelectList(await _academicSectionManager.GetAllAsync(), "Id", "Name", exitStudent.AcademicSectionId).ToList();
        exitStudent.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name", exitStudent.BloodGroupId).ToList();
        exitStudent.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", exitStudent.GenderId).ToList();
        exitStudent.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", exitStudent.NationalityId).ToList();
        exitStudent.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", exitStudent.ReligionId).ToList();
        exitStudent.PresentDivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name", exitStudent.PresentDivisionId).ToList();
        exitStudent.PermanentDivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name", exitStudent.PermanentDivisionId).ToList();
        ViewData["UpazilaList"] = new SelectList(await _upazilaManager.GetAllAsync(), "Id", "Name", exitStudent.PresentDistrictId);

        return View(exitStudent);
    }
    #endregion Edit

    #region Delete
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "DeleteStudentsPolicy")]
    public async Task<IActionResult> Delete(int? id, string returnUrl = null)
    {
        if (id == null)
        {
            return NotFound();
        }
        int myId = (int)(id);
        var student = await _studentManager.GetByIdAsync(myId);

        if (student == null)
        {
            return NotFound();
        }
        // Carry the originating list context so Delete/Back return to the same filtered list.
        ViewData["ReturnUrl"] = returnUrl;
        return View(student);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteStudentsPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl = null)
    {
        var student = await _studentManager.GetByIdAsync(id);
        bool isSaved = await _studentManager.RemoveAsync(student);
        if (isSaved == true)
        {
            TempData["delete"] = "Deleted Successfully.";
            return RedirectToList(returnUrl);
        }
        else
        {
            return RedirectToAction("Delete", new { id, returnUrl });
        }

    }
    #endregion

    #region Profile
    [Authorize, AllowAnonymous]
    [Authorize(Policy = "ProfileStudentsPolicy")]
    public async Task<IActionResult> Profile(int id)
    {


        var user = await _userManager.GetUserAsync(User);
        if (user.UserType == 's')
        {
            if (user.ReferenceId != id)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }
        var student = await _studentManager.GetStudentByUniqueIdAsync(id.ToString())
            ?? await _studentManager.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        StudentProfileVM studentProfileVM = new StudentProfileVM();
        studentProfileVM.Student = student;

        //Attendance
        var attendance = await _studentManager.GetProfileAttendanceAsync(student.Id);
        studentProfileVM.Attendances = attendance;

        //Results
        var result = await _academicExamManager.GetSingleStudentResultDetailForProfile(student.Id);
        studentProfileVM.Results = result;
        //Payment
        var payment = await _studentPaymentManager.GetProfilePaymentAsync(student.Id);
        studentProfileVM.Payments = payment;

        //Documents
        var documents = await _studentManager.GetStudentProfileDocuments(student.Id);
        studentProfileVM.Documents = documents;
        return View(studentProfileVM);
    }
    #endregion

    #region Other's
    [Authorize(Policy = "DueAmountStudentsPolicy")]
    public async Task<double> DueAmount(int id)
    {
        Student student = await _studentManager.GetByIdAsync(id);

        LocalDate start = new LocalDate(student.AdmissionDate.Year, student.AdmissionDate.Month, student.AdmissionDate.Day);
        LocalDate end = new LocalDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        Period period = Period.Between(start, end);
        double months = period.Months;

        var allFee = await _classFeeListManager.GetAllByClassIdAsync(student.AcademicClassId);

        return months;
    }

    private async Task<int> CreateRoll(int sessionId, int ClassId, int providedRoll)
    {
        var admissionSession = await _academicSessionManager.GetByIdAsync(sessionId);
        var admissionClass = await _academicClassManager.GetByIdAsync(ClassId);
        //var totalStudent = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(sessionId, ClassId);
        string year = admissionSession.Name.Substring(admissionSession.Name.Length - 2);
        string aClass = admissionClass.ClassSerial.ToString("d2");
        //string stuCount = (totalStudent.Count+1).ToString("d3");
        string cRoll = providedRoll.ToString("d3");
        int roll = Convert.ToInt32(year + aClass + cRoll);
        return roll;
    }

    public async Task<IActionResult> StudentReport()
    {
        var students = await _studentManager.GetAllAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int studentId, string operationDate, bool studentStatus, string returnUrl = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(operationDate))
            {
                TempData["statusError"] = "Please select an operation date.";
                return RedirectToAction("Edit", new { id = studentId, returnUrl });
            }

            DateTime opDate = Convert.ToDateTime(operationDate);
            Student existingStudent = await _studentManager.GetByIdAsync(studentId);

            if (existingStudent == null)
            {
                TempData["statusError"] = "Student not found.";
                return RedirectToAction("Edit", new { id = studentId, returnUrl });
            }

            if (existingStudent.AdmissionDate.Date > opDate.Date)
            {
                TempData["statusError"] = "Operation date cannot be before the admission date.";
                return RedirectToAction("Edit", new { id = studentId, returnUrl });
            }

            existingStudent.Status = studentStatus;
            existingStudent.EditedBy = HttpContext.Session.GetString("UserId");
            existingStudent.EditedAt = DateTime.Now;
            existingStudent.MACAddress = MACService.GetMAC();

            bool isUpdated = await _studentManager.UpdateAsync(existingStudent);

            if (!isUpdated)
            {
                TempData["statusError"] = "Failed to update student status. Please try again.";
                return RedirectToAction("Edit", new { id = studentId, returnUrl });
            }

            StudentActivateHist hist = new()
            {
                StudentId = studentId,
                IsActive = studentStatus,
                ActionDateTime = opDate,
                CreatedBy = HttpContext.Session.GetString("UserId"),
                CreatedAt = DateTime.Now,
                MACAddress = MACService.GetMAC(),
                LastAction = "Add"
            };

            bool isHistAdded = await _studentActivateHistManager.AddAsync(hist);

            if (!isHistAdded)
            {
                TempData["statusError"] = "Status updated but history record failed. Please contact support.";
                return RedirectToAction("Edit", new { id = studentId, returnUrl });
            }

            string statusLabel = studentStatus ? "activated" : "deactivated";
            TempData["statusSuccess"] = $"{existingStudent.Name} {statusLabel} successfully.";

            var user = await _userManager.FindByIdAsync(HttpContext.Session.GetString("UserId"));
            await _appLogger.InfoAsync($"{existingStudent.Name} status changed to {studentStatus} by {user?.Email}");
        }
        catch (Exception ex)
        {
            await _appLogger.ErrorAsync($"Student status change failed", ex.Message);
            TempData["statusError"] = $"Operation failed: {ex.Message}";
        }

        return RedirectToAction("Edit", new { id = studentId, returnUrl });
    }

    public int StudentCount()
    {
        return 5;
    }

    private async Task<double> GetTotalDue(int stuId)
    {
        Student st = await _studentManager.GetByIdAsync(stuId);
        int admissionYear = st.AdmissionDate.Year;
        int currentYear = DateTime.Now.Year;

        int admissionMonth = admissionYear < currentYear ? 1 : st.AdmissionDate.Month;
        double monthlyFee = await GetFeeAsync(st.AcademicClassId, 1, st.AcademicSessionId); //1=monthlyfee, 2=admissionFee, 3=ExamFee
        double admissionFee = await GetFeeAsync(st.AcademicClassId, 2, st.AcademicSessionId); //1=monthlyfee, 2=admissionFee, 3=ExamFee
        double totalAmount = ((12 - (admissionMonth - 1)) * monthlyFee) + admissionFee;
        double totalPaid = await GetTotalPaid(st.Id);
        double totalDue = totalAmount - totalPaid;
        totalDue = totalDue >= 0 ? totalDue : 0;

        return totalDue;
    }

    public async Task<double> GetCurrntDue(int studId)
    {
        double currentDue = 0.00;
        int currentMonth = DateTime.Now.Month;

        Student st = await _studentManager.GetByIdAsync(studId);
        try
        {
            if (st == null)
            {
                return 0;
            }
            double totalCurrentPayable = 0;
            double totalCurrentPaid = 0;
            double admissionOrSessionFee = 0;
            int feeHeadValue = 0;
            double cMonthlyFee = 0;
            double othersFee = 0;


            //0     = admission fee
            //1-12  = monthly fee
            //13    = session fee
            //14- >   other's fee

            //admission or session fee calculation
            feeHeadValue = st.AdmissionDate.Year < DateTime.Now.Year ? 13 : 0;
            var admissionOrSessionFeeFromAllocation = await _studentFeeAllocationManager.GetStudentFeeAllocationByUniqueIdFeeHeadId(st.UniqueId, feeHeadValue);
            admissionOrSessionFee = await _classFeeListManager.GetFeeAmountByFeeListSlAsync(st.UniqueId, feeHeadValue);
            if (admissionOrSessionFeeFromAllocation != null)
            {
                admissionOrSessionFee = admissionOrSessionFeeFromAllocation.AllocatedAmount;
            }

            //monthly fee calculation

            var feeHeads = await _studentFeeHeadManager.GetAllAsync();
            for (
                int i = st.AdmissionDate.Month; i <= DateTime.Now.Month; i++)
            {
                feeHeadValue = i;
                var feeHead = feeHeads.FirstOrDefault(s => s.SL == feeHeadValue);
                var monthlyFeeAllocation = await _studentFeeAllocationManager.GetStudentFeeAllocationByUniqueIdFeeHeadId(st.UniqueId, feeHead.Id);
                if (monthlyFeeAllocation != null)
                {
                    cMonthlyFee += monthlyFeeAllocation.AllocatedAmount;
                }
                else
                {
                    cMonthlyFee += await _classFeeListManager.GetFeeAmountByFeeListSlAsync(st.UniqueId, feeHeadValue);
                }
            }
            //others fee calculation
            var othersFeeList = await _classFeeListManager.GetByClassIdSessionIdStudentIdAsync(st.AcademicClassId, st.AcademicSessionId, st.Id);
            if (othersFeeList != null)
            {
                foreach (var item in othersFeeList)
                {
                    if (item.SL > 13)
                    {
                        var othersFeeFromAllocation = await _studentFeeAllocationManager.GetStudentFeeAllocationByUniqueIdFeeHeadId(st.UniqueId, item.StudentFeeHeadId);
                        if (othersFeeFromAllocation != null)
                        {
                            othersFee += othersFeeFromAllocation.AllocatedAmount;
                        }
                        else
                        {
                            othersFee += item.Amount;
                        }
                    }
                }
            }
            totalCurrentPayable = admissionOrSessionFee + cMonthlyFee + othersFee;
            totalCurrentPaid = await GetTotalPaid(studId);
            currentDue = totalCurrentPayable - totalCurrentPaid;
        }
        catch (Exception)
        {
            throw;
        }

        return currentDue;
    }
    private async Task<double> GetCurrentDue(int stuId)
    {
        double currentDue = 0;
        Student st = await _studentManager.GetByIdAsync(stuId);
        try
        {
            List<ClassFeeList> feeLists = await _classFeeListManager.GetAllByStudentId(st.Id);
        }
        catch (Exception)
        {

            throw;
        }
        return currentDue;
    }
    private async Task<double> GetFeeAsync(int aClassId, int feeHeadId, int sessionId)
    {
        ClassFeeList classFeeList = await _classFeeListManager.GetByClassIdAndFeeHeadIdAsync(aClassId, feeHeadId, sessionId);
        if (classFeeList != null)
        {
            return classFeeList.Amount;
        }
        return 0;
    }
    private async Task<double> GetTotalPaid(int stuId)
    {
        List<StudentPayment> studentPayments = (List<StudentPayment>)await _studentPaymentManager.GetAllByStudentIdAsync(stuId);
        double paidAmount = studentPayments.Sum(s => s.TotalPayment);
        return paidAmount;
    }

    private async Task<string> GenerateUniquId(Student student)
    {
        string uniqueId = string.Empty;
        uniqueId = student.DOB.ToString("yyMMdd");
        AcademicSession academicSession = await _academicSessionManager.GetByIdAsync(student.AcademicSessionId);
        uniqueId += academicSession.Name.Substring(academicSession.Name.Length - 1, 1);
        uniqueId += student.ClassRoll.ToString().Substring(student.ClassRoll.ToString().Length - 2, 2);

        // A birth year ending 00-09 puts a leading zero on "yyMMdd" (e.g. DOB 2005 -> "05...").
        // That leading zero doesn't change the ID's numeric value, but a UniqueId is also typed
        // into the attendance machine as a PIN, and used to have to match by exact string on some
        // screens - normalize it away here so a leading zero can never reach the database again.
        return AttendancePinMatcher.Normalize(uniqueId) ?? uniqueId;
    }
    #region APIs //All of the students related APIs will placed here

    [Route("api/Students/getbyclasswithsessionId")]
    [HttpPost]
    public async Task<JsonResult> GetStudentsByClassSessionAsync(int academicClassId, int? academicSessionId)
    {
        try
        {
            if (academicSessionId == null)
            {
                AcademicSession academicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
                academicSessionId = academicSession.Id;
            }
            var studentList = await _studentManager.GetStudentsByClassIdAndSessionIdAsync((int)academicSessionId, academicClassId);
            return Json(studentList.OrderBy(s => s.ClassRoll));
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<JsonResult> GetStudentsBySessionIdClassIdSectionId(int? academicSessionId, int academicClassId, int? academicSectionId)
    {
        if (academicSessionId == null || academicSessionId <= 0)
        {
            AcademicSession currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
            academicSessionId = currentSession.Id;
        }
        if (academicSectionId == null || academicSectionId <= 0)
        {
            academicSectionId = 0;
        }
        var studets = await _studentManager.GetStudentsByClassSessionSectionAsync((int)academicSessionId, academicClassId, (int)academicSectionId);
        return Json(studets.OrderBy(s => s.ClassRoll));
    }
    #endregion APIs

    [HttpGet]
    [Route("api/Students/GetUniqueIdByStudentId")]
    public async Task<JsonResult> GetUniqueIdByStudentId(string id)
    {
        int stuId = Convert.ToInt32(id);
        var student = await _studentManager.GetByIdAsync(stuId);
        if (student != null)
        {
            return Json(student.UniqueId);
        }
        return null;
    }

    // GET: Students/Export - robust, column-selectable export of the (filtered) student list.
    // The caller passes the same filters shown on the list plus the chosen columns and format;
    // the selectable columns and the produced file both come from StudentExportColumns so they
    // can never drift apart.
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexStudentsPolicy")]
    public async Task<IActionResult> Export(int? academicSessionId, int? academicClassId, int? academicSectionId,
        string aStatus, string aCategory, string searchString, string[] columns, string format = "csv")
    {
        // Match the list view's default: no session chosen means the current session.
        if (academicSessionId == null)
        {
            var currentSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
            academicSessionId = currentSession?.Id;
        }

        var students = await _studentManager.GetStudentsForExportAsync(
            academicSessionId ?? 0, academicClassId ?? 0, academicSectionId ?? 0);

        // Category and status are not part of the query filter, so apply them here (as the list does).
        if (aCategory == "residential")
            students = students.Where(s => s.IsResidential).ToList();
        else if (aCategory == "nonResidential")
            students = students.Where(s => !s.IsResidential).ToList();

        if (aStatus == "0" || aStatus == "1")
        {
            bool isActive = aStatus == "1";
            students = students.Where(s => s.Status == isActive).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var term = searchString.Trim();
            var lower = term.ToLowerInvariant();
            students = students.Where(s =>
                (s.Name?.ToLowerInvariant().Contains(lower) ?? false)
                || (s.NameBangla?.Contains(term) ?? false)
                || s.ClassRoll.ToString().Contains(term)
                || (s.UniqueId?.Contains(term) ?? false)
                || (s.PhoneNo?.Contains(term) ?? false)
                || (s.GuardianPhone?.Contains(term) ?? false)).ToList();
        }

        var cols = StudentExportColumns.Resolve(columns);
        var stamp = DateTime.Today.ToString("yyMMdd");

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var institute = await _instituteManager.GetFirstOrDefaultAsync();
            var headers = cols.Select(c => c.Header).ToList();
            var rows = students
                .Select(s => (IReadOnlyList<string>)cols.Select(c => c.Value(s) ?? string.Empty).ToList())
                .ToList();
            var subtitle = $"Student List ({students.Count}) — {DateTime.Now:dd MMM yyyy}";
            var pdf = new TabularPdfBuilder(institute?.Name ?? "Student List", subtitle, headers, rows).GeneratePdf();
            return File(pdf, "application/pdf", $"{stamp}_Student_Export.pdf");
        }

        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Students");
            for (int c = 0; c < cols.Count; c++)
                ws.Cell(1, c + 1).Value = cols[c].Header;
            ws.Row(1).Style.Font.Bold = true;

            int r = 2;
            foreach (var s in students)
            {
                for (int c = 0; c < cols.Count; c++)
                    ws.Cell(r, c + 1).Value = cols[c].Value(s) ?? string.Empty;
                r++;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{stamp}_Student_Export.xlsx");
        }

        // CSV (default): RFC-4180 quoting + UTF-8 BOM so Excel reads Bangla text correctly.
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", cols.Select(c => CsvField(c.Header))));
        foreach (var s in students)
            sb.AppendLine(string.Join(",", cols.Select(c => CsvField(c.Value(s)))));

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"{stamp}_Student_Export.csv");
    }

    /// <summary>Quotes a CSV field per RFC 4180 (wrap in quotes, double any embedded quote).</summary>
    private static string CsvField(string value)
    {
        value ??= string.Empty;
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    /// <summary>
    /// Returns to the originating student list, preserving its filters, search, sort and
    /// page as captured in <paramref name="returnUrl"/>. Falls back to a default list when
    /// the URL is missing or not a local application URL (open-redirect guard).
    /// </summary>
    private IActionResult RedirectToList(string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    private PersonalDetails GetPersonalData(Student student)
    {
        var personalDetails = new PersonalDetails()
        {
            StudentName = student.Name,
            Gender = student.Gender.Name,
            DateOfBirth = student.DOB,
            Nationality = student.Nationality.Name,
            Religion = student.Religion.Name,
            BloodGroup = student.BloodGroup.Name,
            Phone = student.PhoneNo,
            Email = student.Email,
            FatherName = student.FatherName,
            MotherName = student.MotherName,
            GuardianPhone = student.GuardianPhone,
            PresentAddress = $"{student.PresentAddressArea}, {student.PresentUpazila.Name}, {student.PresentDistrict.Name}, {student.PresentDivision.Name}",
            PermanentAddress = $"{student.PermanentAddressArea}, {student.PermanentUpazila.Name}, {student.PermanentDistrict.Name}, {student.PermanentDivision.Name}"
        };
        return personalDetails;
    }
    #endregion Other's

    #region Application
    // The old online-application prototype (AppliedStudent) exposed applicants' details to anyone
    // with the URL. It is replaced by the Online Admission module; old links land on its notice board.
    [AllowAnonymous]
    public IActionResult Application() => RedirectToAction("Index", "Admission");
    #endregion Application

    #region API
    [HttpGet]
    public async Task<JsonResult> GetAllStudentBySectionId(int academicSectionId, int? classId = null, int? sessionId = null, bool? isResidential = null)
    {
        var students = await _studentManager.GetStudentsWithSectionBySectionIdAsync(academicSectionId, classId, sessionId, isResidential);
        return new JsonResult(students.OrderBy(s => s.ClassRoll));
    }

    [HttpGet]
    public async Task<JsonResult> GetAllStudentsByClassSessionResidential(int classId, int sessionId, bool isResidential)
    {
        var students = await _studentManager.GetStudentsWithSectionByClassSessionResidentialAsync(classId, sessionId, isResidential);
        return new JsonResult(students.OrderBy(s => s.ClassRoll));
    }

    [HttpGet]
    public async Task<JsonResult> CheckRollExist(int roll, int sessionId, int classId)
    {
        bool isExist = false;
        var newRoll = await CreateRoll(sessionId, classId, roll);
        var existingStudent = await _studentManager.GetStudentByClassRollAsync(newRoll);
        if (existingStudent != null)
        {
            isExist = true;
        }
        return Json(isExist);
    }
    #endregion API
}