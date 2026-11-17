using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMS_App.MultiTenancy;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class MobileContentController : Controller
    {
        private readonly IMobileNoticeManager _noticeManager;
        private readonly IMobileSliderManager _sliderManager;
        private readonly ITenantFileStorage _tenantFiles;

        private const string SliderFolder = "Images/MobileSlider";
        private const long MaxImageBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

        public MobileContentController(
            IMobileNoticeManager noticeManager,
            IMobileSliderManager sliderManager,
            ITenantFileStorage tenantFiles)
        {
            _noticeManager = noticeManager;
            _sliderManager = sliderManager;
            _tenantFiles = tenantFiles;
        }

        public async Task<IActionResult> Notices()
        {
            var items = await _noticeManager.GetAllAsync();
            return View(items.OrderBy(n => n.DisplayOrder).ThenByDescending(n => n.CreatedAt));
        }

        public IActionResult CreateNotice()
        {
            return View(new MobileNotice { IsPublished = true, DisplayOrder = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotice(MobileNotice model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var now = DateTime.Now;
            var notice = new MobileNotice
            {
                Title = model.Title?.Trim(),
                Message = model.Message?.Trim(),
                Url = model.Url?.Trim(),
                IsPublished = model.IsPublished,
                PublishedFrom = model.PublishedFrom,
                PublishedTo = model.PublishedTo,
                DisplayOrder = model.DisplayOrder,
                CreatedAt = now,
                CreatedBy = HttpContext.Session.GetString("UserId"),
                EditedAt = now,
                EditedBy = HttpContext.Session.GetString("UserId"),
                MACAddress = MACService.GetMAC(),
            };

            if (!await _noticeManager.AddAsync(notice))
            {
                TempData["failed"] = "Could not save the notice.";
                return View(model);
            }

            TempData["created"] = "Notice saved.";
            return RedirectToAction(nameof(Notices));
        }

        public async Task<IActionResult> EditNotice(int id)
        {
            var notice = await _noticeManager.GetByIdAsync(id);
            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNotice(int id, MobileNotice model)
        {
            var notice = await _noticeManager.GetByIdAsync(id);
            if (notice == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            notice.Title = model.Title?.Trim();
            notice.Message = model.Message?.Trim();
            notice.Url = model.Url?.Trim();
            notice.IsPublished = model.IsPublished;
            notice.PublishedFrom = model.PublishedFrom;
            notice.PublishedTo = model.PublishedTo;
            notice.DisplayOrder = model.DisplayOrder;
            notice.EditedAt = DateTime.Now;
            notice.EditedBy = HttpContext.Session.GetString("UserId");
            notice.MACAddress = MACService.GetMAC();

            if (!await _noticeManager.UpdateAsync(notice))
            {
                TempData["failed"] = "Could not update the notice.";
                return View(model);
            }

            TempData["created"] = "Notice updated.";
            return RedirectToAction(nameof(Notices));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var notice = await _noticeManager.GetByIdAsync(id);
            if (notice == null)
            {
                return NotFound();
            }

            if (!await _noticeManager.RemoveAsync(notice))
            {
                TempData["failed"] = "Could not delete the notice.";
            }
            else
            {
                TempData["deleted"] = "Notice deleted.";
            }

            return RedirectToAction(nameof(Notices));
        }

        public async Task<IActionResult> Sliders()
        {
            var items = await _sliderManager.GetAllAsync();
            return View(items.OrderBy(s => s.DisplayOrder).ThenByDescending(s => s.CreatedAt));
        }

        public IActionResult CreateSlider()
        {
            return View(new MobileSlider { IsPublished = true, DisplayOrder = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSlider(MobileSlider model, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var imagePath = await SaveImageAsync(image);
            if (image != null && imagePath == null)
            {
                TempData["failed"] = "Upload a PNG, JPG, GIF or WebP image under 5 MB.";
                return View(model);
            }

            var now = DateTime.Now;
            var slider = new MobileSlider
            {
                Title = model.Title?.Trim(),
                Subtitle = model.Subtitle?.Trim(),
                ImagePath = imagePath,
                TargetUrl = model.TargetUrl?.Trim(),
                IsPublished = model.IsPublished,
                PublishedFrom = model.PublishedFrom,
                PublishedTo = model.PublishedTo,
                DisplayOrder = model.DisplayOrder,
                CreatedAt = now,
                CreatedBy = HttpContext.Session.GetString("UserId"),
                EditedAt = now,
                EditedBy = HttpContext.Session.GetString("UserId"),
                MACAddress = MACService.GetMAC(),
            };

            if (!await _sliderManager.AddAsync(slider))
            {
                TempData["failed"] = "Could not save the slider.";
                return View(model);
            }

            TempData["created"] = "Slider saved.";
            return RedirectToAction(nameof(Sliders));
        }

        public async Task<IActionResult> EditSlider(int id)
        {
            var slider = await _sliderManager.GetByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSlider(int id, MobileSlider model, IFormFile image)
        {
            var slider = await _sliderManager.GetByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var imagePath = await SaveImageAsync(image);
            if (image != null && imagePath == null)
            {
                TempData["failed"] = "Upload a PNG, JPG, GIF or WebP image under 5 MB.";
                return View(model);
            }

            slider.Title = model.Title?.Trim();
            slider.Subtitle = model.Subtitle?.Trim();
            slider.TargetUrl = model.TargetUrl?.Trim();
            slider.IsPublished = model.IsPublished;
            slider.PublishedFrom = model.PublishedFrom;
            slider.PublishedTo = model.PublishedTo;
            slider.DisplayOrder = model.DisplayOrder;
            if (imagePath != null)
            {
                DeleteImage(slider.ImagePath);
                slider.ImagePath = imagePath;
            }
            slider.EditedAt = DateTime.Now;
            slider.EditedBy = HttpContext.Session.GetString("UserId");
            slider.MACAddress = MACService.GetMAC();

            if (!await _sliderManager.UpdateAsync(slider))
            {
                TempData["failed"] = "Could not update the slider.";
                return View(model);
            }

            TempData["created"] = "Slider updated.";
            return RedirectToAction(nameof(Sliders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlider(int id)
        {
            var slider = await _sliderManager.GetByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            if (!await _sliderManager.RemoveAsync(slider))
            {
                TempData["failed"] = "Could not delete the slider.";
            }
            else
            {
                DeleteImage(slider.ImagePath);
                TempData["deleted"] = "Slider deleted.";
            }

            return RedirectToAction(nameof(Sliders));
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (extension == null || !AllowedImageExtensions.Contains(extension) || image.Length > MaxImageBytes)
            {
                return null;
            }

            var folder = Path.Combine(_tenantFiles.WebRootPath, SliderFolder);
            Directory.CreateDirectory(folder);
            var fileName = "slider_" + Guid.NewGuid().ToString("N") + extension;
            var fullPath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/" + SliderFolder.Replace('\\', '/') + "/" + fileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var fileName = Path.GetFileName(imagePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var fullPath = Path.Combine(_tenantFiles.WebRootPath, SliderFolder, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
