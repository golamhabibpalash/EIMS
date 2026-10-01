using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.InstituteVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize(Roles = "SuperAdmin, Admin")]
public class InstitutesController : Controller
{
    private readonly IInstituteManager _instituteManager;
    private readonly IWebHostEnvironment _host;

    public InstitutesController(IInstituteManager instituteManager, IWebHostEnvironment host)
    {
        _instituteManager = instituteManager;
        _host = host;
    }

    [Authorize(Policy = "IndexInstitutesPolicy")]
    public async Task<ActionResult> Index()
    {
        var result = await _instituteManager.GetAllAsync();
        if (result.Count() != 0)
        {
            var instituteInfo = result.FirstOrDefault();
            if (instituteInfo != null)
            {
                return View(instituteInfo);
            }
        }            
        return RedirectToAction("Create");
    }


    public ActionResult Details(int id)
    {
        return View();
    }


    public ActionResult Create()
    {
        return View();
    }


    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateInstitutesPolicy")]
    public async Task<ActionResult> Create(Institute institute, IFormFile logo, IFormFile banner)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (banner != null)
                {
                    string banarName = "";
                    string root = _host.WebRootPath;
                    string folder = "Images/Institute";
                    banarName = "instituteBanner_" + Guid.NewGuid() + Path.GetExtension(banner.FileName);
                    var pathCombine = Path.Combine(root, folder, banarName);
                    using (var stream = new FileStream(pathCombine, FileMode.Create))
                    {
                        await banner.CopyToAsync(stream);
                    }
                        institute.Banner = banarName;
                }
                if (logo != null)
                {
                    string logoName = "";
                    string root = _host.WebRootPath;
                    string folder = "Images/Institute";
                    logoName = "instituteLogo_" + Guid.NewGuid() + Path.GetExtension(logo.FileName);
                    var pathCombine = Path.Combine(root, folder, logoName);
                    
                    using (var stream = new FileStream(pathCombine, FileMode.Create))
                    {
                        await logo.CopyToAsync(stream);
                    }
                    institute.Logo = logoName;
                }

                institute.CreatedAt = DateTime.Now;
                institute.CreatedBy = HttpContext.Session.GetString("UserId");

                await _instituteManager.AddAsync(institute);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        else
        {
            return View(institute);
        }
        
    }


    public async Task<ActionResult> Edit(int id)
    {
        var institute = await _instituteManager.GetByIdAsync(id);
        return View(institute);
    }


    [HttpPost,ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<ActionResult> Edit(int id,Institute existingInstitute, IFormFile logo, IFormFile banner, IFormFile fav_Icon)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // The Edit form only posts eight of the Institute's fields.
                // Binding straight into a fresh entity and saving that wiped
                // everything the form does not carry - ShortName, Phone1,
                // Phone2, Email, FavIcon and, most damaging, StartingTime /
                // ClosingTime / LateTime, which the attendance SMS window and
                // the check-in/check-out split are both derived from. Load the
                // stored row and copy the edited fields onto it instead, the
                // same way SchoolTimeTable already does.
                Institute institute = await _instituteManager.GetByIdAsync(id);
                if (institute == null)
                {
                    TempData["failed"] = "Institute not found.";
                    return RedirectToAction("Index");
                }

                institute.Name = existingInstitute.Name;
                institute.EIIN = existingInstitute.EIIN;
                institute.Slogan = existingInstitute.Slogan;
                institute.Address = existingInstitute.Address;
                institute.BranchName = existingInstitute.BranchName;
                institute.StartingTime = existingInstitute.StartingTime;
                institute.ClosingTime = existingInstitute.ClosingTime;
                institute.LateTime = existingInstitute.LateTime;

                string img = "";
                string root = _host.WebRootPath;
                string folder = "Images/Institute/";
                if (logo != null)
                {
                    string fileExt = Path.GetExtension(logo.FileName);
                    img = "instituteLogo_" + Guid.NewGuid() + fileExt;
                    string pathCombine = Path.Combine(root, folder, img);
                    using (var stream = new FileStream(pathCombine, FileMode.Create))
                    {
                        await logo.CopyToAsync(stream);
                    }
                    institute.Logo = img;
                }

                if (banner != null)
                {
                    string fileExt = Path.GetExtension(banner.FileName);
                    img = "instituteBanner_" + Guid.NewGuid() + fileExt;
                    string pathCombine = Path.Combine(root, folder, img);
                    using (var stream = new FileStream(pathCombine, FileMode.Create))
                    {
                        await banner.CopyToAsync(stream);
                    }
                    institute.Banner = img;
                }

                if (fav_Icon !=null)
                {
                    string fileExt = Path.GetExtension(fav_Icon.FileName);
                    img = "instituteFavIcon_" + Guid.NewGuid() + fileExt;
                    string pathCombine = Path.Combine(root, folder, img);
                    using (var stream = new FileStream(pathCombine, FileMode.Create))
                    {
                        await fav_Icon.CopyToAsync(stream);
                    }
                    institute.FavIcon = img;
                }
                institute.EditedAt = DateTime.Now;
                institute.EditedBy = HttpContext.Session.GetString("UserId");
                institute.MACAddress = MACService.GetMAC();
                bool isUpdate = await _instituteManager.UpdateAsync(institute);
                if (isUpdate)
                {
                    //TempData["updated"] = "Information updated successfully";
                    TempData["created"] = "Information updated successfully";
                }
                else
                {
                    TempData["failed"] = "Failed to update.";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Failed to Update";
                return View(existingInstitute);
            }
        }
        else
        {
            TempData["error"] = "Input value something wrong.";
        }
        return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
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

    [HttpGet]
    public async Task<ActionResult> SchoolTimeTable()
    {
        GlobalUI.PageTitle = "School Time Table";

        Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
        InstituteTimeVM instituteTimeVM = new InstituteTimeVM() { };
        instituteTimeVM.StartingTime = institute.StartingTime;
        instituteTimeVM.ClosingTime = institute.ClosingTime;
        instituteTimeVM.LateTimeStart = institute.LateTime;

        return View(instituteTimeVM);
    }

    [HttpPost]
    [Authorize(Policy = "SchoolTimeTableInstitutesPolicy")]
    public async Task<ActionResult> SchoolTimeTable(InstituteTimeVM model)
    {
        GlobalUI.PageTitle = "School Time Table";

        if (ModelState.IsValid)
        {
            Institute institute = await _instituteManager.GetFirstOrDefaultAsync();
            if (institute != null)
            {
                // The form uses <input type="time">, which posts 24-hour
                // "HH:mm" values, so the bound DateTimes are already correct
                // as-is. (The old code tried an AM/PM +12h adjustment here,
                // but DateTime.AddHours() returns a new value and the result
                // was discarded, so it was a silent no-op.)
                institute.StartingTime = model.StartingTime;
                institute.ClosingTime = model.ClosingTime;
                institute.LateTime = model.LateTimeStart;

                await _instituteManager.UpdateAsync(institute);
                return RedirectToAction(nameof(Index));
            }
        }
        return View();
    }


}
