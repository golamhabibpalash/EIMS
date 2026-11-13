using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.AcademicVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class AcademicSubjectsController : Controller
{
    private readonly IAcademicSubjectManager _academicSubjectManager;
    private readonly IAcademicSubjectTypeManager _academicSubjectTypeManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IQuestionFormationManager _questionFormationManager;
    private readonly IReligionManager _religionManager;
    private readonly IAcademicClassSubjectManager _classSubjectManager;

    public AcademicSubjectsController(IAcademicSubjectManager academicSubjectManger, IAcademicSubjectTypeManager academicSubjectTypeManager, IAcademicClassManager academicClassManager, ILogger<AcademicSubjectsController> _Logger, IQuestionFormationManager questionFormationManager, IReligionManager religionManager, IAcademicClassSubjectManager academicClassSubjectManager)
    {
        _academicSubjectManager = academicSubjectManger;
        _academicSubjectTypeManager = academicSubjectTypeManager;
        _academicClassManager = academicClassManager;
        _questionFormationManager = questionFormationManager;
        _religionManager = religionManager;
        _classSubjectManager = academicClassSubjectManager;
    }

    // GET: AcademicSubjects
    [Authorize(Policy = "IndexAcademicSubjectPolicy")]
    public async Task<IActionResult> Index()
    {
        ViewData["AcademicSubjectTypeId"] = new SelectList(await _academicSubjectTypeManager.GetAllAsync(), "Id", "SubjectTypeName");
        ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        ViewData["QuestionFormatId"] = new SelectList(await _questionFormationManager.GetAllAsync(), "Id", "Name");
        var academicSubject = await _academicSubjectManager.GetAllAsync();
        return View(academicSubject.OrderBy(s => s.SubjectName));
    }

    // GET: AcademicSubjects/Details/5
    [Authorize(Policy = "DetailsAcademicSubjectPolicy")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicSubject = await _academicSubjectManager.GetByIdAsync((int)id);
        if (academicSubject == null)
        {
            return NotFound();
        }

        return View(academicSubject);
    }

    // GET: AcademicSubjects/Create
    [Authorize(Policy = "CreateAcademicSubjectPolicy")]
    public async Task<IActionResult> Create()
    {
        ViewData["AcademicSubjectTypeId"] = new SelectList(await _academicSubjectTypeManager.GetAllAsync(), "Id", "SubjectTypeName");
        ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        ViewData["QuestionFormatId"] = new SelectList(await _questionFormationManager.GetAllAsync(), "Id", "Name");
        ViewData["ReligionList"] = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name");
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateAcademicSubjectPolicy")]
    public async Task<IActionResult> Create([Bind("Id,SubjectName,AcademicSubjectTypeId,AcademicClassId,SubjectCode,SubjectFor,TotalMarks,Status,CreatedBy,CreatedAt,EditedBy,EditedAt,QuestionFormatId,ReligionId")] AcademicSubject academicSubject)
    {
        academicSubject.Status = true;
        if (ModelState.IsValid)
        {
            bool isExist = await _academicSubjectManager.IsExistAsync(academicSubject);
            if (!isExist)
            {
                academicSubject.MACAddress = MACService.GetMAC();
                academicSubject.CreatedAt = DateTime.Now;
                academicSubject.CreatedBy = HttpContext.Session.GetString("UserId");
                academicSubject.SubjectFor = 's';
                var isSaved = await _academicSubjectManager.AddAsync(academicSubject);
                if (isSaved)
                {
                    TempData["created"] = "Created Successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"] = "Something wrong";
                }
            }
            else
            {
                TempData["error"] = "This Subject is already exist!";
            }
        }

        ViewData["AcademicSubjectTypeId"] = new SelectList(await _academicSubjectTypeManager.GetAllAsync(), "Id", "SubjectTypeName", academicSubject.AcademicSubjectTypeId);
        ViewData["QuestionFormatId"] = new SelectList(await _questionFormationManager.GetAllAsync(), "Id", "Name", academicSubject.QuestionFormatId);
        ViewData["ReligionList"] = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", academicSubject.ReligionId);
        return View(academicSubject);
    }

    // GET: AcademicSubjects/Edit/5
    [Authorize(Policy = "EditAcademicSubjectPolicy")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicSubject = await _academicSubjectManager.GetByIdAsync((int)id);
        if (academicSubject == null)
        {
            return NotFound();
        }
        ViewData["AcademicSubjectTypeId"] = new SelectList(await _academicSubjectTypeManager.GetAllAsync(), "Id", "SubjectTypeName", academicSubject.AcademicSubjectTypeId);
        ViewData["QuestionFormatId"] = new SelectList(await _questionFormationManager.GetAllAsync(), "Id", "Name", academicSubject.QuestionFormatId);
        ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicSubject.AcademicClassId);
        ViewData["ReligionList"] = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", academicSubject.ReligionId);
        return View(academicSubject);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditAcademicSubjectPolicy")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectName,AcademicSubjectTypeId,SubjectCode,SubjectFor,TotalMarks,Status,CreatedBy,CreatedAt,EditedBy,EditedAt,QuestionFormatId,AcademicClassId,ReligionId")] AcademicSubject academicSubject)
    {
        if (id != academicSubject.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                academicSubject.EditedAt = DateTime.Now;
                academicSubject.EditedBy = HttpContext.Session.GetString("UserId");
                academicSubject.MACAddress = MACService.GetMAC();

                TempData["updated"] = "Updated Successfully";
                await _academicSubjectManager.UpdateAsync(academicSubject);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcademicSubjectExists(academicSubject.Id))
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
        ViewData["AcademicSubjectTypeId"] = new SelectList(await _academicSubjectTypeManager.GetAllAsync(), "Id", "SubjectTypeId", academicSubject.AcademicSubjectTypeId);
        ViewData["QuestionFormatId"] = new SelectList(await _questionFormationManager.GetAllAsync(), "Id", "Name", academicSubject.QuestionFormatId);
        ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicSubject.AcademicClassId);
        return View(academicSubject);
    }

    // GET: AcademicSubjects/Delete/5
    [Authorize(Policy = "DeleteAcademicSubjectPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicSubject = await _academicSubjectManager.GetByIdAsync((int)id);
        if (academicSubject == null)
        {
            return NotFound();
        }

        return View(academicSubject);
    }

    // POST: AcademicSubjects/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteAcademicSubjectPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var academicSubject = await _academicSubjectManager.GetByIdAsync(id);

        await _academicSubjectManager.RemoveAsync(academicSubject);
        TempData["deleted"] = "Deleted Successfully";
        return RedirectToAction(nameof(Index));
    }

    
    private bool AcademicSubjectExists(int id)
    {
        var academicSubject = _academicSubjectManager.GetByIdAsync(id);
        if (academicSubject != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    
    public async Task<JsonResult> GetSubjectsByClassId(int classId)
    {
        var subjects = await _classSubjectManager.GetSubjectsByClassIdAsync(classId);
        return Json(subjects);
    }

    
    public async Task<JsonResult> GetSubjectDetailsBySubjectId(int SubjectId)
    {
        var existingSubject = await _academicSubjectManager.GetByIdAsync(SubjectId);
        return Json(existingSubject);
    }

    [Authorize(Policy = "ViewClassWiseSubjectAllocationAcademicSubjectPolicy")]
    public async Task<IActionResult> ClassWiseSubjectAllocation()
    {
        GlobalUI.PageTitle = "Class-wise Subjects";
        var subjects = await _academicSubjectManager.GetAllAsync();
        ViewData["subjectList"] = new SelectList(await _academicSubjectManager.GetAllAsync(), "Id", "SubjectName");
        ViewData["classList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        List<AcademicClassSubject> academicSubjects = (List<AcademicClassSubject>)await _classSubjectManager.GetAllAsync();
        AcademicClassSubjectAllocationVM academicClassSubjectAllocationVM = new AcademicClassSubjectAllocationVM();
        academicClassSubjectAllocationVM.academicClassSubjects = academicSubjects;
        return View(academicClassSubjectAllocationVM);
    }

    [HttpPost]
    [Authorize(Policy = "CreateClassWiseSubjectAllocationAcademicSubjectPolicy")]
    public async Task<IActionResult> ClassWiseSubjectAllocationCreate(List<AcademicClassSubject> academicClassSubjects)
    {
        int count = 0;
        if (ModelState.IsValid)
        {
            foreach (var classSubject in academicClassSubjects)
            {
                classSubject.Status = true;
                classSubject.MACAddress = MACService.GetMAC();
                classSubject.CreatedBy = HttpContext.Session.GetString("UserId");
                classSubject.CreatedAt = DateTime.Now;
                bool isSaved = await _classSubjectManager.AddAsync(classSubject);
                if (isSaved)
                {
                    count++;
                }
            }
            TempData["created"] = "Total " + count + " Subject/s Allocated successfully";
        }
        return RedirectToAction("ClassWiseSubjectAllocation");
    }
    [Authorize(Policy = "DeleteClassWiseSubjectAllocationAcademicSubjectPolicy")]
    public async Task<IActionResult> ClassWiseSubjectAllocationDelete(int id)
    {
        if (id <= 0)
        {
            TempData["deleted"] = "Sorry! Not Found";
            return RedirectToAction("ClassWiseSubjectAllocation");
        }

        try
        {
            AcademicClassSubject academicClassSubject = await _classSubjectManager.GetByIdAsync(id);
            if (academicClassSubject == null)
            {
                TempData["deleted"] = "Sorry! Not Found";
                return RedirectToAction("ClassWiseSubjectAllocation");
            }

            bool isDeleted = await _classSubjectManager.RemoveAsync(academicClassSubject);
            if (isDeleted)
            {

            }
        }
        catch (Exception)
        {

            throw;
        }

        TempData["created"] = "delete successfully";

        return RedirectToAction("ClassWiseSubjectAllocation");
    }
}
