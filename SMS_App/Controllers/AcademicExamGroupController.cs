using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.AcademicVM;
using SMS_App.ViewModels.ExamVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class AcademicExamGroupController : Controller
{
    private readonly IAcademicExamGroupManager _examGroupManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IAcademicExamTypeManager _academicExamTypeManager;
    private readonly IAcademicExamManager _academicExamManager;
    private readonly IMapper _mapper;
    private readonly IExamResultManager _examResultManager;
    public AcademicExamGroupController(IAcademicExamGroupManager examGroupManager, IAcademicSessionManager academicSessionManager, IAcademicExamTypeManager academicExamTypeManager, IAcademicExamManager academicExamManager, IMapper mapper, IExamResultManager examResultManager)
    {
        _examGroupManager = examGroupManager;
        _academicSessionManager = academicSessionManager;
        _academicExamTypeManager = academicExamTypeManager;
        _academicExamManager = academicExamManager;
        _mapper = mapper;
        _examResultManager = examResultManager;

    }
    // GET: AcademicExamGroupController
    [Authorize(Policy = "IndexAcademicExamGroupPolicy")]
    public async Task<ActionResult> Index()
    {
        AcademicExamGroupVM academicExamGroupVM = new();
        try
        {
            var examGroupList = await _examGroupManager.GetAllAsync();

            var examGroupAndClassPairs = examGroupList
                .SelectMany(item => item.AcademicExams
                    .Select(exa => (ExamGroupId: item.Id, ClassId: exa.AcademicClassId)))
                .Distinct()
                .ToList();

            var resultStatusDict = await _examResultManager.GetResultProcessedStatusBulkAsync(examGroupAndClassPairs);

            foreach (var item in examGroupList)
            {
                AcademicExamGroupIndexVM e = _mapper.Map<AcademicExamGroupIndexVM>(item);
                e.NumberOfProcessResults = item.AcademicExams
                    .Count(exa => resultStatusDict.GetValueOrDefault((item.Id, exa.AcademicClassId), false));
                academicExamGroupVM.AcademicExamGroupIndexVMList.Add(e);
            }

            // ✅ Sequential await instead of Task.WhenAll
            var sessions = await _academicSessionManager.GetAllAsync();
            var examTypes = await _academicExamTypeManager.GetAllAsync();

            academicExamGroupVM.AcademicSessionList = new SelectList(sessions, "Id", "Name").ToList();
            academicExamGroupVM.ExamTypeList = new SelectList(examTypes, "Id", "ExamTypeName").ToList();
        }
        catch (Exception)
        {
            throw;
        }

        return View(academicExamGroupVM);
    }

    // GET: AcademicExamGroupController/Details/5
    [Authorize(Policy = "DetailsAcademicExamGroupPolicy")]
    public async Task<ActionResult> Details(int id)
    {
        if (TempData["failed"] != null)
        {
            TempData["failed"] = TempData["failed"].ToString();
        }
        if (TempData["success"] != null)
        {
            TempData["created"] = TempData["success"].ToString();
        }
        ViewBag.ScrollPosition = HttpContext.Session.GetInt32("ScrollPosition");
        if (ViewBag.ScrollPosition == null)
        {
            ViewBag.ScrollPosition = 0;
        }
        GlobalUI.PageTitle = "Exam Group Details";

        AcademicExamGroup academicExamGroup = await _examGroupManager.GetByIdAsync(id);
        AcademicExamGroupIndexVM academicExamGroupIndexVM = _mapper.Map<AcademicExamGroupIndexVM>(academicExamGroup);
        return View(academicExamGroupIndexVM);
    }

    // POST: AcademicExamGroupController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateAcademicExamGroupPolicy")]
    public async Task<ActionResult> Create(AcademicExamGroup academicExamGroup)
    {
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Something wrong";
            AcademicExamGroupVM academicExamGroupVM = new AcademicExamGroupVM();
            //academicExamGroupVM.AcademicExamGroups = (List<AcademicExamGroup>)await _examGroupManager.GetAllAsync();
            academicExamGroupVM.ExamGroupName = academicExamGroup.ExamGroupName;
            academicExamGroupVM.AcademicExamTypeId = academicExamGroup.AcademicExamTypeId;
            academicExamGroupVM.ExamMonthId = academicExamGroup.ExamMonthId;
            return RedirectToAction("index");
        }

        ViewData["AcademicSessionList"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicExamGroup.AcademicSessionId);
        ViewData["AcademicExamTypeList"] = new SelectList(await _academicExamTypeManager.GetAllAsync(), "Id", "ExamTypeName", academicExamGroup.AcademicExamTypeId);

        var allExamGroup = await _examGroupManager.GetAllAsync();
        AcademicExamGroup existingExamGroup = allExamGroup.FirstOrDefault(s => s.ExamGroupName == academicExamGroup.ExamGroupName && s.AcademicSessionId == academicExamGroup.AcademicSessionId && s.AcademicExamTypeId == academicExamGroup.AcademicExamTypeId);
        try
        {
            if (existingExamGroup != null)
            {
                TempData["error"] = "This Exam Group is already exist.";
                return RedirectToAction("index", academicExamGroup);
            }
            if (string.IsNullOrEmpty(academicExamGroup.ExamGroupName))
            {
                TempData["error"] = "Group Name should not empty.";
                return RedirectToAction("index", academicExamGroup);
            }
            academicExamGroup.CreatedAt = DateTime.Now;
            academicExamGroup.CreatedBy = HttpContext.Session.GetString("UserId");
            academicExamGroup.MACAddress = MACService.GetMAC();
            bool isSaved = await _examGroupManager.AddAsync(academicExamGroup);
            if (!isSaved)
            {
                TempData["error"] = "Something during save time";
            }
            else
            {
                TempData["created"] = "Alhamdulillah! Exam Group Successfully Created";
            }
        }
        catch
        {
            throw;
        }

        return RedirectToAction("Index");
    }

    // GET: AcademicExamGroupController/Edit/5
    [Authorize(Policy = "EditAcademicExamGroupPolicy")]
    public async Task<ActionResult> Edit(int id)
    {
        AcademicExamGroup academicExamGroup = await _examGroupManager.GetByIdAsync(id);
        if (academicExamGroup != null)
        {
            ViewData["AcademicSessionList"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicExamGroup.AcademicSessionId);
            ViewData["AcademicExamTypeList"] = new SelectList(await _academicExamTypeManager.GetAllAsync(), "Id", "ExamTypeName", academicExamGroup.AcademicExamTypeId);
            return View(academicExamGroup);
        }
        else
        {
            TempData["error"] = "Data not found";
            return RedirectToAction("Index");
        }
    }

    // POST: AcademicExamGroupController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditAcademicExamGroupPolicy")]
    public async Task<ActionResult> Edit(int id, AcademicExamGroup academicExamGroup)
    {
        if (id != academicExamGroup.Id)
        {
            TempData["error"] = "Data Not matched";
            return View(academicExamGroup);
        }

        var allExamGroup = await _examGroupManager.GetAllAsync();
        AcademicExamGroup existingExamGroup = allExamGroup.FirstOrDefault(s => s.ExamGroupName == academicExamGroup.ExamGroupName && s.AcademicSessionId == academicExamGroup.AcademicSessionId && s.AcademicExamTypeId == academicExamGroup.AcademicExamTypeId);

        ViewData["AcademicSessionList"] = new SelectList(await _academicSessionManager.GetAllAsync(), "Id", "Name", academicExamGroup.AcademicSessionId);
        ViewData["AcademicExamTypeList"] = new SelectList(await _academicExamTypeManager.GetAllAsync(), "Id", "ExamTypeName", academicExamGroup.AcademicExamTypeId);
        try
        {
            if (existingExamGroup != null && id != existingExamGroup.Id)
            {
                TempData["error"] = "This Exam Group is already exist.";
                return RedirectToAction("index");
            }
            academicExamGroup.EditedAt = DateTime.Now;
            academicExamGroup.EditedBy = HttpContext.Session.GetString("UserId");
            academicExamGroup.MACAddress = MACService.GetMAC();
            bool isUpdated = await _examGroupManager.UpdateAsync(academicExamGroup);
            if (isUpdated)
            {
                TempData["updated"] = "Exam Group Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Update failed. Something wrong";
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = "Exception: " + ex.Message;
            return RedirectToAction("Index");
        }
        return RedirectToAction("index");
    }

    // GET: AcademicExamGroupController/Delete/5
    [HttpPost]
    [Authorize(Policy = "DeleteAcademicExamGroupPolicy")]
    public async Task<ActionResult> Delete(int id)
    {
        AcademicExamGroup academicExamGroup = await _examGroupManager.GetByIdAsync(id);
        try
        {
            if (academicExamGroup != null)
            {
                var examList = await _academicExamManager.GetAllAsync();
                if (examList != null)
                {
                    AcademicExam existingExam = examList.FirstOrDefault(s => s.AcademicExamGroupId == id);
                    if (existingExam != null)
                    {
                        TempData["error"] = "Existing Exam available in this group.";
                        return RedirectToAction("index");
                    }
                }
                bool isDeleted = await _examGroupManager.RemoveAsync(academicExamGroup);
                if (isDeleted)
                {
                    TempData["created"] = "Deleted Successfully.";
                }
                else
                {
                    TempData["error"] = "Failed! Something wrong.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = "Exception: " + ex.Message;
        }

        return RedirectToAction("index");
    }

    public async Task<JsonResult> GetExamGroups(int monthId, int examTypeId)
    {
        List<AcademicExamGroup> examGroup = new List<AcademicExamGroup>();
        try
        {
            examGroup = (List<AcademicExamGroup>)await _examGroupManager.GetByMonthExamType(monthId, examTypeId);
        }
        catch (Exception)
        {

            throw;
        }
        return Json(examGroup);
    }

    public async Task<JsonResult> GetExamGroupsBySession(int sessionId)
    {
        var examGroup = new List<AcademicExamGroup>();
        try
        {
            examGroup = (List<AcademicExamGroup>)await _examGroupManager.GetBySession(sessionId);
        }
        catch (Exception)
        {
            throw;
        }
        return Json(examGroup);
    }
}
