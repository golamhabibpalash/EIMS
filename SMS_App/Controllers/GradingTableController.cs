using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.GradingTable;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class GradingTableController(IGradingTableManager gradingTableManager) : Controller
{
    private readonly IGradingTableManager _gradingTableManager = gradingTableManager;

    [Authorize(Policy = "IndexGradingTablePolicy")]
    public async Task<IActionResult> Index()
    {
        List<GradingIndexVM> gradingIndexVMs = new List<GradingIndexVM>();
        List<GradingTable> gradingTables = (List<GradingTable>) await _gradingTableManager.GetAllAsync();
        if (gradingTables != null)
        {
            foreach (var item in gradingTables)
            {
                GradingIndexVM gradingIndexVM = new GradingIndexVM();
                gradingIndexVM.NumberRange = item.NumberRangeMin + " - " + item.NumberRangeMax;
                gradingIndexVM.LetterGrade = item.LetterGrade;
                gradingIndexVM.GradePoint = item.GradePoint;
                gradingIndexVM.Id = item.Id;
                gradingIndexVM.GradeComment = item.gradeComments;
                gradingIndexVMs.Add(gradingIndexVM);
            }
        }

        return View(gradingIndexVMs.OrderByDescending(s => s.GradePoint));
    }
    
    [Authorize(Policy = "DetailsGradingTablePolicy")]
    public IActionResult Details()
    {
        return View();
    }
    
    [Authorize(Policy = "CreateGradingTablePolicy")]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [Authorize(Policy = "CreateGradingTablePolicy")]
    public async Task<IActionResult> Create(GradingTable obj)
    {
        if (ModelState.IsValid)
        {
            obj.CreatedAt = DateTime.Now;
            obj.CreatedBy = HttpContext.Session.GetString("UserId");
            obj.MACAddress = MACService.GetMAC();
            var IsValidObj = await ValidateGradingTableObject(obj,"create");
            if (IsValidObj.isValid)
            {
                bool isSaved = await _gradingTableManager.AddAsync(obj);
                if (isSaved)
                {
                    TempData["created"] = "Grading Row Created Successfully.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["failed"] = IsValidObj.eMsg;
            }
        }
        return View(obj);
    }
    
    [HttpGet]
    [Authorize(Policy = "EditGradingTablePolicy")]
    public async Task<IActionResult> Edit(int id)
    {
        GradingTable gradingTable = await _gradingTableManager.GetByIdAsync(id);
        if (gradingTable == null)
        {
            TempData["error"] = "Item not found";
            return RedirectToAction("Index");
        }

        return View(gradingTable);
    }
    
    [HttpPost]
    [Authorize(Policy = "EditGradingTablePolicy")]
    public async Task<IActionResult> Edit(GradingTable obj)
    {
        if (ModelState.IsValid)
        {
            obj.EditedAt = DateTime.Now;
            obj.EditedBy = HttpContext.Session.GetString("UserId");
            obj.MACAddress = MACService.GetMAC();
            var IsValidObj = await ValidateGradingTableObject(obj,"edit");
            if (IsValidObj.isValid)
            {
                bool isUpdated = await _gradingTableManager.UpdateAsync(obj);
                if (isUpdated)
                {
                    TempData["created"] = "Grading Row Updated Successfully.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["failed"] = IsValidObj.eMsg;
            }
        }
        return View(obj);
    }

    [Authorize(Policy = "DeleteGradingTablePolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _gradingTableManager.GetByIdAsync(id);
        if (s != null)
        {
            TempData["Id"] = id;
        }
        return View(s);
    }
    
    [HttpPost]
    [Authorize(Policy = "DeleteGradingTablePolicy")]
    public async Task<IActionResult> DeleteConfirm(int id, string dVal)
    {
        bool isDelete = true;
        if (id.ToString() != TempData["Id"].ToString())
        {
            isDelete = false;
        }
        if (dVal != "Delete")
        {
            isDelete = false;
        }
        if (isDelete)
        {
            GradingTable existingGradingTable = await _gradingTableManager.GetByIdAsync(id);
            if (existingGradingTable != null)
            {
                bool isSuccess = await _gradingTableManager.RemoveAsync(existingGradingTable);
                if (isSuccess)
                {
                    TempData["deleted"] = "Item has been deleted";
                }
                else
                {
                    TempData["failed"] = "Delete operation failed";
                }
            }
            else
            {
                TempData["failed"] = "Item not found";
            }
        }
        return RedirectToAction("Index");
    }
    private async Task<(bool isValid, string eMsg)> ValidateGradingTableObject(GradingTable obj, string action)
    {
        bool isValid = true;
        string msg = string.Empty;
        if (obj.NumberRangeMin > obj.NumberRangeMax)
        {
            msg = "Max number should be greater than Min Number";
            isValid = false;
            return new(isValid, msg);
        }
        var allGradings = await _gradingTableManager.GetAllAsync();

        
        foreach (var grading in allGradings)
        {
            if (obj.Id != grading.Id)
            {
                for (int i = grading.NumberRangeMin; i <= grading.NumberRangeMax; i++)
                {
                    if (i == obj.NumberRangeMin || i == obj.NumberRangeMax)
                    {
                        isValid = false;
                        msg = "This number (" + i + ") is already used.";
                        break;
                    }
                }
                if (grading.LetterGrade == obj.LetterGrade)
                {
                    isValid = false;
                    msg = "Same Letter grade (" + grading.LetterGrade + ") is already Exist";
                    break;
                }
                if (grading.GradePoint == obj.GradePoint)
                {
                    isValid = false;
                    msg = "Same Grade Point (" + grading.GradePoint + ") is already Exist";
                    break;
                }
            }
        }
        
        return new(isValid, msg);
    }
}
