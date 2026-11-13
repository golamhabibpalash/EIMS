using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize]
public class OffDaysController : Controller
{
    private readonly IOffDayManager _offDayManager;
    private readonly IOffDayTypeManager _offDayTypeManager;
    public OffDaysController(IOffDayManager offDayManager, IOffDayTypeManager offDayTypeManager)
    {
        _offDayManager = offDayManager;
        _offDayTypeManager = offDayTypeManager;
    }
    // GET: OffDaysControllercs
    [Authorize(Policy = "IndexOffDaysPolicy")]
    public async Task<ActionResult> Index()
    {
        var objects = await _offDayManager.GetAllAsync();
        return View(objects);
    }

    // GET: OffDaysControllercs/Details/5
    [Authorize(Policy = "DetailsOffDaysPolicy")]
    public async Task<ActionResult> Details(int id)
    {
        OffDay offDay = await _offDayManager.GetByIdAsync(id);
        return View(offDay);
    }

    // GET: OffDaysControllercs/Create
    [Authorize(Policy = "CreateOffDaysPolicy")]
    public async Task<ActionResult> Create()
    {
        ViewBag.OffDayType = new SelectList(await _offDayTypeManager.GetAllAsync(), "Id", "OffDayTypeName");
        return View();
    }

    // POST: OffDaysControllercs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateOffDaysPolicy")]
    public async Task<ActionResult> Create(OffDay offDay)
    {
        ViewBag.OffDayType = new SelectList(await _offDayTypeManager.GetAllAsync(), "Id", "OffDayTypeName");
        if (ModelState.IsValid)
        {
            try
            {
                OffDay newOffday = new OffDay()
                {
                    OffDayName = offDay.OffDayName,
                    OffDayStartingDate = offDay.OffDayStartingDate,
                    OffDayEndDate = offDay.OffDayEndDate.ToString("yyyy-MM-dd") == "0001-01-01" ? offDay.OffDayStartingDate : offDay.OffDayEndDate,
                    OffDayTypeId = offDay.OffDayTypeId,
                    MACAddress = MACService.GetMAC(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = HttpContext.Session.GetString("UserId"),
                    Description = offDay.Description,
                    TotalDays = 1
                };
                if (offDay.OffDayEndDate.ToString("yyyy-MM-dd") == "0001-01-01")
                {
                    newOffday.TotalDays = 1;
                }
                else
                {
                    newOffday.TotalDays = (offDay.OffDayEndDate.AddDays(1) - offDay.OffDayStartingDate).Days;
                }
                bool isSaved = await _offDayManager.AddAsync(newOffday);
                if (isSaved)
                {
                    ViewData["created"] = "Created Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View();
            }
        }

        TempData["failed"] = "Failed";
        return View(offDay);

    }

    // GET: OffDaysControllercs/Edit/5
    [HttpGet]
    [Authorize(Policy = "EditOffDaysPolicy")]
    public async Task<ActionResult> Edit(int id)
    {
        OffDay offDay = await _offDayManager.GetByIdAsync(id);
        if (offDay != null)
        {
            ViewBag.OffDayType = new SelectList(await _offDayTypeManager.GetAllAsync(), "Id", "OffDayTypeName", offDay.OffDayType);
            return View(offDay);
        }
        TempData["failed"] = "Not Found";
        return View();
    }

    // POST: OffDaysControllercs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditOffDaysPolicy")]
    public async Task<ActionResult> Edit(int id, OffDay offDay)
    {
        if (ModelState.IsValid)
        {
            try
            {
                OffDay existingOffDay = await _offDayManager.GetByIdAsync(id);
                existingOffDay.OffDayName = offDay.OffDayName;
                existingOffDay.OffDayTypeId = offDay.OffDayTypeId;
                existingOffDay.OffDayStartingDate = offDay.OffDayStartingDate;
                existingOffDay.OffDayEndDate = offDay.OffDayEndDate;
                int totalDays = (offDay.OffDayEndDate - offDay.OffDayStartingDate).Days;
                existingOffDay.TotalDays = totalDays <= 0 ? 1 : totalDays;                    
                existingOffDay.MACAddress = MACService.GetMAC();
                existingOffDay.EditedAt = DateTime.Now;
                existingOffDay.EditedBy = HttpContext.Session.GetString("UserId");
                existingOffDay.Description = offDay.Description;
                bool isUpdated = await _offDayManager.UpdateAsync(existingOffDay);
                if (isUpdated)
                {
                    TempData["created"] = "Updated Successful";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View();
            }
        }
        TempData["failed"] = "Not Found";
        return View(offDay);
    }

    // GET: OffDaysControllercs/Delete/5
    [Authorize(Policy = "DeleteOffDaysPolicy")]
    public async Task<ActionResult> Delete(int id)
    {
        OffDay oDay = await _offDayManager.GetByIdAsync(id);
        return View(oDay);
    }

    // POST: OffDaysControllercs/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteOffDaysPolicy")]
    public async Task<ActionResult> Delete(int id, OffDay offDay)
    {
        try
        {
            bool isDeleted = await _offDayManager.RemoveAsync(offDay);
            if (isDeleted)
            {
                TempData["deleted"] = "Off day is deleted";
                return RedirectToAction(nameof(Index));
            }
            else
            {

                TempData["failed"] = "Failed to delete";
                return View(offDay);
            }

        }
        catch
        {
            return View(offDay);
        }
    }
}
