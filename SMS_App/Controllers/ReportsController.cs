using AspNetCore.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Reporting.NETCore;
using SchoolManagementSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SMS.BLL.Contracts;
using SMS.BLL.Contracts.Reports;
using SMS.Entities;
using SMS.Entities.RptModels.AttendanceVM;
using SMS.Entities.RptModels.StudentPayment;
using SMS.Entities.Utilities;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.Others;
using SMS_App.ViewModels.AttendanceVM;
using SMS_App.ViewModels.ReportVM;
using QuestPDF.Fluent;
using SMS_App.Utilities.Reports;
using SMS_App.ViewModels.ReportVM.MarkSheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using LocalReport = Microsoft.Reporting.NETCore.LocalReport;

namespace SMS_App.Controllers;

[Authorize]
public class ReportsController : Controller
{
    #region properties
    private readonly IWebHostEnvironment _host;
    private readonly IStudentManager _studentManager;
    private readonly IReportManager _reportManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IOffDayManager _OffDayManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IAcademicExamDetailsManager _academicExamDetailsManager;
    private readonly IAcademicExamManager _academicExamManager;
    private readonly IGradingTableManager _gradingTableManager;
    private readonly IAcademicExamGroupManager _academicExamGroupManager;
    private readonly IStudentFeeHeadManager _studentFeeHeadManager;
    private readonly IExamResultManager _examResultManager;
    private readonly IAppLogger _appLogger;

    #endregion properties

    #region Constructor
    public ReportsController(IWebHostEnvironment host, IStudentManager studentManager, IReportManager reportManager, IAcademicClassManager academicClassManager, IAttendanceMachineManager attendanceMachineManager, IOffDayManager dayManager, IAcademicSessionManager academicSessionManager, IAcademicSectionManager academicSectionManager, IInstituteManager instituteManager, IAcademicExamDetailsManager academicExamDetailsManager, IAcademicExamManager academicExamManager, IGradingTableManager gradingTableManager, IAcademicExamGroupManager academicExamGroupManager, IStudentFeeHeadManager studentFeeHeadManager, IExamResultManager examResultManager, IAppLogger appLogger)
    {
        _host = host;
        _studentManager = studentManager;
        _reportManager = reportManager;
        _academicClassManager = academicClassManager;
        _attendanceMachineManager = attendanceMachineManager;
        _OffDayManager = dayManager;
        _academicSessionManager = academicSessionManager;
        _academicSectionManager = academicSectionManager;
        _instituteManager = instituteManager;
        _academicExamDetailsManager = academicExamDetailsManager;
        _academicExamManager = academicExamManager;
        _gradingTableManager = gradingTableManager;
        _academicExamGroupManager = academicExamGroupManager;
        _studentFeeHeadManager = studentFeeHeadManager;
        _examResultManager = examResultManager;
        _appLogger = appLogger;
    }
    #endregion Constructor

    #region Student List Report
    [Authorize(Policy = "StudentsReportsPolicy")]
    public async Task<IActionResult> StudentsReport()
    {
        Rpt_Student_VM rpt_Student_VM = new()
        {
            AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList(),
            AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name").ToList()
        };
        return View(rpt_Student_VM);
    }

    [Authorize(Policy = "StudentsReportsPolicy")]
    public async Task<IActionResult> StudentsReportExport(string reportType, string fileName, int? academicClassId, int? academicSectionId)
    {
        AcademicSession aSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        if (aSession == null)
        {
            return new JsonResult("Current Session not set");
        }

        var studens = await _reportManager.GetStudentsInfo(aSession.Id, academicClassId, academicSectionId);
        if (studens == null || studens.Count <= 0)
        {
            return new JsonResult("Sorry! Students Not Found.");
        }
        Institute institute = await _instituteManager.GetByIdAsync(1);
        if (institute == null)
        {
            return new JsonResult("Sorry! Institute Information Not Found");
        }

        string logoBase64 = "";
        if (!string.IsNullOrEmpty(institute.Logo))
        {
            var imagePath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);
            if (System.IO.File.Exists(imagePath))
                logoBase64 = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(imagePath));
        }

        var doc = new StudentListPdfBuilder(institute, logoBase64, studens);
        var pdf = doc.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "student_list" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        return File(pdf, MediaTypeNames.Application.Octet, $"{fileName}.pdf");
    }

    public async Task<IActionResult> StudentDynamicReport()
    {
        var columnMap = new Dictionary<string, string>
        {
            { "Name", "Name" },
            { "ClassRoll", "Class Roll" },
            { "NameBangla", "Name (Bangla)" },
            { "AcademicSection", "Section" },
            { "Father", "Father's Name" },
            { "Mother", "Mother's Name" },
            { "AdmissionDate", "Admission Date" },
            { "Email", "Email" },
            { "Gender", "Gender" },
            { "PhoneNo", "Phone" },
            { "GuardianPhone", "Guardian Phone" },
            { "DOB", "Date of Birth" },
            { "Religion", "Religion" },
            { "BloodGroup", "Blood Group" },
            { "PresentAddress", "Present Address" },
            { "PermanentAddress", "Permanent Address" },
            { "PreviousSchool", "Previous School" },
            { "IsResidential", "Is Residential" },
            { "SMSService", "SMS Service?" },
            { "Status", "Current Status" },
        };

        var reportModel = new RptStudentDynamicReportVM
        {
            AcademicClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList(),
            AcademicSessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name").ToList(),

            // Set the ColumnMap dictionary
            ColumnMap = columnMap
        };

        return View(reportModel);
    }

    public async Task<IActionResult> StudentDynamicReportExport(
    string reportType,
    string fileName,
    int academicSessionId,
    int academicClassId,
    int academicSectionId,
    [FromQuery(Name = "columns")] List<string> columns)
    {
        // 1️ Validate input columns
        if (columns == null || !columns.Any())
            return BadRequest("No columns selected.");

        // 2️ Fetch students
        var students = await _studentManager.GetStudentsByClassSessionSectionAsync(
            academicSessionId, academicClassId, academicSectionId);
        if (students == null || !students.Any())
            return Json("Sorry! Students Not Found.");

        // 3️ Fetch institute info
        var institute = await _instituteManager.GetByIdAsync(1);
        if (institute == null)
            return Json("Sorry! Institute Information Not Found");

        // 4️ Define column mappings
        var columnMappings = GetColumnMappings();

        // 5️ Ensure mandatory columns are included
        var mandatoryColumns = new List<string> { "Name", "ClassRoll", "AcademicClass", "AcademicSession" };
        foreach (var col in mandatoryColumns)
            if (!columns.Contains(col)) columns.Insert(0, col);

        // 6️ Map dynamic columns to RDLC fixed columns
        var dynamicColumnMap = MapDynamicColumns(columns, mandatoryColumns);

        // 7️ Prepare DataTable
        var reportColumns = GetReportColumns();
        var dataTable = BuildDataTable(students, reportColumns, columnMappings, dynamicColumnMap);

        // 8️⃣ Prepare report path (cross-platform)
        var reportPath = Path.Combine(
            _host.WebRootPath,
            "Reports",
            "Academic",
            "Students",
            "RptStudentDynamicReport.rdlc"
        );

        if (!System.IO.File.Exists(reportPath))
            throw new FileNotFoundException("RDLC file not found at: " + reportPath);

        string logoBase64Dynamic = await GetBase64LogoAsync(institute.Logo);
        var cleanLogo = StripDataUriPrefix(logoBase64Dynamic);

        var colLabels = columnMappings.ToDictionary(k => k.Key, v => v.Value.Label);
        var dynDoc = new StudentDynamicReportPdfBuilder(institute, cleanLogo, dataTable, columns, colLabels);
        var finalReport = dynDoc.GeneratePdf();

        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = fileName + "_" + DateTime.Now.ToString("yyMMddhhmm");
            return File(finalReport, MediaTypeNames.Application.Octet, $"{fileName}.pdf");
        }
        return File(finalReport, "application/pdf");
    }

    // ==================== Helper Methods ====================

    private Dictionary<string, (string Label, Func<Student, object> Selector)> GetColumnMappings()
    {
        return new Dictionary<string, (string, Func<Student, object>)>
        {
            { "Name", ("Name", s => s.Name) },
            { "ClassRoll", ("Class Roll", s => s.ClassRoll) },
            { "NameBangla", ("Name (Bangla)", s => s.NameBangla) },
            { "AcademicClass", ("Class", s => s.AcademicClass?.Name) },
            { "AcademicSection", ("Section", s => s.AcademicSection?.Name) },
            { "Father", ("Father's Name", s => s.FatherName) },
            { "Mother", ("Mother's Name", s => s.MotherName) },
            { "AdmissionDate", ("Admission Date", s => s.AdmissionDate.ToString("dd-MM-yyyy")) },
            { "Email", ("Email", s => s.Email) },
            { "Gender", ("Gender", s => s.Gender?.Name) },
            { "PhoneNo", ("Phone", s => s.PhoneNo) },
            { "GuardianPhone", ("Guardian Phone", s => s.GuardianPhone) },
            { "DOB", ("Date of Birth", s => s.DOB.ToString("dd-MM-yyyy")) },
            { "Religion", ("Religion", s => s.Religion?.Name) },
            { "BloodGroup", ("Blood Group", s => s.BloodGroup?.Name) },
            { "PresentAddress", ("Present Address", s => $"{s.PresentAddressArea}, {s.PresentAddressPO}, {s.PresentUpazila?.Name}, {s.PresentDistrict?.Name}, {s.PresentDivision?.Name}") },
            { "PermanentAddress", ("Permanent Address", s => $"{s.PermanentAddressArea}, {s.PermanentAddressPO}, {s.PermanentUpazila?.Name}, {s.PermanentDistrict?.Name}, {s.PermanentDivision?.Name}") },
            { "AcademicSession", ("Session", s => s.AcademicSession?.Name) },
            { "PreviousSchool", ("Previous School", s => s.PreviousSchool) },
            { "IsResidential", ("Residential Type", s => s.IsResidential ? "Residential" : "Non-Residential") },
            { "SMSService", ("SMS Service", s => s.SMSService ? "Yes" : "No SMS") },
            { "Status", ("Status", s => s.Status ? "Active" : "Inactive") }
        };
    }

    private List<string> GetReportColumns()
    {
        return new List<string>
        {
            "ClassRoll","Name","Column3","Column4","Column5","Column6","Column7","Column8","Column9","AcademicClass","AcademicSession","ColumnExtra3","ColumnExtra4","ColumnExtra5"
        };
    }

    private Dictionary<string, string> MapDynamicColumns(List<string> columns, List<string> mandatoryColumns)
    {
        var map = new Dictionary<string, string>();
        var dynammicColumns = columns.Except(mandatoryColumns).ToList();
        for (int i = 1; i < 8; i++)
        {
            string fixedCol = "Column" + (i+2);
            map[fixedCol] =dynammicColumns.Count>=i?dynammicColumns[i-1]: fixedCol;
            
        }
        return map;
    }

    private DataTable BuildDataTable(
        IEnumerable<Student> students,
        List<string> reportColumns,
        Dictionary<string, (string Label, Func<Student, object> Selector)> columnMappings,
        Dictionary<string, string> dynamicColumnMap)
    {
        var dt = new DataTable("StudentDynamicReportDataset");
        reportColumns.ForEach(c => dt.Columns.Add(c));

        foreach (var student in students.OrderBy(s => s.AcademicClassId).ThenBy(s => s.ClassRoll))
        {
            var row = dt.NewRow();

            // Mandatory fields
            row["ClassRoll"] = student.ClassRoll;
            row["Name"] = student.Name;
            row["AcademicClass"] = student.AcademicClass?.Name ?? "";
            row["AcademicSession"] = student.AcademicSession?.Name ?? "";

            // Dynamic mapped fields
            foreach (var map in dynamicColumnMap)
            {
                if (map.Value == "Name" || map.Value == "ClassRoll") continue;

                if (columnMappings.TryGetValue(map.Value, out var colInfo))
                {
                    if (map.Value == map.Key)
                    {
                        row[map.Key] = "";
                    }
                    else
                    {
                        row[map.Key] = colInfo.Selector(student) ?? string.Empty;
                    }
                }                    
            }

            dt.Rows.Add(row);
        }

        return dt;
    }

    private async Task<string> GetBase64LogoAsync(string logoFileName)
    {
        var path = Path.Combine(_host.WebRootPath, "Images", "Institute", logoFileName);

        if (!System.IO.File.Exists(path))
            return string.Empty;

        try
        {
            // Read the image bytes directly (cross-platform)
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(path);

            // Convert to Base64 and prefix for RDLC external image
            return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<ReportParameter[]> GetReportParameters(
        Institute institute,
        Dictionary<string, string> dynamicColumnMap,
        Dictionary<string, (string Label, Func<Student, object> Selector)> columnMappings)
    {

        var dynamicParams = dynamicColumnMap
            .Select((map, index) =>
                new ReportParameter(
                    $"Column{index + 3}Header",
                    map.Value == map.Key
                        ? " "
                        : (columnMappings.TryGetValue(map.Value, out var val) ? val.Label : map.Value)
                )
            )
            .ToArray();


        var parameters = new[]
        {
        new ReportParameter("InstituteName", institute.Name),
        new ReportParameter("ReportName", "Student List"),
        new ReportParameter("Address", institute.Address),
        new ReportParameter("EIIN", institute.EIIN),
        new ReportParameter("Logo", await GetBase64LogoAsync(institute.Logo))
    }.Concat(dynamicParams).ToArray();

        return parameters;
    }

    #endregion Student List Report

    #region Attendance Reports
    [Authorize(Policy = "DailyAttendanceReportsPolicy")]
    public async Task<IActionResult> DailyAttendanceReport()
    {
        ViewData["AcademicClass"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList();
        return View();
    }

    [Authorize(Policy = "DailyAttendanceReportsPolicy")]
    public async Task<IActionResult> DailyAttendaceReportExport(string reportType, string fromDate, string academicClassId, string academicSectionId, string attendanceType, string fileName, string attendanceFor, string attendanceCategory, string sms)
    {

        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
        {
            return new JsonResult("Institute Information not found!");
        }

        var path = Path.Combine(_host.WebRootPath, "Reports", "Attendance", "Rpt_Daily_Attendance.rdlc");


        var reportName = "Students Daily Attendance Report (Check In)";
        if (attendanceCategory == "In")
        {
            reportName = "Students Daily Attendance Report (Check In)";
        }
        else
        {
            reportName = "Students Daily Attendance Report (Check Out)";
            path = Path.Combine(
                _host.WebRootPath,
                "Reports",
                "Attendance",
                "Rpt_Daily_Attendance_CheckOut.rdlc"
            );
        }

        // Cross-platform image path

        string defaultInstituteLogo = "smslogo.png";
        string logoFileName = string.IsNullOrWhiteSpace(institute.Logo)
            ? defaultInstituteLogo
            : institute.Logo;
        string imageParam = await LoadInstituteLogoAsync(logoFileName);
        
        attendanceFor = attendanceFor == "s" ? "student" : "employee";
        AcademicSession academicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var reportData = new List<RptDailyAttendaceVM>();
        reportData = attendanceCategory == "In"
            ? await _reportManager.GetDailyAttendanceReport(fromDate, academicClassId, academicSectionId, attendanceType, academicSession.Id.ToString(), attendanceFor)
            : await _reportManager.GetDailyAttendanceReportCheckOut(fromDate, academicClassId, academicSectionId, attendanceType, academicSession.Id.ToString(), attendanceFor);

        if (attendanceFor == "employee")
        {
            path = Path.Combine(_host.WebRootPath, "Reports", "Attendance", "Rpt_Daily_Attendance_Employee.rdlc");
            reportName = "Employees Daily Attendance Report";
        }
        if (reportData.Count > 0)
        {
            var allActiveStudents = await _studentManager.GetCurrentStudentListAsync(null, null);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            foreach (var item in reportData)
            {
                // Every field below is nullable in the database. Before this
                // guard a single student with no phone number threw a
                // NullReferenceException and the whole report failed to render.
                var cardNo = item.CardNo?.Trim() ?? string.Empty;
                var isResidential = allActiveStudents.FirstOrDefault(s =>
                        s.ClassRoll.ToString() == cardNo || AttendancePinMatcher.Matches(s.UniqueId, cardNo))?.IsResidential;

                item.Name = textInfo.ToTitleCase((item.Name ?? string.Empty).ToLower());
                if (isResidential == true) item.Name += " (R)";

                item.Phone = (item.Phone ?? string.Empty).PadLeft(11, '0');
                item.GuardianPhone = (item.GuardianPhone ?? string.Empty).PadLeft(11, '0');
            }
        }
        if (!string.IsNullOrEmpty(sms))
        {
            if (sms == "sms") reportData = reportData.Where(s => s.SMSSent != "Not Sent").ToList();
            else if (sms == "no") reportData = reportData.Where(s => s.SMSSent == "Not Sent").ToList();
        }

        bool isEmp = attendanceFor == "employee";
        var dailyDoc = new DailyAttendancePdfBuilder(
            institute, StripDataUriPrefix(imageParam), reportData, reportName, fromDate, isEmp);
        var pdf = dailyDoc.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "attendance" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}.pdf\"";
        return File(pdf, MediaTypeNames.Application.Pdf);

    }

    //Monthly Attendance Report
    [Authorize(Policy = "AttendanceReportsPolicy")]
    public async Task<IActionResult> AttendanceReport()
    {
        ViewBag.AcademicClasslist = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList();
        string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
        List<SelectListItem> monthsList = new();
        for (int i = 1; i <= 12; i++)
        {
            SelectListItem item = new()
            {
                Text = monthNames[i - 1],
                Value = i.ToString()
            };
            monthsList.Add(item);
        }
        ViewBag.Monthlist = new SelectList(monthsList, "Value", "Text");

        MonthlyAttendanceFullClass monthlyAttendanceFullClass = new();
        return View(monthlyAttendanceFullClass);
    }

    //Monthly Attendance Report
    [HttpPost]
    [Authorize(Policy = "AttendanceReportsPolicy")]
    public async Task<IActionResult> AttendanceReport(int monthId, int classId)
    {
        ViewBag.AcademicClasslist = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classId).ToList();

        string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
        List<SelectListItem> monthsList = new();
        for (int i = 1; i <= 12; i++)
        {
            SelectListItem item = new()
            {
                Text = monthNames[i - 1],
                Value = i.ToString()
            };
            monthsList.Add(item);
        }
        ViewBag.Monthlist = new SelectList(monthsList, "Value", "Text", monthId);

        string StartDate = string.Empty;
        string EndDate = string.Empty;
        string monthName = string.Empty;
        string className = string.Empty;
        // Create a new DateTime object for the first day of the month
        DateTime firstDateOfMonth = new(DateTime.Now.Year, monthId, 1);
        ViewBag.StartDate = firstDateOfMonth.ToString("dd-MMM-yyyy");
        // Get the last day of the month by adding one month to the first day and subtracting one day
        DateTime lastDateOfMonth = firstDateOfMonth.AddMonths(1).AddDays(-1);
        StartDate = firstDateOfMonth.ToString("yyyy-MM-dd");
        EndDate = lastDateOfMonth.ToString("yyyy-MM-dd");

        //Get Month name by month number 
        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        monthName = dfi.MonthNames[monthId - 1];

        //Get Class Name by ClasssId
        AcademicClass academicClass = await _academicClassManager.GetByIdAsync(classId);
        className = academicClass.Name;


        MonthlyAttendanceFullClass monthlyAttendanceFullClass = new()
        {
            MonthName = monthName,
            ClassName = className,
            MothlyAttendanceFullClassDetailses = new List<MonthlyAttendanceFullClassDetails>()
        };

        int monthDays = ViewBag.daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, monthId);

        var attendanceList = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(StartDate, EndDate);
        AcademicSession academicSession = await _academicSessionManager.GetCurrentAcademicSessionAsync();
        var studentList = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(academicSession.Id, classId);

        List<DateTime> monthlyHolidays = await _OffDayManager.GetMonthlyHolidaysAsync(firstDateOfMonth.ToString("MMyyyy"));
        ViewBag.monthlyHolidays = monthlyHolidays;
        foreach (Student student in studentList.Where(s => s.Status == true))
        {
            var studentUniqueId = student.UniqueId?.Trim();
            var myAttendances = attendanceList.Where(t => CardNumbersMatch(t.CardNo, studentUniqueId)).ToList();

            IDictionary<int, bool> daysPresents = new Dictionary<int, bool>();
            MonthlyAttendanceFullClassDetails monthlyAttendanceFullClassDetails = new()
            {
                StudentName = student.Name,
                Roll = student.ClassRoll,
                isPresents = daysPresents
            };


            for (int i = 1; i <= monthDays; i++)
            {
                var currentDate = firstDateOfMonth.AddDays(i - 1).ToString("ddMMyyyy");
                var attended = myAttendances.Where(a => a.PunchDatetime.ToString("ddMMyyyy") == currentDate).Any();
                if (attended)
                {
                    daysPresents.Add(i, true);
                }
                else
                {
                    daysPresents.Add(i, false);
                }
                monthlyAttendanceFullClassDetails.isPresents = daysPresents;
            }
            int total = daysPresents.Where(m => m.Value == true).Count();
            monthlyAttendanceFullClassDetails.Total = total;
            monthlyAttendanceFullClassDetails.CountPercentage = (total * 100) / (monthDays - monthlyHolidays.Count);
            monthlyAttendanceFullClassDetails.Holidays = monthlyHolidays;
            monthlyAttendanceFullClass.MothlyAttendanceFullClassDetailses.Add(monthlyAttendanceFullClassDetails);
        }

        ViewBag.studentList = studentList;
        return View(monthlyAttendanceFullClass);
    }

    // CardNo (raw punch) and Student.UniqueId are string columns, and inconsistent about leading
    // zeros ("0123" vs "123"). Delegates to the same normalizer the dashboard's absent-list uses
    // (AttendanceMachineRepository) so the two can't independently drift the way they did before.
    private static bool CardNumbersMatch(string cardNo, string uniqueId) => AttendancePinMatcher.Matches(cardNo, uniqueId);

    public IActionResult MonthlyAttendanceReport()
    {
        return View();
    }

    public async Task<IActionResult> MonthlyAttendanceReportExport()
    {
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
        {
            return new JsonResult("Institute Information not found!");
        }

        var students = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(5, 1);
        string mediaType = "application/pdf";
        var path = Path.Combine(_host.WebRootPath, "Reports", "Attendance", "Rpt_Monthly_Attendance_Report.rdlc");

        using var report = new LocalReport();

        // 1️⃣ Cross-platform image path
        var imagePath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);

        // 2️⃣ Check if file exists
        string imageParam = string.Empty;
        if (!System.IO.File.Exists(imagePath))
        {
            imageParam = string.Empty;
        }
        else
        {
            // 3️⃣ Read file bytes directly (no System.Drawing)
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);

            // 4️⃣ Convert to Base64 for RDLC external image
            imageParam = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }

        int monthId = 1;

        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        var monthName = dfi.MonthNames[monthId - 1];

        DateTime firstDateOfMonth = new(DateTime.Now.Year, monthId, 1);

        // Get the last day of the month by adding one month to the first day and subtracting one day
        DateTime lastDateOfMonth = firstDateOfMonth.AddMonths(1).AddDays(-1);
        var StartDate = firstDateOfMonth.ToString("yyyy-MM-dd");
        var EndDate = lastDateOfMonth.ToString("yyyy-MM-dd");

        var attendanceList = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(StartDate, EndDate);
        List<RptMonthlyAttendanceVM> monthlyAttendance = new List<RptMonthlyAttendanceVM>();
        foreach (var student in students)
        {
            RptMonthlyAttendanceVM monthlyAttendanceVM = new RptMonthlyAttendanceVM
            {
                ClassRoll = student.ClassRoll.ToString(),
                StudentName = student.Name,
                Day1 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "01") ? "P" : ".",
                Day2 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "02") ? "P" : ".",
                Day3 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "03") ? "P" : ".",
                Day4 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "04") ? "P" : ".",
                Day5 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "05") ? "P" : ".",
                Day6 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "06") ? "P" : ".",
                Day7 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "07") ? "P" : ".",
                Day8 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "08") ? "P" : ".",
                Day9 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "09") ? "P" : ".",
                Day10 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "10") ? "P" : ".",
                Day11 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "11") ? "P" : ".",
                Day12 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "12") ? "P" : ".",
                Day13 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "13") ? "P" : ".",
                Day14 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "14") ? "P" : ".",
                Day15 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "15") ? "P" : ".",
                Day16 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "16") ? "P" : ".",
                Day17 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "17") ? "P" : ".",
                Day18 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "18") ? "P" : ".",
                Day19 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "19") ? "P" : ".",
                Day20 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "20") ? "P" : ".",
                Day21 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "21") ? "P" : ".",
                Day22 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "22") ? "P" : ".",
                Day23 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "23") ? "P" : ".",
                Day24 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "24") ? "P" : ".",
                Day25 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "25") ? "P" : ".",
                Day26 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "26") ? "P" : ".",
                Day27 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "27") ? "P" : ".",
                Day28 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "28") ? "P" : ".",
                Day29 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "29") ? "P" : ".",
                Day30 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "30") ? "P" : ".",
                Day31 = attendanceList.Any(s => AttendancePinMatcher.Matches(s.CardNo, student.UniqueId) && s.PunchDatetime.ToString("dd") == "31") ? "P" : ".",

            };
            monthlyAttendance.Add(monthlyAttendanceVM);
        }
        var monthlyLogoBase64 = string.Empty;
        if (!string.IsNullOrEmpty(institute.Logo))
        {
            var mImgPath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);
            if (System.IO.File.Exists(mImgPath))
                monthlyLogoBase64 = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(mImgPath));
        }

        var monthlyDoc = new MonthlyAttendancePdfBuilder(
            institute, monthlyLogoBase64, monthlyAttendance, monthName, "Class 6", int.Parse(lastDateOfMonth.ToString("dd")));
        var pdf = monthlyDoc.GeneratePdf();
        return File(pdf, mediaType);
    }

 #endregion Attendance Reports

    #region Result or MarkSheet
    [Authorize(Policy = "SubjectWiseMarkSheetReportsPolicy")]
    public async Task<IActionResult> SubjectWiseMarkSheet(string reportType, int examId, string fileName)
    {
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();

        var reportPath = Path.Combine(_host.WebRootPath, "Reports", "ExamResult", "Rpt_Subject_Wise_MarkSheet.rdlc");

        string imageParam = "";
        var instituteLogoPath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);

        // 2️⃣ Check if file exists
        if (!System.IO.File.Exists(instituteLogoPath))
        {
            imageParam = string.Empty;
        }
        else
        {
            // 3️⃣ Read file bytes directly (no System.Drawing)
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(instituteLogoPath);

            // 4️⃣ Convert to Base64 for RDLC external image
            imageParam = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }



        var examDetails = await _reportManager.GetSubjectWiseMarkSheet(examId);
        if (examDetails == null || !examDetails.Any())
            return new JsonResult("Nothing Found");

        var examInfo = await _academicExamManager.GetByIdAsync(examId);
        if (examInfo == null)
            return new JsonResult("Please provide proper information");

        var subjectDoc = new SubjectWiseMarkSheetPdfBuilder(
            institute,
            StripDataUriPrefix(imageParam),
            examDetails.OrderBy(s => s.ClassRoll).ToList(),
            examInfo.AcademicClass?.Name ?? "",
            examInfo.AcademicSubject?.SubjectName ?? "",
            examInfo.AcademicExamGroup?.ExamGroupName ?? "",
            examInfo.Employee?.EmployeeName ?? "",
            examInfo.AcademicExamGroup?.AcademicSession?.Name ?? "",
            examInfo.TotalMarks);

        var pdf = subjectDoc.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "subject_marksheet" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        return File(pdf, MediaTypeNames.Application.Octet, $"{fileName}.pdf");

    }

    [Authorize(Policy = "SubjectWiseMarkSheetReportsPolicy")]
    public async Task<IActionResult> StudentWiseMarkSheet(string reportType, int examGroupId, int classId, string fileName)
    {
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();

        var reportPath = _host.WebRootPath + "\\Reports\\Rpt_Student_Wise_MarkSheet.rdlc";

        string imageParam = "";
        var instituteLogoPath = _host.WebRootPath + "\\Images\\Institute\\" + institute.Logo;

        
        if (System.IO.File.Exists(instituteLogoPath))
        {
            // Read image bytes directly (cross-platform)
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(instituteLogoPath);

            // Convert to Base64 for RDLC or other use
            imageParam = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }


        var examDetails = await _reportManager.GetStudentWiseMarkSheet(examGroupId, classId);
        if (examDetails == null || !examDetails.Any())
            return new JsonResult("Nothing Found");

        var gradingTables = (await _gradingTableManager.GetAllAsync()).ToList();
        var academicExamGroup = await _academicExamGroupManager.GetByIdAsync(examGroupId);
        string signaturePath2 = Path.Combine(_host.WebRootPath, "Images", "Institute", "signature.jpg");
        string signatureParam2 = System.IO.File.Exists(signaturePath2)
            ? Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(signaturePath2))
            : string.Empty;

        var annualReports2 = new Dictionary<int, List<SubRerportAnnualReport>>();
        foreach (var sid in examDetails.Select(r => r.StudentId).Distinct())
            annualReports2[sid] = await GetAnnualReportData(sid);

        var swBuilder = new Utilities.Reports.MarkSheetPdfBuilder(
            institute,
            StripDataUriPrefix(imageParam),
            signatureParam2,
            examDetails,
            gradingTables,
            annualReports2,
            academicExamGroup?.ExamGroupName ?? "",
            examDetails.FirstOrDefault()?.ClassName ?? "",
            DateTime.Today.ToString("dd MMM yyyy"),
            examDetails.Max(r => r.TotalObtainMarks).ToString());

        var pdf = swBuilder.Generate();
        fileName = (string.IsNullOrEmpty(fileName) ? "marksheet" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        return File(pdf, MediaTypeNames.Application.Octet, $"{fileName}.pdf");
    }

    [Authorize(Policy = "StudentWiseMarkSheetReportsPolicy")]
    public async Task<IActionResult> MarkSheetReport()
    {
        GlobalUI.PageTitle = "Mark-Sheet Automation";
        ViewBag.SessionList = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name").ToList();
        ViewBag.ClassList = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList();
        ViewBag.ExamGroupList = new SelectList(await _academicExamGroupManager.GetAllAsync(), "Id", "ExamGroupName").ToList();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetSectionsByExamGroup(int examGroupId, int classId, int sessionId)
    {
        var sections = await _academicSectionManager.GetAllByExamGroupIdClassIdSessionId(examGroupId, classId, sessionId);
        return Json(sections.Select(s => new { s.Id, s.Name }));
    }

    [Authorize(Policy = "StudentWiseMarkSheetReportsPolicy")]
    public async Task<IActionResult> MarkSheetReportExport(
    string reportType,
    string fileName,
    int examGroupId,
    int academicClassId,
    int? sectionId,
    int sessionId,
    int studentId)
    {
        // 1️⃣ Get report data
        var results = await _reportManager.GetStudentWiseMarkSheet(examGroupId, academicClassId);

        if (results == null || results.Count == 0)
            return new JsonResult("Result not found");
        // Get highest marks
        var highestMarks = results.Max(r => r.TotalObtainMarks).ToString();

        if (sectionId.HasValue && sectionId.Value > 0)
            results = results.Where(s => s.AcademicSectionId == sectionId).ToList();

        if (studentId > 0)
            results = results.Where(s => s.StudentId == studentId).ToList();

        if (results.Count == 0)
            return new JsonResult("Result not found");

        // Get institute info
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        string imageParam = await LoadInstituteLogoAsync(institute.Logo);

        string signaturePath = Path.Combine(_host.WebRootPath, "Images", "Institute", "signature.jpg");
        string signatureParam = System.IO.File.Exists(signaturePath)
            ? Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(signaturePath))
            : string.Empty;

        string publicationDate = results.Select(r => r.CreatedAt).FirstOrDefault().ToString("dd MMM yyyy");
        string examName = results.Select(s => s.ExamGroupName).FirstOrDefault();
        string className = results.Select(s => s.ClassName).FirstOrDefault();

        var gradingTables = (await _gradingTableManager.GetAllAsync()).ToList();

        var annualReports = new Dictionary<int, List<SubRerportAnnualReport>>();
        foreach (var sid in results.Select(r => r.StudentId).Distinct())
        {
            annualReports[sid] = await GetAnnualReportData(sid);
        }

        var builder = new Utilities.Reports.MarkSheetPdfBuilder(
            institute,
            StripDataUriPrefix(imageParam),
            signatureParam,
            results,
            gradingTables,
            annualReports,
            examName,
            className,
            publicationDate,
            highestMarks);

        var reportBytes = builder.Generate();

        string mediaType = MediaTypeNames.Application.Pdf;

        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = fileName + "_" + DateTime.Now.ToString("dd MMM yyyy");
            return File(reportBytes, mediaType, GetReportName(fileName, "pdf"));
        }

        return File(reportBytes, mediaType);
    }
    #endregion Result or MarkSheet

    #region Student Payment Reports
    [Authorize(Policy = "StudentPaymentInfoReportsPolicy")]
    public IActionResult StudentPaymentInfo()
    {
        return View();
    }

    [Authorize(Policy = "StudentPaymentInfoReportsPolicy")]
    public async Task<IActionResult> StudentPaymentInfoExport(string reportType, string fileName, int classRoll, string fromDate, string toDate)
    {
        Student student = await _studentManager.GetStudentByClassRollAsync(classRoll);
        if (student == null)
        {
            return new JsonResult("Sorry! Student Data Not Found");
        }
        DateTime fdate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime tdate = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var studentPayments = await _reportManager.GetStudentPaymentsByRoll(classRoll, fdate.ToString("yyyy-MM-dd"), tdate.ToString("yyyy-MM-dd"));
        if (studentPayments.Count == 0)
        {
            return new JsonResult("Sorry! Any Payment Data Not Found");
        }

        double totalPaid = studentPayments.Sum(s => s.TotalPayment);
        string numberToWord = NumberToWords.ConvertAmount(totalPaid);
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
        {
            return new JsonResult("Institute Information not found!");
        }

        string logoBase64 = "";
        if (!string.IsNullOrEmpty(institute.Logo))
        {
            var imagePath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);
            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                logoBase64 = Convert.ToBase64String(imageBytes);
            }
        }

        var document = new StudentPaymentInfoPdfBuilder(
            institute,
            logoBase64,
            student,
            studentPayments,
            fdate.ToString("dd MMM yyyy"),
            tdate.ToString("dd MMM yyyy"),
            numberToWord
        );

        var pdf = document.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "payment_info" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}.pdf\"";
        return File(pdf, MediaTypeNames.Application.Pdf);
    }
    [Authorize(Policy = "StudentPaymentReportsPolicy")]
    public async Task<IActionResult> StudentPaymentReport()
    {
        ViewData["AcademicClass"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name").ToList();

        return View();
    }

    [Authorize(Policy = "StudentPaymentReportsPolicy")]
    public async Task<IActionResult> StudentPaymentReportExport(string reportType, string fileName, string fromDate, string toDate, string academicClassId, string academicSectionId, string paymentCategory, int? paymentType)
    {
        string reportName = "Payments Report";
        var sPayment = await _reportManager.GetStudentPayment(fromDate, toDate, academicClassId, academicSectionId);
        if (paymentType != null)
        {
            var feeHead = await _studentFeeHeadManager.GetByIdAsync((int)paymentType);
            if (feeHead != null)
            {
                sPayment = sPayment.Where(s => s.PaymentType == feeHead.Name).ToList();
            }
        }
        if (paymentCategory == "residential")
        {
            sPayment = sPayment.Where(s => s.IsResidential).ToList();
            reportName = "Payments Summary Report(Residential)";
        }
        else if (paymentCategory == "nonResidential")
        {
            sPayment = sPayment.Where(s => s.IsResidential == false).ToList();
            reportName = "Payments Summary Report(Non Residential)";
        }
        else
        {
            reportName = "Payments Summary Report(Residential and Non Residential)";
        }
        if (sPayment.Count == 0)
        {
            return new JsonResult("Sorry! Data Not Found");
        }
        double amount = sPayment.Sum(m => m.TotalPayment);
        Institute institute = await _instituteManager.GetByIdAsync(1);
        if (institute == null)
        {
            return new JsonResult("Institute Information not found!");
        }
        string logoBase64 = "";
        if (!string.IsNullOrEmpty(institute.Logo))
        {
            var imagePath = Path.Combine(_host.WebRootPath, "Images", "Institute", institute.Logo);
            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                logoBase64 = Convert.ToBase64String(imageBytes);
            }
        }

        var document = new StudentPaymentSummaryPdfBuilder(
            institute,
            logoBase64,
            sPayment,
            reportName,
            fromDate,
            toDate,
            NumberToWords.ConvertAmount(amount)
        );

        var pdf = document.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "payment_summary" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}.pdf\"";
        return File(pdf, MediaTypeNames.Application.Pdf);

    }

    [Authorize(Policy = "ReceiptPaymentReportsPolicy")]
    public async Task<IActionResult> ReceiptPaymentExportOld(string reportType, int paymentId, string myFileName)
    {
        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
        {
            return new JsonResult("Institute Information not found!");
        }
        string mediaType = "application/pdf";
        var path = _host.WebRootPath + "\\Reports\\Accounts\\Rpt_Payment_Receipt.rdlc";

        string imageParam = "";
        var imagePath = Path.Combine(
            _host.WebRootPath,
            "Images",
            "Institute",
            "institute.jpeg"
        );



        if (System.IO.File.Exists(imagePath))
        {
            // Read image bytes directly (cross-platform)
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);

            // Convert to Base64 for RDLC or other use
            imageParam = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }
        List<RptPaymentReceiptVM> rptPaymentReceiptVMs = await _reportManager.GetPaymentReceiptReport(paymentId);

        string numberToWord = string.Empty;
        if (rptPaymentReceiptVMs != null && rptPaymentReceiptVMs.Count > 0)
        {
            numberToWord = NumberToWords.ConvertAmount(rptPaymentReceiptVMs.Select(s => s.TotalPayment).FirstOrDefault());
        }

        using var report = new LocalReport();
        report.DataSources.Add(new ReportDataSource("Payment_Receipt_DataSet", rptPaymentReceiptVMs));
        var parameters = new[] {
            new ReportParameter("InstituteName", institute.Name),
            new ReportParameter("InstituteAddress", institute.Address),
            //new ReportParameter("EIINNo", institute.EIIN),
            new ReportParameter("Logo", imageParam),
            new ReportParameter("ReportName", "Payment Receipt"),
            new ReportParameter("AmountInWord", numberToWord)
        };
        report.ReportPath = path;
        report.SetParameters(parameters);
        var pdf = report.Render("pdf");
        if (!string.IsNullOrEmpty(myFileName) && myFileName.Length > 0)
        {
            return File(pdf, MediaTypeNames.Application.Octet, GetReportName(myFileName, reportType));
        }
        return File(pdf, mediaType);
    }

    [Authorize(Policy = "ReceiptPaymentReportsPolicy")]
    public async Task<IActionResult> ReceiptPaymentExport(string reportType, int paymentId, string myFileName)
    {
        // 1. Validate institute
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        if (institute == null)
            return NotFound("Institute information not found.");

        // 2. Build report path (cross-platform)
        var reportPath = Path.Combine(_host.WebRootPath, "Reports", "Accounts", "Rpt_Payment_Receipt.rdlc");
        if (!System.IO.File.Exists(reportPath))
            return NotFound("Report template not found.");

        // 3. Load logo (cross-platform, with fallback)
        var logoBase64 = await LoadInstituteLogoAsync(institute.Logo);

        // 4. Fetch report data
        var receiptData = await _reportManager.GetPaymentReceiptReport(paymentId);
        if (receiptData == null || receiptData.Count == 0)
            return NotFound("No payment receipt data found for the given payment ID.");

        // 5. Convert amount to words
        var amountInWords = NumberToWords.ConvertAmount(receiptData.First().TotalPayment);

        // 6. Build and render report
        var pdf = RenderReceiptReport(reportPath, receiptData, institute, logoBase64, amountInWords);

        // 7. Return file
        return BuildFileResult(pdf, reportType, myFileName);
    }

    #endregion Student Payment Reports

    #region Admit Card Reports
    [Authorize(Policy = "AdmitCardReportsPolicy")]
    public async Task<IActionResult> AdmitCardExport(string reportType, string fileName, int monthId, string academicClassId, string academicSectionId, int examTypeId, int examGroupId = 0, bool download = false, string sheet = "Legal")
    {
        var institute = await _instituteManager.GetByIdAsync(1);
        if (institute == null)
        {
            return new JsonResult("Institute information not found!");
        }

        string defaultInstituteLogo = "smslogo.png";
        string logoFileName = string.IsNullOrWhiteSpace(institute.Logo)
            ? defaultInstituteLogo
            : institute.Logo;

        string logoPath = Path.Combine(_host.WebRootPath, "Images", "Institute", logoFileName);

        if (!System.IO.File.Exists(logoPath))
        {
            await _appLogger.InfoAsync($"Institute logo not found");
            logoPath = Path.Combine(_host.WebRootPath, "Images", "Institute", defaultInstituteLogo);
        }

        string imageParam = ConvertImageToBase64(logoPath);

        string signaturePath = Path.Combine(_host.WebRootPath, "Images", "Institute", "signature.jpg");
        string signatureParam = System.IO.File.Exists(signaturePath)
            ? Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(signaturePath))
            : string.Empty;

        var renderType = string.IsNullOrEmpty(reportType) ? RenderType.Pdf : GetRenderType(reportType);

        int.TryParse(academicClassId, out int aClassId);
        int.TryParse(academicSectionId, out int aSectionId);

        var admitCardList = await _reportManager.GetAdmitCard(monthId, aClassId, aSectionId, examTypeId, examGroupId);
        admitCardList = admitCardList.Where(s => s.StudentStauts == true).ToList();

        if (!admitCardList.Any())
        {
            return new JsonResult("No data found");
        }

        // Cards are tiled onto the chosen paper (3 per Legal sheet, 2 per A4) so they
        // print at their true physical size instead of being rescaled by the printer.
        var cardSheet = string.Equals(sheet, "A4", StringComparison.OrdinalIgnoreCase)
            ? AdmitCardSheet.A4
            : AdmitCardSheet.Legal;

        var admitDoc = new AdmitCardPdfBuilder(institute, StripDataUriPrefix(imageParam), signatureParam, admitCardList, cardSheet);
        var result = admitDoc.GeneratePdf();
        fileName = (string.IsNullOrEmpty(fileName) ? "admit_card" : fileName) + "_" + DateTime.Now.ToString("yyyyMMdd");

        Response.Headers["Content-Disposition"] = download
            ? $"attachment; filename=\"{fileName}.pdf\""
            : $"inline; filename=\"{fileName}.pdf\"";

        return File(result, "application/pdf");
    }

    #endregion Admit Card Reports

    #region Common Methods
    private static RenderType GetRenderType(string reportType)
    {
        var renderType = reportType.ToLower() switch
        {
            "word" => RenderType.Word,
            "xls" => RenderType.Excel,
            _ => RenderType.Pdf,
        };
        return renderType;
    }

    private static string GetReportName(string reportName, string reportType)
    {
        string outputFileName = reportType.ToUpper() switch
        {
            "XLS" => reportName + ".xls",
            "WORD" => reportName + ".doc",
            "EXCEL" => reportName + ".xlsx",
            "EXCELOPENXML" => reportName + ".xlsx",
            _ => reportName + ".pdf",
        };
        return outputFileName;
    }

    private async Task<List<SubRerportAnnualReport>> GetAnnualReportData(int studentId)
    {
        var student = await _studentManager.GetByIdAsync(studentId);
        var examGroups = await _academicExamGroupManager.GetBySession(student.AcademicSessionId);
        List<SubRerportAnnualReport> filteredData = new List<SubRerportAnnualReport>();
        if (examGroups != null)
        {
            foreach (var eGroup in examGroups)
            {
                var examResults = await _examResultManager.GetExamResultsByExamGroupNClassId(eGroup.Id, student.AcademicClassId);
                foreach (var rItem in examResults.Where(s => s.StudentId == student.Id))
                {
                    SubRerportAnnualReport subRerportAnnualReport = new SubRerportAnnualReport()
                    {
                        MonthSL = eGroup.ExamMonthId,
                        AttendancePercent = rItem.AttendancePercentage.ToString(),
                        MeritPosition = rItem.Rank,
                        TotalStudent = examResults.Count()
                    };
                    filteredData.Add(subRerportAnnualReport);
                }
            }
        }
        List<SubRerportAnnualReport> data = new List<SubRerportAnnualReport>();
        DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        int monthSL = 1;
        foreach (var monthName in dateTimeFormat.MonthNames)
        {
            var fData = filteredData.FirstOrDefault(s => s.MonthSL == monthSL);
            if (!string.IsNullOrEmpty(monthName))
            {
                data.Add(new SubRerportAnnualReport()
                {
                    Month = monthName,
                    MonthSL = monthSL,
                    //AttendancePercent = "",
                    //MeritPosition = 0,
                    //TotalStudent = 90
                    AttendancePercent = fData?.AttendancePercent,
                    MeritPosition = fData?.MeritPosition,
                    TotalStudent = fData?.TotalStudent
                });
                monthSL++;
            }
        }
        return data;
    }
    #endregion Common Methods

    #region Gradingtable
    public async Task<IActionResult> GradingTable()
    {
        var gTables = await _gradingTableManager.GetAllAsync();
        LocalReport localSubReport = new LocalReport();
        localSubReport.ReportPath = _host.WebRootPath + "//Reports/Rpt_GradingTable.rdlc";
        localSubReport.DataSources.Add(new ReportDataSource("GradingTable_DataSet", gTables));
        var pdf = localSubReport.Render("pdf");
        return File(pdf, "application/pdf");
    }
    #endregion GradingTable

    #region Annual Report
    public async Task<IActionResult> AnnualReport()
    {
        var data = await GetAnnualReportData(1920);
        LocalReport localSubReport = new LocalReport();

        var parameters = new[] {
            new ReportParameter("StudentId", 1920.ToString()),
        };
        localSubReport.ReportPath = _host.WebRootPath + "//Reports/rptAnnualReport.rdlc";
        localSubReport.SetParameters(parameters);


        localSubReport.DataSources.Add(new ReportDataSource("AnnualReportDS", data));
        var pdf = localSubReport.Render("pdf");
        return File(pdf, "application/pdf");
    }
    #endregion Annual Report

    private string ConvertImageToBase64(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
            return string.Empty;

        try
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imagePath);
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return string.Empty;
        }
    }

  
    /// <summary>
    /// Loads the institute logo as a Base64-encoded data URI.
    /// Tries institute-specific images first (jpeg/jpg/png),
    /// then falls back to the default smsLogo.png in the same directory.
    /// Returns empty string if no image is found at all.
    /// </summary>
    private async Task<string> LoadInstituteLogoAsync(string imageName)
    {
        var imageDirectory = Path.Combine(_host.WebRootPath, "Images", "Institute");

        var candidates = new[]
        {
        (imageName, "iamge/png"),
        ("institute.jpeg", "image/jpeg"),
        ("institute.jpg",  "image/jpeg"),
        ("institute.png",  "image/png"),
        ("smsLogo.png",    "image/png"),   // default fallback
    };

        foreach (var (fileName, mimeType) in candidates)
        {
            var imagePath = Path.Combine(imageDirectory, fileName);
            if (!System.IO.File.Exists(imagePath)) continue;

            var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }

        return string.Empty;
    }

    private byte[] RenderReceiptReport(
        string reportPath,
        List<RptPaymentReceiptVM> data,
        Institute institute,
        string logoBase64,
        string amountInWords)
    {
        var cleanLogo = StripDataUriPrefix(logoBase64);
        var doc = new PaymentReceiptPdfBuilder(institute, cleanLogo, data, amountInWords);
        return doc.GeneratePdf();
    }

    /// <summary>
    /// Returns the appropriate FileResult based on whether a download filename was provided.
    /// </summary>
    private IActionResult BuildFileResult(byte[] pdf, string reportType, string myFileName)
    {
        const string PdfMimeType = "application/pdf";
        const string OctetMimeType = MediaTypeNames.Application.Octet;

        if (!string.IsNullOrWhiteSpace(myFileName))
            return File(pdf, OctetMimeType, GetReportName(myFileName, reportType));

        return File(pdf, PdfMimeType);
    }

    private static string StripDataUriPrefix(string dataUri)
    {
        if (string.IsNullOrEmpty(dataUri)) return string.Empty;

        var commaIndex = dataUri.IndexOf(',');
        return commaIndex >= 0 ? dataUri[(commaIndex + 1)..] : dataUri;
    }
}
