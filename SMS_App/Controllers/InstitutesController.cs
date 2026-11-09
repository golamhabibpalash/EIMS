using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<InstitutesController> _logger;

    private const string ImageFolder = "Images/Institute";
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico", ".bmp" };

    public InstitutesController(IInstituteManager instituteManager, IWebHostEnvironment host, ILogger<InstitutesController> logger)
    {
        _logger = logger;
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
        // There is one institute row; /Institutes/Edit without an id used to load "institute 0",
        // get null and crash the view. Fall back to the stored institute instead.
        var institute = id > 0 ? await _instituteManager.GetByIdAsync(id) : null;
        institute ??= (await _instituteManager.GetAllAsync()).FirstOrDefault();
        if (institute == null)
        {
            return RedirectToAction(nameof(Create));
        }
        return View(institute);
    }

    /// <summary>
    /// Saves an uploaded institute image under wwwroot/Images/Institute and returns the new
    /// file name, or an error message the user can act on.
    /// </summary>
    private async Task<(string fileName, string error)> SaveInstituteImageAsync(IFormFile file, string prefix, string label)
    {
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (!AllowedImageExtensions.Contains(ext))
        {
            return (null, $"{label} must be an image file ({string.Join(", ", AllowedImageExtensions)}).");
        }
        if (file.Length == 0)
        {
            return (null, $"The selected {label.ToLowerInvariant()} file is empty.");
        }
        if (file.Length > MaxImageBytes)
        {
            return (null, $"{label} is too large ({file.Length / 1024 / 1024.0:0.#} MB). Please use an image under 5 MB.");
        }

        var fileName = prefix + Guid.NewGuid() + ext;
        var directory = Path.Combine(_host.WebRootPath, ImageFolder);
        try
        {
            Directory.CreateDirectory(directory);
            await using var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return (fileName, null);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No write permission for institute image folder {Directory}", directory);
            return (null, $"The server does not allow saving the {label.ToLowerInvariant()} (no write permission on {ImageFolder}). Ask the hosting provider to give the site write access to that folder.");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Could not write institute image to {Directory}", directory);
            return (null, $"The {label.ToLowerInvariant()} could not be saved on the server. Please try again.");
        }
    }


    [HttpPost,ValidateAntiForgeryToken]
    [Authorize(Policy = "EditInstitutesPolicy")]
    public async Task<ActionResult> Edit(int id,Institute existingInstitute, IFormFile logo, IFormFile banner, IFormFile fav_Icon)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // The Edit modal on Index posts every scalar field below
                // (Name, ShortName, EIIN, phones, Email, Slogan, Address,
                // BranchName, the three times). Binding straight into a fresh
                // entity and saving that used to wipe them; load the stored
                // row and copy the edited fields onto it instead, the same
                // way SchoolTimeTable already does. Every field the modal
                // posts must appear here, otherwise it silently stops saving.
                Institute institute = await _instituteManager.GetByIdAsync(id);
                if (institute == null)
                {
                    TempData["error"] = "Institute not found.";
                    return RedirectToAction("Index");
                }

                institute.Name = existingInstitute.Name;
                institute.ShortName = existingInstitute.ShortName;
                institute.EIIN = existingInstitute.EIIN;
                institute.Phone1 = existingInstitute.Phone1;
                institute.Phone2 = existingInstitute.Phone2;
                institute.Email = existingInstitute.Email;
                institute.Slogan = existingInstitute.Slogan;
                institute.Address = existingInstitute.Address;
                institute.BranchName = existingInstitute.BranchName;
                institute.StartingTime = existingInstitute.StartingTime;
                institute.ClosingTime = existingInstitute.ClosingTime;
                institute.LateTime = existingInstitute.LateTime;

                var uploads = new (IFormFile file, string prefix, string label, Action<string> apply)[]
                {
                    (logo, "instituteLogo_", "Logo", f => institute.Logo = f),
                    (banner, "instituteBanner_", "Banner", f => institute.Banner = f),
                    (fav_Icon, "instituteFavIcon_", "Title icon", f => institute.FavIcon = f),
                };
                foreach (var upload in uploads)
                {
                    if (upload.file == null) continue;
                    var (fileName, error) = await SaveInstituteImageAsync(upload.file, upload.prefix, upload.label);
                    if (error != null)
                    {
                        // Nothing has been saved to the database yet, so stop here.
                        TempData["error"] = error;
                        return RedirectToAction(nameof(Index));
                    }
                    upload.apply(fileName);
                }

                institute.EditedAt = DateTime.Now;
                institute.EditedBy = HttpContext.Session.GetString("UserId");
                institute.MACAddress = MACService.GetMAC();
                bool isUpdate = await _instituteManager.UpdateAsync(institute);
                if (isUpdate)
                {
                    TempData["created"] = "Information updated successfully";
                }
                else
                {
                    TempData["error"] = "Failed to update the institute information. Please try again.";
                }
            }
            catch (Exception ex)
            {
                // Used to render the Edit view from this POST, which then broke on its own
                // (see Edit GET) and hid the real error. Log it and go back to the page.
                _logger.LogError(ex, "Updating institute {InstituteId} failed", id);
                TempData["error"] = "Failed to update the institute information. The error has been logged.";
            }
        }
        else
        {
            var problems = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => e.Value.Errors.First().ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct();
            TempData["error"] = "Please check the form: " + string.Join(" ", problems);
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
