using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ModuleSubModuleVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ProjectSubModulesController : Controller
    {
        private readonly IProjectSubModuleManager _projectSubModuleManager;
        private readonly IProjectModuleManager _projectModuleManager;
        private readonly IMapper _mapper;
        public ProjectSubModulesController( IProjectSubModuleManager projectSubModuleManager, IMapper mapper, IProjectModuleManager projectModuleManager)
        {
            _projectSubModuleManager = projectSubModuleManager;
            _mapper = mapper;
            _projectModuleManager = projectModuleManager;
        }

        // GET: ProjectSubModulesController
        [Authorize(Policy = "IndexProjectSubModulesPolicy")]
        public async Task<ActionResult> Index()
        {
            GlobalUI.PageTitle = "Sub Modules";

            string lastUpdatedBy = string.Empty;
            string lastUpdatedAt = string.Empty;

            var subModules = await _projectSubModuleManager.GetAllAsync();
            var lastupdatedSubModule = subModules.OrderByDescending(s => s.EditedAt).FirstOrDefault();
            ViewBag.LastUpdatedAt = lastUpdatedAt = lastupdatedSubModule?.EditedAt.ToString("dd MMM yyyy hh:mm tt") ?? string.Empty;
            ViewBag.LastUpdatedBy = lastUpdatedBy = lastupdatedSubModule?.EditedBy;

            ProjectSubModuleVM projectSubModuleVM = new ProjectSubModuleVM();
            projectSubModuleVM.SubModuleList = subModules.ToList();
            projectSubModuleVM.ModuleSelectList = new SelectList(await _projectModuleManager.GetAllAsync(), "Id", "ModuleName");
            return View(projectSubModuleVM);
        }

        // GET: ProjectSubModulesController/Details/5
        [Authorize(Policy = "DetailsProjectSubModulesPolicy")]
        public ActionResult Details(int id)
        {
            return View();
        }


        // POST: ProjectSubModulesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateProjectSubModulesPolicy")]
        public async Task<ActionResult> Create(ProjectSubModuleVM modelObject)
        {
            try
            {
                if (modelObject.ProjectSubModule==null)
                {
                    TempData["failed"] = "Data not found";
                    return RedirectToAction("Index");
                }
                var subModules = await _projectSubModuleManager.GetAllAsync();
                bool isExist = subModules.Any(s => s.SubModuleName == modelObject.ProjectSubModule.SubModuleName && s.ProjectModuleId == modelObject.ProjectSubModule.ProjectModuleId);
                if (isExist)
                {
                    TempData["failed"] = modelObject.ProjectSubModule.SubModuleName+" is already exist in this module";
                    return RedirectToAction("Index");
                }
                ProjectSubModule projectSubModule = new ProjectSubModule();
                projectSubModule = _mapper.Map<ProjectSubModule>(modelObject.ProjectSubModule);
                projectSubModule.MACAddress = MACService.GetMAC();
                projectSubModule.CreatedBy = HttpContext.Session.GetString("UserId");
                projectSubModule.CreatedAt = DateTime.Now;
                bool isSaved = await _projectSubModuleManager.AddAsync(projectSubModule);
                if (isSaved)
                {
                    TempData["created"] = "Sub Module Added Successfully";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Fail to create";
            }

            return RedirectToAction(nameof(Index));
        }


        // POST: ProjectSubModulesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditProjectSubModulesPolicy")]
        public async Task<ActionResult> Edit(int id, ProjectSubModuleVM modelObject)
        {
            try
            {
                if (modelObject.ProjectSubModule == null)
                {
                    TempData["failed"] = "Data not found";
                    return RedirectToAction("Index");
                }

                var existingModule = await _projectSubModuleManager.GetByIdAsync(modelObject.ProjectSubModule.Id);
                if (existingModule == null)
                {
                    TempData["failed"] = "Data not found";
                    return RedirectToAction("Index");
                }
                existingModule.EditedAt = DateTime.Now;
                existingModule.EditedBy = HttpContext.Session.GetString("UserId");
                existingModule.SubModuleName = modelObject.ProjectSubModule.SubModuleName;
                existingModule.Remarks = modelObject.ProjectSubModule.Remarks;
                existingModule.Status = modelObject.ProjectSubModule.Status;
                existingModule.ProjectModuleId = modelObject.ProjectSubModule.ProjectModuleId;
                bool isUpdated = await _projectSubModuleManager.UpdateAsync(existingModule);
                if (isUpdated)
                {
                    TempData["created"] = "Sub Module Updated Successfully";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Fail to update";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ProjectSubModulesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteProjectSubModulesPolicy")]
        public async Task<ActionResult> Delete(ProjectSubModule projectSubModule)
        {
            try
            {
                ProjectSubModule existingSubModule = await _projectSubModuleManager.GetByIdAsync(projectSubModule.Id);
                if (existingSubModule.ClaimStoresList.Count>0)
                {
                    TempData["failed"] = "Child Claim/s is available in this Sub Domain";
                    return RedirectToAction(nameof(Index));
                }
                bool isRemoved = await _projectSubModuleManager.RemoveAsync(projectSubModule);
                if (isRemoved)
                {
                    TempData["created"] = "Sub Module Deleted Successfully";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Fail to Delete";
            }
                return RedirectToAction(nameof(Index));
        }

        public async Task<JsonResult> getAllByModuleId(int moduleId)
        {
            if (moduleId>0)
            {
                var subModules = await _projectSubModuleManager.GetAllAsync();
                var results = subModules.Where(s => s.ProjectModuleId ==  moduleId).ToList();
                return Json(results);
            }
            return Json(null);
        }
    }
}
