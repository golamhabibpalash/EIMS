using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.Testimonials;

namespace SMS_App.Controllers;

[Authorize]
public class TestimonialsController : Controller
{
    private readonly ITestimonialManager _testimonialManager;
    private readonly IStudentManager _studentManager;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _host;

    public TestimonialsController(ITestimonialManager testimonialManager, IStudentManager studentManager,
        IMapper mapper, IWebHostEnvironment host)
    {
        _testimonialManager = testimonialManager;
        _studentManager = studentManager;
        _mapper = mapper;
        _host = host;
    }

    #region List / Details

    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> Index()
    {
        var items = await _testimonialManager.GetAllWithStudentAsync();
        return View(items);
    }

    /// <summary>Read-only list of the testimonials that are active AND published.</summary>
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> Published()
    {
        var items = await _testimonialManager.GetPublishedAsync();
        return View(items);
    }

    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _testimonialManager.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    /// <summary>Partial list of a single student's testimonials, loaded into the student profile tab.</summary>
    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexTestimonialsPolicy")]
    public async Task<IActionResult> ByStudent(int studentId)
    {
        var items = await _testimonialManager.GetAllByStudentIdAsync(studentId);
        ViewBag.StudentId = studentId;
        return PartialView("_StudentTestimonials", items);
    }

    #endregion

    #region Create

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateTestimonialsPolicy")]
    public async Task<IActionResult> Create(int? studentId)
    {
        var vm = new TestimonialCreateVM
        {
            StudentId = studentId,
            StudentList = await BuildStudentSelectAsync()
        };
        if (studentId != null)
        {
            var student = await _studentManager.GetByIdAsync(studentId.Value);
            if (student != null)
            {
                vm.StudentName = student.Name;
                vm.Designation = student.AcademicClass?.Name;
            }
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateTestimonialsPolicy")]
    public async Task<IActionResult> Create(TestimonialCreateVM vm, IFormFile photoFile)
    {
        if (!ModelState.IsValid)
        {
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        await ValidateStudentReference(vm.StudentId);
        if (!ModelState.IsValid)
        {
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        var entity = _mapper.Map<Testimonial>(vm);
        entity.Photo = await SavePhotoAsync(photoFile);
        StampCreate(entity);

        var saved = await _testimonialManager.AddAsync(entity);
        if (!saved)
        {
            ModelState.AddModelError("", "Could not save the testimonial. Please try again.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        TempData["saved"] = "Testimonial created successfully.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Edit

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditTestimonialsPolicy")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _testimonialManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        var vm = _mapper.Map<TestimonialEditVM>(entity);
        vm.StudentList = await BuildStudentSelectAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditTestimonialsPolicy")]
    public async Task<IActionResult> Edit(int id, TestimonialEditVM vm, IFormFile photoFile)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        var entity = await _testimonialManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        await ValidateStudentReference(vm.StudentId);
        if (!ModelState.IsValid)
        {
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        // Update only the testimonial's own fields; never touch the student record.
        entity.StudentId = vm.StudentId;
        entity.StudentName = vm.StudentName;
        entity.Designation = vm.Designation;
        entity.Content = vm.Content;
        entity.DisplayOrder = vm.DisplayOrder;
        entity.Status = vm.Status;
        entity.IsPublished = vm.IsPublished;

        var newPhoto = await SavePhotoAsync(photoFile);
        if (newPhoto != null) entity.Photo = newPhoto;

        StampEdit(entity);

        var updated = await _testimonialManager.UpdateAsync(entity);
        if (!updated)
        {
            ModelState.AddModelError("", "Could not update the testimonial. Please try again.");
            vm.StudentList = await BuildStudentSelectAsync();
            return View(vm);
        }

        TempData["edited"] = "Testimonial updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Publish / Delete

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "PublishTestimonialsPolicy")]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var entity = await _testimonialManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.IsPublished = !entity.IsPublished;
        StampEdit(entity);
        await _testimonialManager.UpdateAsync(entity);

        TempData["edited"] = entity.IsPublished ? "Testimonial published." : "Testimonial unpublished.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "DeleteTestimonialsPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _testimonialManager.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "DeleteTestimonialsPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _testimonialManager.GetByIdAsync(id);
        if (entity == null) return NotFound();

        var removed = await _testimonialManager.RemoveAsync(entity);
        if (removed) TempData["deleted"] = "Testimonial deleted.";
        else TempData["failed"] = "Could not delete the testimonial.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Helpers

    private async Task<IEnumerable<SelectListItem>> BuildStudentSelectAsync()
    {
        var students = await _studentManager.GetAllAsync();
        return (students ?? Enumerable.Empty<Student>())
            .Where(s => s.Status)
            .OrderBy(s => s.AcademicClass?.ClassSerial).ThenBy(s => s.ClassRoll)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.UniqueId} / Roll {s.ClassRoll})"
            }).ToList();
    }

    /// <summary>Adds a model error when a supplied student id does not resolve to a real student.</summary>
    private async Task ValidateStudentReference(int? studentId)
    {
        if (studentId == null) return;
        var student = await _studentManager.GetByIdAsync(studentId.Value);
        if (student == null)
            ModelState.AddModelError(nameof(TestimonialCreateVM.StudentId), "The selected student does not exist.");
    }

    /// <summary>Saves an optional testimonial photo; returns the new filename or null if none uploaded.</summary>
    private async Task<string> SavePhotoAsync(IFormFile photoFile)
    {
        if (photoFile == null || photoFile.Length == 0) return null;

        var allowed = new[] { ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Only .jpg, .jpeg and .png photos are allowed.");
        if (photoFile.Length > 1 * 1024 * 1024)
            throw new InvalidOperationException("Photo must not exceed 1 MB.");

        var folder = Path.Combine(_host.WebRootPath, "Images", "Testimonial");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"T_{Guid.NewGuid():N}{ext}";
        await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
        await photoFile.CopyToAsync(stream);
        return fileName;
    }

    private void StampCreate(Testimonial e)
    {
        var userId = HttpContext.Session.GetString("UserId");
        e.CreatedBy = userId; e.CreatedAt = DateTime.Now;
        e.EditedBy = userId; e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }

    private void StampEdit(Testimonial e)
    {
        e.EditedBy = HttpContext.Session.GetString("UserId");
        e.EditedAt = DateTime.Now;
        e.MACAddress = MACService.GetMAC();
    }

    #endregion
}
