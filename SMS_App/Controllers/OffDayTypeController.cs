using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize]
public class OffDayTypeController : Controller
{
    private readonly IOffDayTypeManager _offDayTypeManager;
    public OffDayTypeController(IOffDayTypeManager offDayTypeManager)
    {
        _offDayTypeManager = offDayTypeManager;
    }
    // GET: OffDayTypeController
    [Authorize(Policy = "IndexOffDayTypesPolicy")]
    public async Task<ActionResult> Index()
    {
        string msg = string.Empty;
        //if (!string.IsNullOrEmpty(TempData["updated"].ToString()))
        //{
        //    ViewBag.msg = msg = TempData["updated"].ToString();
        //}
        //if (!string.IsNullOrEmpty(TempData["created"].ToString()))
        //{
        //    ViewBag.msg = msg = TempData["created"].ToString();
        //}
        //if (!string.IsNullOrEmpty(TempData["deleted"].ToString()))
        //{
        //    ViewBag.msg = msg = TempData["deleted"].ToString();
        //}
        var result = await _offDayTypeManager.GetAllAsync();
        return View(result);
    }

    // GET: OffDayTypeController/Details/5
    [Authorize(Policy = "DetailsOffDayTypesPolicy")]
    public async Task<ActionResult> Details(int id)
    {
        var result = await _offDayTypeManager.GetByIdAsync(id);
        return View(result);
    }

    // GET: OffDayTypeController/Create
    [Authorize(Policy = "CreateOffDayTypesPolicy")]
    public ActionResult Create()
    {
        return View();
    }

    // POST: OffDayTypeController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateOffDayTypesPolicy")]
    public async Task<ActionResult> Create(OffDayType offDayType)
    {
        try
        {
            var newObject = new OffDayType();
            newObject.OffDayTypeName = offDayType.OffDayTypeName;
            newObject.Remarks = offDayType.Remarks;
            newObject.MACAddress = MACService.GetMAC();
            newObject.CreatedAt = DateTime.Now;
            newObject.CreatedBy = HttpContext.Session.GetString("UserId");
            bool isAdded = await _offDayTypeManager.AddAsync(newObject);
            if (isAdded)
            {
                TempData["created"] = "New Off Day Type created";                    
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            return View(offDayType);
        }
        return View(offDayType);
    }

    // GET: OffDayTypeController/Edit/5
    [Authorize(Policy = "EditOffDayTypesPolicy")]
    public async Task<ActionResult> Edit(int id)
    {
        OffDayType offDayType = await _offDayTypeManager.GetByIdAsync(id);
        return View(offDayType);
    }

    // POST: OffDayTypeController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditOffDayTypesPolicy")]
    public async Task<ActionResult> Edit(int id, OffDayType offDayType)
    {
        if (offDayType.Id != id)
        {
            return RedirectToAction(nameof(Index));
        }
        try
        {
            OffDayType existingOffDayType = await _offDayTypeManager.GetByIdAsync(id);
            if (offDayType.OffDayTypeName != existingOffDayType.OffDayTypeName || offDayType.Remarks != existingOffDayType.Remarks) 
            {
                existingOffDayType.OffDayTypeName = offDayType.OffDayTypeName;
                existingOffDayType.Remarks = offDayType.Remarks;
                existingOffDayType.EditedAt = DateTime.Now;
                existingOffDayType.EditedBy = HttpContext.Session.GetString("UserId");
                bool isUpdated = await _offDayTypeManager.UpdateAsync(existingOffDayType);
                if (isUpdated)
                {
                    TempData["updated"] = "Off Day Type Name Updated Successfully";
                }
                else
                {
                    TempData["failed"] = "Failed to update";
                    return View(existingOffDayType);
                }
            }
            else
            {
                TempData[""] = "Change not found";
            }
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["failed"] = "Failed to update";
            return View(offDayType);
        }
    }

    // GET: OffDayTypeController/Delete/5
    [Authorize(Policy = "DeleteOffDayTypesPolicy")]
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: OffDayTypeController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteOffDayTypesPolicy")]
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
}
