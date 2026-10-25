using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ExamResult;
using SMS_App.ViewModels.ExamVM;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class ExamResultsController : Controller
{
    private readonly IExamResultManager _examResultManager;
    private readonly IAcademicExamManager _academicExamManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentManager _studentManager;
    private readonly IAcademicExamTypeManager _academicExamTypeManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IAcademicExamGroupManager _academicExamGroupManager;
    private readonly IAcademicSessionManager _sessionManager;
    private readonly IGradingTableManager _gradingTableManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IOffDayManager _OffDayManager;
    private readonly IAcademicExamDetailsManager _academicExamDetailsManager;
    private readonly IAcademicSectionManager _academicSectionManager;
    public ExamResultsController(IExamResultManager examResultManager, IAcademicExamManager academicExamManager, UserManager<ApplicationUser> userManager, IStudentManager studentManager, IAcademicExamTypeManager academicExamTypeManager, IAcademicClassManager academicClassManager, IAcademicExamGroupManager academicExamGroupManager, IAcademicSessionManager sessionManager, IGradingTableManager gradingTableManager, IInstituteManager instituteManager, IAttendanceMachineManager attendanceMachineManager, IOffDayManager offDayManager, IAcademicExamDetailsManager academicExamDetailsManager, IAcademicSectionManager academicSectionManager)
    {
        _examResultManager = examResultManager;
        _academicExamManager = academicExamManager;
        _userManager = userManager;
        _studentManager = studentManager;
        _academicExamTypeManager = academicExamTypeManager;
        _academicClassManager = academicClassManager;
        _academicExamGroupManager = academicExamGroupManager;
        _sessionManager = sessionManager;
        _gradingTableManager = gradingTableManager;
        _instituteManager = instituteManager;
        _attendanceMachineManager = attendanceMachineManager;
        _OffDayManager = offDayManager;
        _academicExamDetailsManager = academicExamDetailsManager;
        _academicSectionManager = academicSectionManager;
    }
    // GET: ExamResultsController
    [Authorize(Policy = "IndexExamResultsPolicy")]
    public async Task<ActionResult> Index()
    {
        GlobalUI.PageTitle = "Exams Result";

        List<ExamResultVM> exams = new();

        List<AcademicExam> existingExams = (List<AcademicExam>)await _academicExamManager.GetAllAsync();
        var user = await _userManager.GetUserAsync(User);

        ViewData["ExamType"] = new SelectList(await _academicExamTypeManager.GetAllAsync(), "Id", "ExamTypeName");
        ViewData["ClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");


        //await _academicExamTypeManager.GetAllAsync();
        if (user.UserType == 's')
        {
            Student student = await _studentManager.GetByIdAsync(user.ReferenceId);
            existingExams = existingExams.Where(e => e.AcademicSectionId == student.AcademicSectionId &&
                //e.AcademicSessionId == student.AcademicSessionId &&
                e.AcademicSubject.AcademicClassId == student.AcademicClassId)
                .ToList();
        }

        if (existingExams != null)
        {
            foreach (AcademicExam academicExam in existingExams)
            {

                ExamResultVM exam = new();
                exam.ExamId = academicExam.Id;
                //exam.ExamName = academicExam.ExamName;
                //exam.AcademicClass = academicExam.AcademicSubject.AcademicClass.Name;
                //exam.TotalExaminee = academicExam.AcademicExamDetails.Count;
                exams.Add(exam);
            }
        }
        return View(exams);
    }

    [Authorize(Policy = "ResultExamResultsPolicy")]
    public async Task<ActionResult> Result()
    {
        GlobalUI.PageTitle = "Academic Results";

        ViewData["ExamGroupList"] = new SelectList(await _academicExamGroupManager.GetAllAsync(), "Id", "ExamGroupName");
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");

        return View();
    }

    [HttpPost]
    [Authorize(Policy = "ResultExamResultsPolicy")]
    public IActionResult Result(string resultType, int examGroupId, int classId)
    {
        return View();
    }

    [Authorize(Policy = "SubjectWiseResultExamResultsPolicy")]
    public ActionResult SubjectWiseResult()
    {
        GlobalUI.PageTitle = GlobalUI.SiteTitle = "Student-Wise Result";
        return View();
    }

    [Authorize(Policy = "ClassWiseResultExamResultsPolicy")]
    public async Task<ActionResult> ClassWiseResult()
    {
        GlobalUI.PageTitle = GlobalUI.SiteTitle = "Class-Wise Result";
        var sessions = await _sessionManager.GetAllAsync();
        var currentSession = await _sessionManager.GetCurrentAcademicSessionAsync();
        ViewData["SessionList"] = new SelectList(sessions, "Id", "Name", currentSession?.Id);
        ViewData["ExamGroupList"] = new SelectList(await _academicExamGroupManager.GetAllAsync(currentSession?.Id ?? 0), "Id", "ExamGroupName");
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        ViewBag.SectionList = new SelectList(new List<AcademicSection>(), "Id", "Name");

        ViewBag.sessionId = currentSession?.Id;
        ViewBag.IsLoading = false;
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "ClassWiseResultExamResultsPolicy")]
    public async Task<ActionResult> ClassWiseResult(int examGroupId, int classId, int? sectionId, int sessionId)
    {
        GlobalUI.PageTitle = "Class-Wise Result";

        ViewBag.examGroupId = examGroupId;
        ViewBag.classId = classId;
        ViewBag.sectionId = sectionId;
        ViewBag.sessionId = sessionId;

        var sessions = await _sessionManager.GetAllAsync();
        var examGroups = sessionId > 0 ? await _academicExamGroupManager.GetAllAsync(sessionId) : await _academicExamGroupManager.GetAllAsync();
        ViewData["SessionList"] = new SelectList(sessions, "Id", "Name", sessionId);
        ViewData["ExamGroupList"] = new SelectList(examGroups, "Id", "ExamGroupName", examGroupId);
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classId);
        var examList = new List<AcademicExam>();
        if (sectionId > 0)
        {
            examList = await _academicExamManager.GetByClassIdExamGroupIdSectionIdAsync(examGroupId, classId, (int)sectionId);
        }
        else
        {
            examList = await _academicExamManager.GetByClassIdExamGroupIdAsync(examGroupId, classId);
        }

        var sectionsInExam = (List<AcademicSection>)await _academicSectionManager.GetAllByExamGroupIdClassIdSessionId(examGroupId, classId, sessionId);
        if (sectionsInExam.Count > 0)
        {
            sectionsInExam.Insert(0, new AcademicSection { Id = 0, Name = "All" });
        }

        ViewBag.SectionList = new SelectList(sectionsInExam, "Id", "Name", sectionId);
        if (sectionId > 0)
        {
            examList = examList.Where(e => e.AcademicSectionId == sectionId).ToList();
        }
        if (examList == null || examList.Count <= 0)
        {
            TempData["failed"] = "Exam Not Found";
            ViewBag.IsLoading = true;
            return View();
        }
        List<Student> students;
        if (sectionId > 0)
        {
            students = await _studentManager.GetStudentsByClassSessionSectionAsync(sessionId, classId, sectionId.Value);
        }
        else
        {
            var studentIdsInExam = examList.SelectMany(e => e.AcademicExamDetails).Select(d => d.StudentId).Distinct().ToList();
            var allStudents = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(sessionId, classId);
            students = allStudents.Where(s => studentIdsInExam.Contains(s.Id)).ToList();
        }
        List<ExaminationResultVM> examinationResultVMs = new List<ExaminationResultVM>();
        var institute = await _instituteManager.GetFirstOrDefaultAsync();
        var examGroup = await _academicExamGroupManager.GetByIdAsync(examGroupId);
        var academicClass = await _academicClassManager.GetByIdAsync(classId);

        ViewBag.InstituteName = institute.Name;
        ViewBag.logo = institute.Logo;
        ViewBag.ExamGroupName = examGroup.ExamGroupName;
        ViewBag.academicClassName = academicClass.Name;
        var newManupulateResult = examList.SelectMany(s => s.AcademicExamDetails).GroupBy(s => new { s.AcademicExamId, s.StudentId }).Select(group => new
        {
            examId = group.Key.AcademicExamId,
            studentId = group.Key.StudentId,
            obtainMark = group.Sum(m => m.ObtainMark)
        }).ToList();

        int monthDays = ViewBag.daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, examGroup.ExamMonthId);

        DateTime firstDateOfMonth = new(DateTime.Now.Year, examGroup.ExamMonthId, 1);
        DateTime lastDateOfMonth = firstDateOfMonth.AddMonths(1).AddDays(-1);

        string StartDate = firstDateOfMonth.ToString("yyyy-MM-dd");
        string EndDate = lastDateOfMonth.ToString("yyyy-MM-dd");

        var attendanceList = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(StartDate, EndDate);
        List<DateTime> monthlyHolidays = await _OffDayManager.GetMonthlyHolidaysAsync(firstDateOfMonth.ToString("MMyyyy"));

        foreach (var student in students.Where(s => s.Status == true).OrderBy(s => s.ClassRoll))
        {
            ExaminationResultVM examinationResultVM = new ExaminationResultVM();
            examinationResultVM.ClassRoll = student.ClassRoll;
            examinationResultVM.StudentName = student.Name;
            examinationResultVM.Phone1 = student.GuardianPhone;
            examinationResultVM.Phone2 = student.PhoneNo;
            examinationResultVM.Gender = student.Gender;
            double obtainPoint = 0.00;
            int subCount = 0;
            int totalFail = 0;
            foreach (var exam in examList)
            {
                ExaminationResultDetailsVMs exDetail = new ExaminationResultDetailsVMs();
                exDetail.SubjectName = exam.AcademicSubject.SubjectName;
                exDetail.IsReligion = exam.AcademicSubject.ReligionId != null ? true : false;
                exDetail.ObtainMarks = exam.AcademicExamDetails.Where(s => s.StudentId == student.Id && s.AcademicExamId == exam.Id).Sum(s => s.ObtainMark);
                exDetail.ObtainPoint = await GetGradePointByNumber((exDetail.ObtainMarks * 100) / exam.TotalMarks);
                exDetail.ObtainGrade = await GetGradeByPoint(exDetail.ObtainPoint);
                exDetail.ExamMarks = exam.TotalMarks;
                exDetail.HighestObtainMark = exam.AcademicExamDetails
                    .Where(s => s.AcademicExamId == exam.Id)
                    .OrderByDescending(s => s.ObtainMark)
                    .Select(s => s.ObtainMark).Max();

                examinationResultVM.ExaminationResultDetailsVMs.Add(exDetail);
                subCount++;

                if (exDetail.ObtainPoint <= 0)
                {
                    totalFail++;
                }
                obtainPoint += exDetail.ObtainPoint;

            }
            examinationResultVM.FailSubCount = totalFail;
            examinationResultVM.CGPA = examinationResultVM.FailSubCount > 0 ? 0.00 : (obtainPoint / subCount);
            examinationResultVM.TotalMarks = newManupulateResult.Where(s => s.studentId == student.Id).Sum(s => s.obtainMark);
            examinationResultVM.Grade = await GetGradeByPoint(examinationResultVM.CGPA);
            examinationResultVM.GradeComment = await GetGradeComments(examinationResultVM.CGPA);
            var myAttendances = attendanceList.Where(t => SMS.Entities.Utilities.AttendancePinMatcher.MatchesStudent(t.CardNo, student.MachineUserId, student.UniqueId, student.ClassRoll)).ToList();
            IDictionary<int, bool> daysPresents = new Dictionary<int, bool>();
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
            }
            int total = daysPresents.Where(m => m.Value == true).Count();
            examinationResultVM.AttendancePercent = (total * 100) / (monthDays - monthlyHolidays.Count);
            examinationResultVM.PreviousMonthRank = 10;
            examinationResultVMs.Add(examinationResultVM);
        }
        var rankedResults = examinationResultVMs
            .OrderByDescending(result => result.CGPA)
            .ThenBy(result => result.FailSubCount)
            .ThenByDescending(result => result.TotalMarks)
            .ThenByDescending(result => result.AttendancePercent)
            .Select((result, index) => new ExaminationResultVM
            {
                TotalMarks = result.TotalMarks,
                CGPA = result.CGPA,
                FailSubCount = result.FailSubCount,
                Rank = index + 1,
                ClassRoll = result.ClassRoll
            })
            .ToList();
        foreach (var result in examinationResultVMs)
        {
            result.Rank = rankedResults.Where(s => s.ClassRoll == result.ClassRoll).Select(s => s.Rank).FirstOrDefault();
        }
        ViewBag.IsLoading = true;
        return View(examinationResultVMs);
    }

    [Authorize(Policy = "ClassWiseResultAfterProcessExamResultsPolicy")]
    public async Task<ActionResult> ClassWiseResultAfterProcess()
    {
        GlobalUI.PageTitle = GlobalUI.SiteTitle = "Class-Wise Result";
        var currentSession = await _sessionManager.GetCurrentAcademicSessionAsync();
        var allExamGroups = await _academicExamGroupManager.GetAllAsync();
        allExamGroups = allExamGroups.Where(s => s.AcademicSessionId == currentSession.Id).ToList();
        ViewData["ExamGroupList"] = new SelectList(allExamGroups, "Id", "ExamGroupName");
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");

        ViewBag.IsLoading = false;
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "ClassWiseResultAfterProcessExamResultsPolicy")]
    public async Task<ActionResult> ClassWiseResultAfterProcess(string resultType, int examGroupId, int classId)
    {
        GlobalUI.PageTitle = "Class-Wise Result 1";
        ViewData["ExamGroupList"] = new SelectList(await _academicExamGroupManager.GetAllAsync(), "Id", "ExamGroupName", examGroupId);
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", classId);
        var resultList = await _examResultManager.GetExamResultsByExamGroupNClassId(examGroupId, classId);
        if (resultList == null || resultList.Count <= 0)
        {
            TempData["failed"] = "Result Not Found";
            ViewBag.IsLoading = true;
            return View();
        }

        ViewBag.IsLoading = true;
        return View(resultList);
    }

    [HttpGet]
    [Authorize(Policy = "SubjectWiseResultAfterProcessExamResultsPolicy")]
    public ActionResult SubjectWiseResultAfterProcess()
    {
        return View();
    }

    [Authorize(Policy = "SubjectWiseResultAfterProcessExamResultsPolicy")]
    public ActionResult SubjectWiseResultAfterProcess(int subjectId)
    {
        return View();
    }

    [Authorize(Policy = "StudentWiseResultAfterProcessExamResultsPolicy")]
    public async Task<ActionResult> StudentWiseResult()
    {
        GlobalUI.PageTitle = GlobalUI.SiteTitle = "Student-Wise Result";

        ViewData["ExamType"] = new SelectList(await _academicExamTypeManager.GetAllAsync(), "Id", "ExamTypeName");
        ViewData["ClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");


        return View();
    }

    // GET: ExamResultsController/Details/5
    [Authorize(Policy = "DetailsExamResultsPolicy")]
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: ExamResultsController/Create
    [Authorize(Policy = "CreateExamResultsPolicy")]
    public ActionResult Create()
    {
        return View();
    }

    // POST: ExamResultsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateExamResultsPolicy")]
    public ActionResult Create(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: ExamResultsController/Edit/5
    [Authorize(Policy = "EditExamResultsPolicy")]
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: ExamResultsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditExamResultsPolicy")]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    [Authorize(Policy = "ProcessResultExamResultsPolicy")]
    public async Task<ActionResult> ProcessResult(int classId, int groupId, int scrollPosition)
    {
        //// Storing scroll position in session
        HttpContext.Session.SetInt32("ScrollPosition", scrollPosition);

        //// Retrieving scroll position from session
        //var scrollPosition = HttpContext.Session.GetInt32("ScrollPosition");
        if (classId > 0 && groupId > 0)
        {
            bool isExamExist = _examResultManager.IsResultProcessedAsync(groupId, classId);
            if (isExamExist)
            {
                TempData[""] = TempData["failed"] = "This Result is already processed";
                return RedirectToAction("Details", "AcademicExamGroup", new { id = groupId });
            }
        var exams = await _academicExamManager.GetAllByClassIdExamGroupIdAsync(groupId, classId);
        var session = await _sessionManager.GetCurrentAcademicSessionAsync();
            var examGroup = await _academicExamGroupManager.GetByIdAsync(groupId);

            List<Student> students = exams?
                .Where(e => e.AcademicExamDetails != null)
                .SelectMany(e => e.AcademicExamDetails)
                .Select(d => d.Student)
                .DistinctBy(s => s.Id)
                .ToList() ?? new();

            if (students.Count <= 0)
            {
                return Json("Student Not Found");
            }
            foreach (var student in students)
            {
                var selectedExams = exams.Where(s => s.AcademicSectionId == null || s.AcademicSectionId == student.AcademicSectionId).ToList();
                if (selectedExams.Count <= 0)
                {
                    continue;
                }
                if (student.ClassRoll == 2610001)
                {
                    Console.Write("Got it");
                }
                ExamResult examResult = new ExamResult();
                examResult.CreatedAt = DateTime.Now;
                examResult.CreatedBy = HttpContext.Session.GetString("UserId");
                examResult.MACAddress = MACService.GetMAC();

                examResult.AcademicExamGroupId = groupId;
                examResult.StudentId = student.Id;
                examResult.AcademicClassId = classId;
                examResult.TotalObtainMarks = await GetTotalObtainMarkFromExam(groupId, student.Id);
                
                
                string gpa = (await GetCgpaPointFromExam(groupId, student.Id, student.AcademicClassId)).ToString("F2");
                examResult.CGPA = Convert.ToDouble(gpa);
                examResult.FinalGrade = await GetGradeByPoint(examResult.CGPA);
                examResult.GradeComments = await GetGradeComments(examResult.CGPA);
                examResult.TotalFails = 0;
                if (examResult.CGPA <= 0)
                {
                    examResult.TotalFails = await GetTotalFailFromExam(groupId, student.Id, student.AcademicClassId);
                }
                
                List<ExamResultDetail> examResultDetails = new List<ExamResultDetail>();


                var subjectWiseExams = selectedExams.DistinctBy(s => s.AcademicSubjectId);
                foreach (var item in subjectWiseExams)
                {
                    ExamResultDetail examResultDetail = new()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = HttpContext.Session.GetString("UserId"),
                        MACAddress = MACService.GetMAC(),
                        ExamResultId = examResult.Id,
                        AcademicSubjectId = item.AcademicSubjectId,
                    };
                    var targetedExams = selectedExams.Where(s => s.AcademicSubjectId == item.AcademicSubjectId);

                    double subWiseObtainMark = 0;
                    double subWiseTotalMark = 0;
                    bool isPass = true;
                    foreach (var ex in targetedExams)
                    {
                        double gotMarks = ex.AcademicExamDetails.Where(s => s.StudentId == student.Id).Select(s => s.ObtainMark).FirstOrDefault(); 
                        var gotPointSinglePortion = await GetGradePointByNumber(( gotMarks* 100) / ex.TotalMarks);
                        if (gotPointSinglePortion<=0)
                        {
                            isPass = false;
                        }
                        subWiseObtainMark += gotMarks;
                        subWiseTotalMark += ex.TotalMarks;
                    };
                    examResultDetail.ObtainMark = subWiseObtainMark;
                    examResultDetail.TotalMark = subWiseTotalMark;
                    if (isPass)
                    {
                        double gotPoint = await GetGradePointByNumber((subWiseObtainMark * 100) / subWiseTotalMark);
                        examResultDetail.GPA = gotPoint;
                        examResultDetail.Grade = await GetGradeByPoint(gotPoint);
                    }
                    else
                    {
                        examResultDetail.GPA = 0;
                        examResultDetail.Grade = "F";
                    }
                        examResultDetails.Add(examResultDetail);
                }
                


                string monthYear = examGroup.ExamMonthId.ToString().PadLeft(2, '0') + DateTime.Now.Year;
                var monthlyAttendance = await _attendanceMachineManager.GetAttendanceByMonthSingleStudent(student.Id, monthYear);
                if (monthlyAttendance.Count > 0)
                {
                    var holidays = await _OffDayManager.GetMonthlyHolidaysAsync(examGroup.ExamMonthId.ToString().PadLeft(2, '0') + DateTime.Now.Year.ToString());
                    int totalActiveDay = DateTime.DaysInMonth(DateTime.Now.Year, examGroup.ExamMonthId) - holidays.Count;
                    if (totalActiveDay > 22)
                    {
                        //string fff = "No";
                    }
                    examResult.AttendancePercentage = (monthlyAttendance.Count * 100) / totalActiveDay;
                }
                else
                {
                    examResult.AttendancePercentage = 0;
                }
                examResult.ExamResultDetails = examResultDetails;
                await _examResultManager.AddAsync(examResult);
            }

            //int totalUpdated = await UpdateRanking(groupId, classId);
            //TempData["created"] = "Total "+totalUpdated+" result updated";
        }
        else
        {
            TempData["failed"] = "Not Found!";
        }
        return RedirectToAction("UpdateRanking", new { groupId = groupId, classId = classId });
    }

    [Authorize(Policy = "UpdateRankingExamResultsPolicy")]
    public async Task<IActionResult> UpdateRanking(int groupId, int classId)
    {
        int rankingUpdate = 0;
        try
        {
            var examResults = await _examResultManager.GetExamResultsByExamGroupNClassId(groupId, classId);
            examResults = examResults.Where(s => s.AcademicExamGroupId == groupId && s.AcademicClassId == classId).ToList();

            var rankedResults = examResults
            .OrderBy(result => result.TotalFails)
            .ThenByDescending(result => result.CGPA)
            .ThenByDescending(result => result.TotalObtainMarks)
            .ThenByDescending(result => result.AttendancePercentage)
            .Select((result, index) => new ExaminationResultVM
            {
                TotalMarks = result.TotalObtainMarks,
                CGPA = result.CGPA,
                FailSubCount = result.TotalFails,
                Rank = index + 1,
                ClassRoll = result.Student.ClassRoll
            })
            .ToList();

            foreach (var result in examResults)
            {
                result.Rank = rankedResults.Where(s => s.ClassRoll == result.Student.ClassRoll).Select(s => s.Rank).FirstOrDefault();
                await _examResultManager.UpdateAsync(result);
                rankingUpdate++;
            }
        }
        catch (Exception)
        {

            throw;
        }

        return RedirectToAction("Details", "AcademicExamGroup", new { id = groupId });
    }

    // GET: ExamResultsController/Delete/5
    [Authorize(Policy = "DeleteExamResultsPolicy")]
    public ActionResult Delete(int id)
    {
        return View();
    }

    [Authorize(Policy = "DeleteResultExamResultsPolicy")]
    public async Task<ActionResult> DeleteResult(int classId, int groupId, int scrollPosition)
    {
        HttpContext.Session.SetInt32("ScrollPosition", scrollPosition);

        bool deleted = await _examResultManager.DeleteByGroupAndClassAsync(groupId, classId);
        if (deleted)
        {
            TempData["success"] = "Exam Results are deleted successfully";
        }
        else
        {
            TempData["failed"] = "Exam Results Not found";
        }
        return RedirectToAction("details", "AcademicExamGroup", new { id = groupId });
    }
    [Authorize(Policy = "LiveResultExamResultsPolicy")]
    public async Task<IActionResult> LiveResult(LiveResultVM model)
    {
        var sessions = await _sessionManager.GetAllAsync();
        
        // Use selected session from query param only
        var selectedSessionId = model.AcademicSessionId;
        var examGroups = selectedSessionId > 0 
            ? await _academicExamGroupManager.GetAllAsync(selectedSessionId)
            : new List<AcademicExamGroup>();

        // Early return for missing parameters
        if (model.AcademicClassId == 0 || model.ExamGroupId == 0)
        {
            var vm = new LiveResultVM
            {
                AcademicSessionList = sessions.Select(s => new SelectListItem 
                { 
                    Value = s.Id.ToString(), 
                    Text = s.Name 
                }).ToList(),
                AcademicExamGroupList = examGroups.Select(eg => new SelectListItem 
                { 
                    Value = eg.Id.ToString(), 
                    Text = eg.ExamGroupName 
                }).ToList()
            };
            return View(vm);
        }
        if (model.AcademicSectionId == 0)
        {
            model.AcademicSectionId = null;
        }

        var expectedGroup = examGroups?
            .Where(g => g.Id == model.ExamGroupId)
            .ToList() ?? new List<AcademicExamGroup>();

        // Extract distinct classes from all groups
        var classList = expectedGroup?
            .Where(g => g.AcademicExams != null && g.AcademicExams.Count > 0 && g.Id == model.ExamGroupId)
            .SelectMany(g => g.AcademicExams)
            .Where(e => e.AcademicClass != null)
            .Select(e => e.AcademicClass)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList() ?? new List<AcademicClass>();

        //Extract Distinct Sections from all class
        var sectionList = expectedGroup?
            .Where(g => g.AcademicExams != null && g.AcademicExams.Count > 0 && g.Id == model.ExamGroupId)
            .SelectMany(g => g.AcademicExams)
            .Where(e => e.AcademicSection != null && e.AcademicClassId == model.AcademicClassId)
            .Select(e => e.AcademicSection)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList() ?? new List<AcademicSection>();

        // Fetch live result
        var liveResult = await _academicExamManager.GetLiveResultByGroupIdClassIdSectionId(
            model.ExamGroupId,
            model.AcademicClassId,
            model.AcademicSectionId
        );

        // Build ViewModel
        liveResult.AcademicSessionId = model.AcademicSessionId;
        liveResult.AcademicSessionList = sessions.Select(s => new SelectListItem 
        { 
            Value = s.Id.ToString(), 
            Text = s.Name 
        }).ToList();
        liveResult.AcademicExamGroupList = examGroups.Select(eg => new SelectListItem 
        { 
            Value = eg.Id.ToString(), 
            Text = eg.ExamGroupName 
        }).ToList();

        liveResult.AcademicClassList = classList.Select(c => new SelectListItem 
        { 
            Value = c.Id.ToString(), 
            Text = c.Name 
        }).ToList();

        liveResult.AcademicSectionList = sectionList.Select(s => new SelectListItem 
        { 
            Value = s.Id.ToString(), 
            Text = s.Name 
        }).ToList();

        liveResult.AcademicClassId = model.AcademicClassId;
        liveResult.ExamGroupId = model.ExamGroupId;

        ViewBag.classId = model.AcademicClassId;
        ViewBag.examGroupId = model.ExamGroupId;
        ViewBag.IsLoading = true;

        return View(liveResult);
    }

    // POST: ExamResultsController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteExamResultsPolicy")]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
    private async Task<double> GetGradePointByNumber(double number)
    {
        try
        {
            var gradings = await _gradingTableManager.GetAllAsync();
            int intValue = (int)Math.Round(number);
            var result = gradings.FirstOrDefault(s => s.NumberRangeMin <= intValue && s.NumberRangeMax >= intValue)?.GradePoint;
            if (result.HasValue && result.Value >= 0)
            {
                return (double)result.Value;
            }

            return 0.00;
        }
        catch
        {
            throw;
        }
    }

    private async Task<string> GetGradeByPoint(double number)
    {
        string grade = string.Empty;
        try
        {
            var gradings = await _gradingTableManager.GetAllAsync();

            var result = (from g in gradings
                          where number >= Convert.ToDouble(g.GradePoint)
                          select g.LetterGrade).ToList().Take(1);

            if (result.Any())
            {
                grade = result.FirstOrDefault().ToString();

            }
        }
        catch (Exception)
        {

            throw;
        }
        return grade;
    }
    private async Task<string> GetGradeComments(double number)
    {
        string gradeComments = string.Empty;
        try
        {
            var gradings = await _gradingTableManager.GetAllAsync();

            var result = (from g in gradings
                          where number >= Convert.ToDouble(g.GradePoint)
                          select g.gradeComments).ToList().Take(1);

            if (result != null && result.Any())
            {
                gradeComments = result.FirstOrDefault().ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return gradeComments;
    }

    private async Task<double> GetTotalObtainMarkFromExam(int examGroupId, int studentId)
    {
        double totalObtainMark = 0;

        if (examGroupId > 0 && studentId > 0)
        {
            var eDetails = await _academicExamDetailsManager.GetAllByExamGroupAndStudentId(examGroupId, studentId);
            if (eDetails != null)
            {
                totalObtainMark = eDetails.Sum(s => s.ObtainMark);
            }
        }
        return totalObtainMark;
    }
    private async Task<double> GetCgpaPointFromExam(int examGroupId, int studentId, int academicClassId)
    {
        var eDetails = await _academicExamDetailsManager.GetAllByExamGroupAndStudentId(examGroupId, studentId);
        if (!eDetails.Any()) return 0;

        // Aggregate marks per subject
        Dictionary<int, double> subWiseNumber = [];
        Dictionary<int, double> subWiseMark = [];

        foreach (var e in eDetails)
        {
            int subjectId = e.AcademicExam.AcademicSubjectId;
            double percentage = (e.ObtainMark * 100) / e.AcademicExam.TotalMarks;

            if (await GetGradePointByNumber(percentage) <= 0) return 0;

            if (subWiseNumber.ContainsKey(subjectId))
            {
                subWiseNumber[subjectId] += e.ObtainMark;
                subWiseMark[subjectId] += e.AcademicExam.TotalMarks;
            }
            else
            {
                subWiseNumber.Add(subjectId, e.ObtainMark);
                subWiseMark.Add(subjectId, e.AcademicExam.TotalMarks);
            }
        }

        // Calculate total GPA across subjects
        double totalGPA = 0;
        foreach (var (subjectId, obtainMark) in subWiseNumber)
        {
            double gpa = await GetGradePointByNumber((obtainMark * 100) / subWiseMark[subjectId]);
            if (gpa <= 0) return 0;
            totalGPA += gpa;
        }

        // Return CGPA only if student attended all subjects
        int totalSubject = await _academicExamManager.GetTotalExamAsync(examGroupId, academicClassId);
        return subWiseNumber.Count == totalSubject ? totalGPA / totalSubject : 0;
    }
    private async Task<int> GetTotalFailFromExam(int examGroupId, int studentId, int academicClassId)
    {
        var examDetails = await _academicExamDetailsManager
            .GetAllByExamGroupAndStudentId(examGroupId, studentId);

        var totalExams = await _academicExamManager
            .GetTotalExamAsync(examGroupId, academicClassId);

        if (examDetails == null || examDetails.Count == 0)
            return totalExams;

        // Group by subject so we count one fail per failed subject,
        // not one fail per failed component (MCQ, Written, Practical).
        var subjectGroups = examDetails
            .Where(d => d.AcademicExam?.TotalMarks > 0)
            .GroupBy(d => d.AcademicExam.AcademicSubjectId);

        int totalFail = 0;

        foreach (var subjectGroup in subjectGroups)
        {
            bool subjectFailed = false;
            foreach (var exam in subjectGroup)
            {
                double percentage = (exam.ObtainMark * 100.0) / exam.AcademicExam.TotalMarks;
                double gpa = await GetGradePointByNumber(percentage);
                if (gpa <= 0)
                {
                    subjectFailed = true;
                    break;
                }
            }
            if (subjectFailed)
                totalFail++;
        }

        // Subjects with no exam details at all count as fails
        int subjectsWithDetails = subjectGroups.Count();
        totalFail += totalExams - subjectsWithDetails;

        return totalFail;
    }

    private async Task<int> GetAttendancePercentage(int monthId, int studentId)
    {
        int attendancePercentage = 0;
        try
        {
            int year = DateTime.Now.Year; // Change this to the desired year
            int monthNumber = monthId; // Change this to the desired month (1 for January, 2 for February, etc.)

            // Get the first day of the specified month
            DateTime startDate = new DateTime(year, monthNumber, 1);

            // Get the last day of the specified month by adding one month and subtracting one day
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            Student student = await _studentManager.GetByIdAsync(studentId);

            var attendanceList = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(startDate.ToString(), endDate.ToString());
            List<DateTime> monthlyHolidays = await _OffDayManager.GetMonthlyHolidaysAsync(startDate.ToString("MMyyyy"));
            var myAttendances = attendanceList.Where(t => SMS.Entities.Utilities.AttendancePinMatcher.MatchesStudent(t.CardNo, student.MachineUserId, student.UniqueId, student.ClassRoll)).ToList();
            int monthDays = DateTime.DaysInMonth(DateTime.Today.Year, monthId);
            IDictionary<int, bool> daysPresents = new Dictionary<int, bool>();
            for (int i = 1; i <= monthDays; i++)
            {
                var currentDate = startDate.AddDays(i - 1).ToString("ddMMyyyy");
                var attended = myAttendances.Where(a => a.PunchDatetime.ToString("ddMMyyyy") == currentDate).Any();
                if (attended)
                {
                    daysPresents.Add(i, true);
                }
                else
                {
                    daysPresents.Add(i, false);
                }
            }
            int total = daysPresents.Where(m => m.Value == true).Count();
            attendancePercentage = (total * 100) / (monthDays - monthlyHolidays.Count);
        }
        catch (Exception)
        {
            throw;
        }

        return attendancePercentage;
    }

    public JsonResult IsResultProcessed(int groupId, int classId)
    {
        bool isExist = _examResultManager.IsResultProcessedAsync(groupId, classId);
        return new JsonResult(isExist);
    }


}
