using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ModuleSubModuleVM;
using SMS.BLL.Contracts;
using SMS.BLL.Managers;
using SMS.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ProjectModulesController:Controller
    {
        private readonly IProjectModuleManager _projectModuleManager;
        public ProjectModulesController(IProjectModuleManager projectModuleManager)
        {
            _projectModuleManager = projectModuleManager;
        }
        [Authorize(Policy = "IndexProjectModulesPolicy")]
        public async Task<ActionResult> Index()
        {
            GlobalUI.PageTitle = "Project Module";

            string lastUpdatedBy = string.Empty;
            string lastUpdatedAt = string.Empty;

            ProjectModuleVM projectModuleVM = new ProjectModuleVM();
            var modules = await _projectModuleManager.GetAllAsync();

            var lastupdatedModule = modules.OrderByDescending(s => s.EditedAt).FirstOrDefault();
            ViewBag.LastUpdatedAt = lastUpdatedAt = lastupdatedModule?.EditedAt.ToString("dd MMM yyyy hh:mm tt") ?? string.Empty;
            ViewBag.LastUpdatedBy = lastUpdatedBy = lastupdatedModule?.EditedBy;

            projectModuleVM.ProjectModuleList=modules.ToList();
            return View(projectModuleVM);
        }

        // GET: ProjectSubModulesController/Details/5
        [Authorize(Policy = "DetailsProjectModulesPolicy")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProjectSubModulesController/Create
        [Authorize(Policy = "CreateProjectModulesPolicy")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProjectSubModulesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateProjectModulesPolicy")]
        public async Task<ActionResult> Create(ProjectModuleVM modelObject)
        {
            try
            {
                if (modelObject.ProjectModule==null)
                {
                    TempData["failed"] = "Data Not found";
                    return RedirectToAction("Index");
                }
                var modules = await _projectModuleManager.GetAllAsync();
                bool isDuplicate = modules.Where(s => s.ModuleName == modelObject.ProjectModule.ModuleName).Any();
                if (isDuplicate)
                {
                    TempData["failed"] = "Module Name already exist.";
                    return RedirectToAction("Index");
                }
                ProjectModule projectModule = new ProjectModule();
                projectModule.MACAddress = MACService.GetMAC();
                projectModule.CreatedAt = DateTime.Now;
                projectModule.CreatedBy = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(projectModule.CreatedBy))
                {
                    return RedirectToAction("Login", "Accounts");
                }
                projectModule.ModuleName = modelObject.ProjectModule.ModuleName;
                projectModule.Remarks = modelObject.ProjectModule.Remarks;
                projectModule.Status = modelObject.ProjectModule.Status;
                bool isSaved = await _projectModuleManager.AddAsync(projectModule);
                if (isSaved)
                {
                    TempData["created"] = "Data Added Successfull";
                }
            }
            catch
            {
                
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProjectSubModulesController/Edit/5
        [Authorize(Policy = "EditProjectModulesPolicy")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProjectSubModulesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditProjectModulesPolicy")]
        public async Task<ActionResult> Edit(int id, ProjectModuleVM modelObject)
        {
            try
            {
                if (modelObject.ProjectModule==null)
                {
                    TempData["failed"] = "Data Not found";
                    return RedirectToAction("Index");
                }
                var existingModule = await _projectModuleManager.GetByIdAsync(modelObject.ProjectModule.Id);
                if (existingModule == null)
                {
                    TempData["failed"] = "Data Not found";
                    return RedirectToAction("Index");
                }
                existingModule.ModuleName = modelObject.ProjectModule.ModuleName;
                existingModule.Remarks = modelObject.ProjectModule.Remarks;
                existingModule.Status = modelObject.ProjectModule.Status;
                existingModule.EditedAt = DateTime.Now;
                existingModule.EditedBy = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(existingModule.EditedBy))
                {
                    return RedirectToAction("Login", "Accounts");
                }
                bool isUpdated = await _projectModuleManager.UpdateAsync(existingModule);
                if (isUpdated)
                {
                    TempData["created"] = "Updated";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Failed to update";
            }
                return RedirectToAction(nameof(Index));
        }


        // POST: ProjectSubModulesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteProjectModulesPolicy")]
        public async Task<ActionResult> Delete(int id, ProjectModuleVM modelObject)
        {
            try
            {
                ProjectModule existingProjectModule = await _projectModuleManager.GetByIdAsync(modelObject.ProjectModule.Id);
                if (existingProjectModule == null)
                {
                    TempData["failed"] = "Sorry! Data not found";
                    return RedirectToAction(nameof(Index));
                }
                if (existingProjectModule.SubModuleList.Count > 0)
                {
                    TempData["failed"] = "Sorry! "+modelObject.ProjectModule.ModuleName+" Module has child sub module/s!";
                    return RedirectToAction(nameof(Index));
                }
                bool isDeleted = await _projectModuleManager.RemoveAsync(modelObject.ProjectModule);
                if (isDeleted)
                {
                    TempData["deleted"] = "Deleted Succesfully";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Failed to Delete";
            }

                return RedirectToAction(nameof(Index));
        }
    }
}
