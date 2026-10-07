using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.Enums;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ExamVM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize]
public class AcademicExamsController : Controller
{
    private readonly IAcademicExamManager _examManager;
    private readonly IAcademicSessionManager _sessionManager;
    private readonly IAcademicClassManager _classManager;
    private readonly IAcademicExamTypeManager _examTypeManager;
    private readonly IAcademicSubjectManager _academicSubjectManager;
    private readonly IEmployeeManager _employeeManager;
    private readonly IMapper _mapper;
    private readonly IAcademicSectionManager _academicSectionManager;
    private readonly IStudentManager _studentManager;
    private readonly IAcademicExamDetailsManager _academicExamDetailsManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAcademicExamGroupManager _examGroupManager;
    private readonly IGenderManager _genderManager;
    public AcademicExamsController(IAcademicExamManager examManager, IAcademicSessionManager sessionManager, IAcademicClassManager classManager, IAcademicExamTypeManager examTypeManager, IAcademicSubjectManager academicSubjectManager, IEmployeeManager employeeManager, IMapper mapper, IAcademicSectionManager academicSectionManager, IStudentManager studentManager, IAcademicExamDetailsManager academicExamDetailsManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAcademicExamGroupManager academicExamGroupManager, IGenderManager genderManager)
    {
        _examManager = examManager;
        _sessionManager = sessionManager;
        _classManager = classManager;
        _examTypeManager = examTypeManager;
        _academicSubjectManager = academicSubjectManager;
        _employeeManager = employeeManager;
        _mapper = mapper;
        _academicSectionManager = academicSectionManager;
        _studentManager = studentManager;
        _academicExamDetailsManager = academicExamDetailsManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _examGroupManager = academicExamGroupManager;
        _genderManager = genderManager;
    }

    // GET: AcademicExamsController
    [Authorize(Roles = "Admin, Teacher, SuperAdmin")]
    [Authorize(Policy = "IndexAcademicExamPolicy")]
    public async Task<ActionResult> Index()
    {
        if (TempData["error"] != null)
        {
            ViewBag.error = TempData["error"].ToString();
        }
        ViewModels.AcademicVM.AcademicExamVM academicExamVM = new ViewModels.AcademicVM.AcademicExamVM();

        var currentSession = await _sessionManager.GetCurrentAcademicSessionAsync();
        var examGroups = await _examGroupManager.GetAllAsync(currentSession.Id);
        var classes = await _classManager.GetAllAsync();
        var employees = (List<Employee>)await _employeeManager.GetAllAsync();
        var sessionWiseExams = await _examManager.GetExaminationListLiteAsync();

        academicExamVM.AcademicExamGroupList = new SelectList(examGroups, "Id", "ExamGroupName").ToList();
        academicExamVM.AcademicClassList = new SelectList(classes, "Id", "Name").ToList();
        academicExamVM.TeacherList = new SelectList(employees.Where(e => e.Status == true).OrderBy(e => e.JoiningDate).ThenBy(e => e.EmployeeName), "Id", "EmployeeName").ToList();
        academicExamVM.ExamCategoryList = new List<SelectListItem>();
        foreach (var category in Enum.GetValues(typeof(ExamCategory)))
        {
            var newSelectListItem = new SelectListItem { Value = category.ToString(), Text = category.ToString() };
            academicExamVM.ExamCategoryList.Add(newSelectListItem);
        }

        if (sessionWiseExams != null)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            bool isAdminUser = roles.Any(r => r.Contains("Admin") || r.Contains("SuperAdmin"));

            if (!isAdminUser)
            {
                sessionWiseExams = FilterExamsByUser(sessionWiseExams, user.ReferenceId);
            }

            academicExamVM.ExamSessionVM = sessionWiseExams.OrderByDescending(s => s.SessionName.Substring(s.SessionName.Length - 4)).ToList();
        }

        return View(academicExamVM);
    }

    private List<ExamSessionDto> FilterExamsByUser(List<ExamSessionDto> exams, int userReferenceId)
    {
        var filtered = new List<ExamSessionDto>();
        foreach (var session in exams)
        {
            var groupsWithUserExams = session.ExamGroupDtos?
                .Where(g => g.ExaminationDtos?.Any(e => e.ExaminationDetailsDtos?.Any(d => d.EmployeeId == userReferenceId) == true) == true)
                .ToList();

            if (groupsWithUserExams?.Any() == true)
            {
                filtered.Add(new ExamSessionDto
                {
                    Id = session.Id,
                    SessionName = session.SessionName,
                    ExamGroupDtos = groupsWithUserExams
                });
            }
        }
        return filtered;
    }

    // GET: AcademicExamsController/Details/5
    [Authorize(Policy = "DetailsAcademicExamPolicy")]
    public async Task<ActionResult> Details(int id, bool mergeMode = false)
    {

        var exam = await _examManager.GetByIdAsync(id);
        if (exam == null)
        {
            TempData["error"] = "Data not found";
            return RedirectToAction("index");
        }

        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);
        bool isAdminUser = roles.Any(r => r.Contains("Admin") || r.Contains("SuperAdmin"));

        if (user.UserType == 'e')
        {
            if (user.ReferenceId != exam.EmployeeId)
            {
                if (isAdminUser == false)
                {
                    return RedirectToAction("AccessDenied", "Accounts");
                }
            }
        }

        if (mergeMode)
        {
            return await GetMergedExamDetails(id);
        }

        var academicExamDetailVM = new AcademicExamDetailVM();
        academicExamDetailVM = _mapper.Map<AcademicExamDetailVM>(exam);
        var allStudents = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(exam.AcademicExamGroup.AcademicSessionId, exam.AcademicClassId);

        academicExamDetailVM.StudentList = allStudents.OrderBy(s => s.ClassRoll).Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name + "-(" + s.ClassRoll + ")"
        }).ToList();


        var selectedStudent = new List<Student>();

        // Create a HashSet of student IDs from AcademicExamDetails for quick lookup
        var existingStudentIds = new HashSet<int>(
            academicExamDetailVM.AcademicExamDetails.Select(detail => detail.Student.Id)
        );

        // Filter students who are not in the existingStudentIds
        selectedStudent.AddRange(
            allStudents.Where(student => !existingStudentIds.Contains(student.Id))
        );

        academicExamDetailVM.MissingStudentList = selectedStudent.OrderBy(s => s.ClassRoll).Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name + "-(" + s.ClassRoll + ")"
        }).ToList();

        var academicExamVM = new ViewModels.AcademicVM.AcademicExamVM
        {
            AcademicExamGroup = exam.AcademicExamGroup,
            AcademicExamDetails = exam.AcademicExamDetails
                .OrderBy(s => s.Student != null ? s.Student.ClassRoll : int.MaxValue)
                .ToList(),
            AcademicClass = exam.AcademicClass,
            AcademicSection = exam.AcademicSection,
            AcademicSubject = exam.AcademicSubject,
            Employee = exam.Employee,
            TotalMarks = exam.TotalMarks
        };

        return View(academicExamDetailVM);
    }

    private async Task<ActionResult> GetMergedExamDetails(int primaryExamId)
    {
        var primaryExam = await _examManager.GetByIdAsync(primaryExamId);
        if (primaryExam == null)
        {
            TempData["error"] = "Data not found";
            return RedirectToAction("index");
        }

        var mergedExams = await _examManager.GetMergedExamsAsync(primaryExamId);
        if (mergedExams == null || mergedExams.Count == 0)
        {
            TempData["error"] = "No exams found to merge";
            return RedirectToAction("Details", new { id = primaryExamId });
        }

        var mergedVM = new MergedExamDetailVM
        {
            IsMergedView = true,
            PrimaryExamId = primaryExamId,
            TotalMarks = primaryExam.TotalMarks,
            ExamGroupName = primaryExam.AcademicExamGroup.ExamGroupName,
            SubjectName = primaryExam.AcademicSubject.SubjectName,
            SubjectCode = primaryExam.AcademicSubject.SubjectCode?.ToString() ?? string.Empty,
            ExamCategory = primaryExam.ExamCategory,
            ClassName = primaryExam.AcademicClass.Name,
            ClassId = primaryExam.AcademicClassId,
            SessionName = primaryExam.AcademicExamGroup.AcademicSession.Name,
            MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primaryExam.AcademicExamGroup.ExamMonthId),
            TeacherName = primaryExam.Employee.EmployeeName,
            IsLocked = mergedExams.All(e => e.Status)
        };

        foreach (var exam in mergedExams)
        {
            mergedVM.MergedExams.Add(new MergedExamInfo
            {
                ExamId = exam.Id,
                SectionName = exam.AcademicSection?.Name ?? "All Sections",
                SectionId = exam.AcademicSectionId,
                TotalStudents = exam.AcademicExamDetails.Count,
                Status = exam.Status
            });

            foreach (var detail in exam.AcademicExamDetails.Where(d => d.Student != null && d.Student.Status == true).OrderBy(d => d.Student.ClassRoll).ToList())
            {
                mergedVM.MergedExamDetails.Add(new MergedExamDetailItem
                {
                    ExamDetailId = detail.Id,
                    ExamId = exam.Id,
                    StudentId = detail.StudentId,
                    StudentName = detail.Student.Name,
                    ClassRoll = detail.Student.ClassRoll,
                    SectionName = exam.AcademicSection?.Name ?? "All Sections",
                    SectionId = exam.AcademicSectionId,
                    ObtainMark = detail.ObtainMark,
                    Status = detail.Status,
                    Remarks = detail.Remarks,
                    EditedBy = detail.EditedBy
                });
            }
        }

        var allStudents = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(
            primaryExam.AcademicExamGroup.AcademicSessionId, primaryExam.AcademicClassId);

        var existingStudentIds = mergedExams
            .SelectMany(e => e.AcademicExamDetails)
            .Select(d => d.StudentId)
            .Distinct()
            .ToHashSet();

        var missingStudents = allStudents.Where(s => !existingStudentIds.Contains(s.Id) && s.Status == true).ToList();

        mergedVM.StudentList = allStudents.Where(s => existingStudentIds.Contains(s.Id) && s.Status == true)
            .OrderBy(s => s.ClassRoll)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name + "-(" + s.ClassRoll + ")"
            }).ToList();

        mergedVM.MissingStudentList = missingStudents.OrderBy(s => s.ClassRoll)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name + "-(" + s.ClassRoll + ")"
            }).ToList();

        ViewData["MergedExamVM"] = mergedVM;
        return View("MergedDetails", mergedVM);
    }


    // POST: AcademicExamsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateAcademicExamPolicy")]
    public async Task<ActionResult> Create(List<AcademicExam> AcademicExams)
    {
        int success = 0;
        int failed = 0;
        try
        {
            if (AcademicExams.Count > 0)
            {
                foreach (AcademicExam exam in AcademicExams)
                {
                    // Handle multiple sections - get the list of section IDs
                    List<int> selectedSections = new List<int>();

                    // Try to parse AcademicSectionIdList if it's sent as JSON string
                    if (exam.AcademicSectionIdList != null && exam.AcademicSectionIdList.Count > 0)
                    {
                        selectedSections = exam.AcademicSectionIdList;
                    }
                    else if (Request.Form.ContainsKey("AcademicExams[0].AcademicSectionIdList"))
                    {
                        var jsonStr = Request.Form["AcademicExams[0].AcademicSectionIdList"].ToString();
                        if (!string.IsNullOrEmpty(jsonStr))
                        {
                            try
                            {
                                selectedSections = System.Text.Json.JsonSerializer.Deserialize<List<int>>(jsonStr) ?? new List<int>();
                            }
                            catch { selectedSections = new List<int>(); }
                        }
                    }

                    // If AcademicSectionIdList has valid sections (not 0, not null), create exam for each section
                    if (selectedSections.Count > 0 && !selectedSections.Contains(0))
                    {
                        // Remove duplicates from selectedSections
                        selectedSections = selectedSections.Distinct().ToList();

                        foreach (var sectionId in selectedSections)
                        {
                            // Check if exam already exists for this section
                            var isExistForSection = await _examManager.GetAcademicExam(
                                exam.AcademicExamGroupId,
                                exam.AcademicClassId,
                                exam.AcademicSubjectId,
                                exam.ExamCategory,
                                sectionId);

                            if (isExistForSection != null)
                            {
                                failed++;
                                continue; // Skip duplicate
                            }

                            var examForSection = new AcademicExam
                            {
                                AcademicExamGroupId = exam.AcademicExamGroupId,
                                AcademicClassId = exam.AcademicClassId,
                                AcademicSubjectId = exam.AcademicSubjectId,
                                EmployeeId = exam.EmployeeId,
                                TotalMarks = exam.TotalMarks,
                                ExamCategory = exam.ExamCategory,
                                AcademicSectionId = sectionId,
                                CreatedAt = DateTime.Now,
                                CreatedBy = HttpContext.Session.GetString("UserId"),
                                MACAddress = MACService.GetMAC()
                            };

                            var isSaved = await SaveAcademicExamWithDetails(examForSection);
                            if (isSaved) success++;
                            else failed++;
                        }
                        // Stop here - don't process single section case
                        TempData["success"] = "Success: " + success + " Failed: " + failed;
                        return RedirectToAction("index");
                    }

                    // Normalize AcademicSectionId: treat empty/0 as null (all sections)
                    if (exam.AcademicSectionId == null || exam.AcademicSectionId == 0)
                    {
                        exam.AcademicSectionId = null;
                    }

                    var isExist = await _examManager.GetAcademicExam(exam.AcademicExamGroupId, exam.AcademicClassId, exam.AcademicSubjectId, exam.ExamCategory, exam.AcademicSectionId);

                    if (isExist != null)
                    {
                        failed++;
                        continue;
                    }
                    await SaveAcademicExamWithDetails(exam);
                    success++;
                }
                TempData["success"] = "Success: " + success + " Failed: " + failed;
            }
            else
            {
                TempData["success"] = "No data found to create";
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = "Exception: " + ex.Message;
        }
        return RedirectToAction("index");
    }

    private async Task<bool> SaveAcademicExamWithDetails(AcademicExam exam)
    {
        AcademicSubject academicSubject = await _academicSubjectManager.GetByIdAsync(exam.AcademicSubjectId);
        exam.CreatedAt = DateTime.Now;
        exam.CreatedBy = HttpContext.Session.GetString("UserId");
        exam.MACAddress = MACService.GetMAC();
        bool isSaved = await _examManager.AddAsync(exam);
        if (isSaved)
        {
            AcademicExamGroup academicExamGroup = await _examGroupManager.GetByIdAsync(exam.AcademicExamGroupId);
            var students = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(academicExamGroup.AcademicSessionId, exam.AcademicClassId);
            students = students.Where(s => s.Status == true).ToList();
            foreach (Student student in students)
            {
                if (exam.AcademicSectionId != null && exam.AcademicSectionId > 0)
                {
                    if (student.AcademicSectionId != exam.AcademicSectionId)
                    {
                        continue;
                    }
                }
                if (academicSubject.ReligionId != null && academicSubject.ReligionId > 0)
                {
                    if (student.ReligionId != academicSubject.ReligionId)
                    {
                        continue;
                    }
                }
                AcademicExamDetail academicExamDetail = new AcademicExamDetail();
                academicExamDetail.AcademicExamId = exam.Id;
                academicExamDetail.ObtainMark = 0;
                academicExamDetail.StudentId = student.Id;
                academicExamDetail.Status = true;
                academicExamDetail.CreatedAt = DateTime.Now;
                academicExamDetail.CreatedBy = HttpContext.Session.GetString("UserId");
                academicExamDetail.MACAddress = MACService.GetMAC();
                await _academicExamDetailsManager.AddAsync(academicExamDetail);
            }
            return true;
        }
        return false;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddStudentToExistingExam(IndividualStudentExamGroup model)
    {
        var existingExam = await _examManager.GetByIdAsync(model.AcademicExamId);
        if (existingExam != null)
        {
            AcademicExamDetail newDetail = new AcademicExamDetail()
            {
                AcademicExamId = existingExam.Id,
                ObtainMark = model.ObtainMarks,
                StudentId = model.StudentId,
                Remarks = model.Remarks,
                Status = true,
                CreatedAt = DateTime.Now,
                CreatedBy = HttpContext.Session.GetString("UserId"),
                EditedBy = HttpContext.Session.GetString("UserId")
            };
            var ss = await _academicExamDetailsManager.AddAsync(newDetail);
            if (ss)
            {
                TempData["success"] = "Student Added in Exam";
            }
            existingExam.EditedAt = DateTime.Now;
            existingExam.EditedBy = HttpContext.Session.GetString("UserId");
            await _examManager.UpdateAsync(existingExam);
        }
        return RedirectToAction("Details", new { id = model.AcademicExamId });
    }

    [HttpPost]
    public async Task<JsonResult> AddAllStudentsToExam(int academicExamId)
    {
        var exam = await _examManager.GetByIdAsync(academicExamId);
        if (exam == null)
        {
            return Json(new { success = false, message = "Exam not found." });
        }

        var sessionId = exam.AcademicExamGroup.AcademicSessionId;
        var classId = exam.AcademicClassId;
        var sectionId = exam.AcademicSectionId;

        List<Student> allStudents;
        if (sectionId.HasValue)
        {
            allStudents = await _studentManager.GetStudentsByClassSessionSectionAsync(sessionId, classId, sectionId.Value);
        }
        else
        {
            allStudents = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(sessionId, classId);
        }

        var existingStudentIds = new HashSet<int>(
            exam.AcademicExamDetails.Select(d => d.StudentId)
        );

        var newStudents = allStudents.Where(s => !existingStudentIds.Contains(s.Id) && s.Status == true).ToList();
        var skippedCount = allStudents.Count(s => existingStudentIds.Contains(s.Id));

        foreach (var student in newStudents)
        {
            var newDetail = new AcademicExamDetail
            {
                AcademicExamId = exam.Id,
                StudentId = student.Id,
                ObtainMark = 0,
                Status = true,
                CreatedAt = DateTime.Now,
                CreatedBy = HttpContext.Session.GetString("UserId"),
                EditedBy = HttpContext.Session.GetString("UserId")
            };
            await _academicExamDetailsManager.AddAsync(newDetail);
        }

        exam.EditedAt = DateTime.Now;
        exam.EditedBy = HttpContext.Session.GetString("UserId");
        await _examManager.UpdateAsync(exam);

        return Json(new
        {
            success = true,
            addedCount = newStudents.Count,
            skippedCount = skippedCount
        });
    }

    // POST: AcademicExamsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditAcademicExamPolicy")]
    public async Task<ActionResult> Edit(int id, AcademicExam academicExam, ViewModels.ExamVM.AcademicExamVM academicExamVM)
    {

        //Minimum checking
        if (id != academicExam.Id)
        {
            TempData["error"] = "Data Id mismatched.";
            return RedirectToAction("index");
        }
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Failed! Error:" + ModelState.ErrorCount + " Please fillup the form properly.";
            return RedirectToAction("index");
        }
        //Checking, is already exist!
        AcademicExam existingExam = await _examManager.GetAcademicExam(academicExam.AcademicExamGroupId, academicExam.AcademicClassId, academicExam.AcademicSubjectId, academicExam.ExamCategory, academicExam.AcademicSectionId);

        if (existingExam != null)
        {
            TempData["error"] = "Exam is already exist in this group";
            return RedirectToAction("index");
        }
        //Checking is it same data!
        AcademicExam exam = await _examManager.GetByIdAsync(academicExam.Id);

        try
        {
            academicExam.EditedAt = DateTime.Now;
            academicExam.EditedBy = HttpContext.Session.GetString("UserId");
            academicExam.MACAddress = MACService.GetMAC();
            bool isUpdate = await _examManager.UpdateAsync(academicExam);
            if (isUpdate)
            {
                AcademicSubject academicSubject = await _academicSubjectManager.GetByIdAsync(academicExam.AcademicSubjectId);
                if (exam.AcademicSubjectId != academicExam.AcademicSubjectId)
                {
                    var examDetails = await _academicExamDetailsManager.GetByExamIdAsync(exam.Id);
                    if (examDetails != null)
                    {
                        foreach (var eDetail in examDetails)
                        {
                            await _academicExamDetailsManager.RemoveAsync(eDetail);
                        }
                    }
                    AcademicExamGroup academicExamGroup = await _examGroupManager.GetByIdAsync(academicExam.AcademicExamGroupId);
                    var students = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(academicExamGroup.AcademicSessionId, academicExam.AcademicClassId);
                    foreach (Student student in students.Where(s => s.Status = true))
                    {
                        if (academicExam.AcademicSectionId != null || academicExam.AcademicSectionId > 0)
                        {
                            if (student.AcademicSectionId != academicExam.AcademicSectionId)
                            {
                                continue;
                            }
                        }
                        if (academicSubject.ReligionId != null || academicSubject.ReligionId >= 0)
                        {
                            if (student.ReligionId != academicSubject.ReligionId)
                            {
                                continue;
                            }
                        }
                        AcademicExamDetail academicExamDetail = new AcademicExamDetail();
                        academicExamDetail.AcademicExamId = exam.Id;
                        academicExamDetail.ObtainMark = 0;
                        academicExamDetail.StudentId = student.Id;
                        academicExamDetail.Status = true;
                        academicExamDetail.CreatedAt = DateTime.Now;
                        academicExamDetail.CreatedBy = HttpContext.Session.GetString("UserId");
                        academicExamDetail.MACAddress = MACService.GetMAC();
                        await _academicExamDetailsManager.AddAsync(academicExamDetail);
                    }
                }

                TempData["success"] = "success! Data updated successfully";
                return RedirectToAction("index");
            }
            else
            {
                TempData["error"] = "Failed! Something wrong. Please try again later.";
                return RedirectToAction("index");
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = "Execption: " + ex.Message;
            return RedirectToAction("index");
        }
    }

    // GET: AcademicExamsController/Delete/5
    [HttpPost]
    [Authorize(Policy = "DeleteAcademicExamPolicy")]
    public async Task<JsonResult> Delete(int id)
    {
        try
        {
            var academicExam = await _examManager.GetByIdAsync(id);
            if (academicExam == null)
            {
                return Json(new { success = false, message = "Exam not found." });
            }

            // Creating an exam seeds a placeholder AcademicExamDetail row for every student,
            // and that FK is Restrict (cascade delete is disabled globally in ApplicationDbContext),
            // so the exam can never be removed while those child rows exist. Allow deletion only
            // while the exam is still untouched - no marks and no remarks entered - and clear the
            // placeholder rows first so the parent delete can succeed.
            var examDetails = await _academicExamDetailsManager.GetByExamIdAsync(id);

            bool hasMarksEntered = examDetails.Any(d => d.ObtainMark != 0 || !string.IsNullOrWhiteSpace(d.Remarks));
            if (hasMarksEntered)
            {
                return Json(new { success = false, message = "This exam has marks entered and cannot be deleted." });
            }

            foreach (var detail in examDetails)
            {
                await _academicExamDetailsManager.RemoveAsync(detail);
            }

            bool isRemoved = await _examManager.RemoveByIdAsync(id);
            if (isRemoved)
            {
                return Json(new { success = true, message = "Data deleted successfully." });
            }
            return Json(new { success = false, message = "Failed to delete" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Exception: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ExamMarkSubmitAcademicExamPolicy")]
    public async Task<ActionResult> ExmaMarkSubmit(ExamDetailsVM examDetailVM)
    {
        List<AcademicExamDetail> academicExamDetail = new();
        academicExamDetail = examDetailVM.AcademicExamDetails;
        foreach (AcademicExamDetail item in academicExamDetail)
        {
            var existingDetails = await _academicExamDetailsManager.GetByIdAsync(item.Id);
            if (existingDetails != null)
            {
                if (existingDetails.ObtainMark != item.ObtainMark || existingDetails.Remarks != item.Remarks || existingDetails.Status != item.Status)
                {
                    item.MACAddress = MACService.GetMAC();
                    item.EditedAt = DateTime.Now;
                    item.EditedBy = HttpContext.Session.GetString("UserId");
                    await _academicExamDetailsManager.UpdateAsync(item);
                }
            }
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Policy = "ExamMarkSubmitAcademicExamPolicy")]
    public async Task<ActionResult> ExmaMarkSubmitMerged([FromBody] List<MergedExamDetailItem> examDetails)
    {
        try
        {
            if (examDetails == null || !examDetails.Any())
            {
                return Json(new { success = false, message = "No data found to save" });
            }

            int updatedCount = 0;
            int skippedCount = 0;

            var examDetailIds = examDetails.Select(x => x.ExamDetailId).ToList();
            var existingDetailsList = await _academicExamDetailsManager.FindAllAsync(x => examDetailIds.Contains(x.Id));
            var existingDetailsDict = existingDetailsList.ToDictionary(x => x.Id);

            foreach (var item in examDetails)
            {
                if (existingDetailsDict.TryGetValue(item.ExamDetailId, out var existingDetails))
                {
                    if (existingDetails.ObtainMark != item.ObtainMark ||
                        existingDetails.Remarks != item.Remarks ||
                        existingDetails.Status != item.Status)
                    {
                        existingDetails.ObtainMark = item.ObtainMark;
                        existingDetails.Remarks = item.Remarks;
                        existingDetails.Status = item.Status;
                        existingDetails.MACAddress = MACService.GetMAC();
                        existingDetails.EditedAt = DateTime.Now;
                        existingDetails.EditedBy = HttpContext.Session.GetString("UserId");
                        await _academicExamDetailsManager.UpdateAsync(existingDetails);
                        updatedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
            }

            int primaryExamId = examDetails.First().ExamId;
            var redirectUrl = Url.Action("Details", new { id = primaryExamId, mergeMode = true });
            return Json(new { success = true, redirectUrl = redirectUrl, message = $"Updated: {updatedCount}, Skipped (no changes): {skippedCount}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<JsonResult> ExamMarkSubmitAjax([FromBody] AcademicExamDetail examDetail)
    {
        AcademicExamDetail existingDetails = new();
        if (examDetail != null)
        {
            try
            {
                existingDetails = await _academicExamDetailsManager.GetByIdAsync(examDetail.Id);
                if (existingDetails != null)
                {
                    if (existingDetails.ObtainMark != examDetail.ObtainMark || existingDetails.Remarks != examDetail.Remarks || existingDetails.Status != examDetail.Status)
                    {
                        examDetail.MACAddress = MACService.GetMAC();
                        examDetail.EditedAt = DateTime.Now;
                        examDetail.EditedBy = HttpContext.Session.GetString("UserId");
                        await _academicExamDetailsManager.UpdateAsync(examDetail);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        return Json(existingDetails);
    }

    [HttpPost]
    public async Task<JsonResult> UpdateExamDetailStatus([FromBody] ExamDetailStatusUpdate statusUpdate)
    {
        if (statusUpdate == null)
        {
            return Json(new { success = false, message = "Invalid data" });
        }

        var existingDetails = await _academicExamDetailsManager.GetByIdAsync(statusUpdate.Id);
        if (existingDetails != null)
        {
            existingDetails.Status = statusUpdate.Status;
            existingDetails.MACAddress = MACService.GetMAC();
            existingDetails.EditedAt = DateTime.Now;
            existingDetails.EditedBy = HttpContext.Session.GetString("UserId");
            await _academicExamDetailsManager.UpdateAsync(existingDetails);
            return Json(new { success = true, message = "Status updated" });
        }
        return Json(new { success = false, message = "Record not found" });
    }

    [HttpPost]
    public async Task<JsonResult> UpdateExamDetailRemarks([FromBody] ExamDetailRemarksUpdate remarksUpdate)
    {
        if (remarksUpdate == null)
        {
            return Json(new { success = false, message = "Invalid data" });
        }

        var existingDetails = await _academicExamDetailsManager.GetByIdAsync(remarksUpdate.Id);
        if (existingDetails != null)
        {
            existingDetails.Remarks = remarksUpdate.Remarks;
            existingDetails.MACAddress = MACService.GetMAC();
            existingDetails.EditedAt = DateTime.Now;
            existingDetails.EditedBy = HttpContext.Session.GetString("UserId");
            await _academicExamDetailsManager.UpdateAsync(existingDetails);
            return Json(new { success = true, message = "Remarks updated" });
        }
        return Json(new { success = false, message = "Record not found" });
    }

    [Authorize(Policy = "AdmitCardAcademicExamPolicy")]
    public async Task<ActionResult> AdmitCard()
    {
        ViewData["ExamType"] = new SelectList(await _examTypeManager.GetAllAsync(), "Id", "ExamTypeName");
        ViewData["AcademicClass"] = new SelectList(await _classManager.GetAllAsync(), "Id", "Name");

        return View();
    }

    [Authorize(Policy = "SeatPlanReportsPolicy")]
    public async Task<ActionResult> SeatPlan()
    {
        ViewData["ExamType"] = new SelectList(await _examTypeManager.GetAllAsync(), "Id", "ExamTypeName");
        ViewData["AcademicClass"] = new SelectList(await _classManager.GetAllAsync(), "Id", "Name");
        ViewData["Gender"] = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name");

        return View();
    }

    public async Task<JsonResult> GetExamGroupsForAdmitCard(int examTypeId, int monthId, int classId, int sectionId)
    {
        var allGroups = await _examGroupManager.GetAllAsync();
        var examGroups = allGroups
            .Where(eg => eg.AcademicExamTypeId == examTypeId
                && eg.ExamMonthId == monthId
                && (eg.AcademicExams == null || eg.AcademicExams.Any(e =>
                    e.AcademicClassId == classId
                    && (sectionId <= 0 || !e.AcademicSectionId.HasValue || e.AcademicSectionId == sectionId))))
            .OrderByDescending(eg => eg.Id)
            .Select(eg => new { eg.Id, eg.ExamGroupName })
            .ToList();

        return Json(examGroups);
    }

    [HttpPost]
    public async Task<JsonResult> UnlockExam(int exId)
    {
        string msg = string.Empty;
        var existingExam = await _examManager.GetByIdAsync(exId);
        if (existingExam != null)
        {
            //existingExam.IsActive = false;
            bool isUpdated = await _examManager.UpdateAsync(existingExam);
            if (isUpdated)
            {
                msg = "Exam is Unlocked Successfully";
            }
            else
            {
                msg = "Unloacked faild";
            }
            return Json(new { exId = exId, msg = msg });
        }
        return Json(new { msg = "Exam not found!" });
    }

    [HttpPost]
    [Authorize(Policy = "LockAcademicExamPolicy")]
    public async Task<ActionResult> LockExam(int exId)
    {
        string msg = string.Empty;
        var existingExam = await _examManager.GetByIdAsync(exId);
        if (existingExam != null)
        {
            //existingExam.IsActive = true;
            bool isUpdated = await _examManager.UpdateAsync(existingExam);
            if (isUpdated)
            {
                msg = "Exam is Locked Successfully";
            }
            else
            {
                msg = "Loacked faild";
            }
            return Json(new { exId = exId, msg = msg });
        }
        return Json(new { msg = "Exam not found!" });
    }

    public async Task<JsonResult> GetExamsByGrIdAndClassId(int examGroupId, int academicClassId)
    {
        List<AcademicExam> academicExams = (List<AcademicExam>)await _examManager.GetAllAsync();
        var results = academicExams.Where(s => s.AcademicExamGroupId == examGroupId && s.AcademicClassId == academicClassId).ToList();
        return Json(results);
    }

    public async Task<JsonResult> GetExamsByGrId(int examGroupId)
    {
        List<AcademicExam> academicExams = (List<AcademicExam>)await _examManager.GetAllAsync();
        var results = academicExams.Where(s => s.AcademicExamGroupId == examGroupId).ToList();
        return Json(results);
    }

    [HttpGet]
    [Route("api/GetAcademicClassByExamGrId")]
    [AllowAnonymous]
    public async Task<JsonResult> GetAcademicClassByExamGrId(int examGroupId)
    {
        var examGroup = await _examGroupManager.GetByIdAsync(examGroupId);
        if (examGroup?.AcademicExams == null || !examGroup.AcademicExams.Any()) 
        { 
            return Json(new List<AcademicClass>()); 
        }
        var results = examGroup.AcademicExams
            .Where(e => e.AcademicClass != null)
            .Select(e => e.AcademicClass)
            .DistinctBy(c => c.Id)
            .ToList();

        return Json(results);
    }

    [HttpGet]
    [Route("api/GetExamGroupsBySessionId")]
    [AllowAnonymous]
    public async Task<JsonResult> GetExamGroupsBySessionId(int sessionId)
    {
        try
        {
            if (sessionId <= 0)
            {
                return Json(new List<AcademicExamGroup>());
            }

            // Get all exam groups (includes AcademicSession due to repository's GetAllAsync)
            var examGroups = await _examGroupManager.GetAllAsync();
            var filtered = examGroups
                .Where(g => g.AcademicSessionId == sessionId)
                .OrderByDescending(g => g.ExamMonthId)
                .ToList();

            return Json(filtered);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet]
    [Route("api/GetAcademicSectionByExamGrId_ClassId")]
    [AllowAnonymous]
    public async Task<JsonResult> GetAcademicSectionByExamGrId_ClassId(int examGroupId, int classId)
    {
        var sections = new List<AcademicSection>();
        var exams = await _examManager.GetByClassIdExamGroupIdAsync(examGroupId, classId);
        if (exams.Count > 0)
        {
            sections = exams
                    .Select(e => e.AcademicSection ?? new AcademicSection { Id = 0, Name = "All" })
                    .DistinctBy(s => s.Id)
                    .ToList();
        }

        return Json(sections);
    }

    [HttpGet]
    [Route("api/GetExamGroupsBySession")]
    [AllowAnonymous]
    public async Task<JsonResult> GetExamGroupsBySession(int sessionId)
    {
        var examGroups = await _examGroupManager.GetAllAsync(sessionId);
        return Json(examGroups);
    }

    [HttpGet]
    [Route("api/GetSectionsByExamGroupClass")]
    [AllowAnonymous]
    public async Task<JsonResult> GetSectionsByExamGroupClass(int examGroupId, int classId, int sessionId)
    {
        var sections = new List<AcademicSection>();
        var exams = await _examManager.GetByClassIdExamGroupIdAsync(examGroupId, classId);
        if (exams.Count > 0)
        {
            sections = (List<AcademicSection>)await _academicSectionManager.GetAllByExamGroupIdClassIdSessionId(examGroupId, classId, sessionId);
            if (sections.Count > 0)
            {
                sections.Insert(0, new AcademicSection { Id = 0, Name = "All" });
            }
        }
        return Json(sections);
    }

    public async Task<JsonResult> RemoveExamDetailsFromExam(int examDetailId)
    {
        bool result = false;
        var examDetail = await _academicExamDetailsManager.GetByIdAsync(examDetailId);
        if (examDetail != null)
        {
            var ss = await _academicExamDetailsManager.RemoveAsync(examDetail);
            if (ss)
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> CheckDuplicates([FromBody] DuplicateCheckRequest request)
    {
        if (request.SectionIds == null || !request.SectionIds.Any())
        {
            var isDuplicate = await _examManager.IsDuplicateAsync(
                request.ExamGroupId,
                request.ClassId,
                request.SubjectId,
                request.SectionId,
                request.ExamCategory);
            return Json(new { isDuplicate });
        }

        var results = await _examManager.CheckDuplicatesBulkAsync(
            request.ExamGroupId,
            request.ClassId,
            request.SubjectId,
            request.ExamCategory,
            request.SectionIds);

        return Json(new { results });
    }

    public class DuplicateCheckRequest
    {
        public int ExamGroupId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int? SectionId { get; set; }
        public string ExamCategory { get; set; }
        public List<int> SectionIds { get; set; }
    }

    public class ExamDetailStatusUpdate
    {
        public int Id { get; set; }
        public bool Status { get; set; }
    }

    public class ExamDetailRemarksUpdate
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
    }
}
