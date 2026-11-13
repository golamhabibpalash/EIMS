using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ClaimContext;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    public class ClaimStoresController : Controller
    {
        private readonly IClaimStoreManager _claimStoreManager;
        private readonly IProjectModuleManager _projectModuleManager;
        private readonly IMapper _mapper;
        public ClaimStoresController(IClaimStoreManager claimStoreManager, IProjectModuleManager projectModuleManager, IMapper mapper)
        {
            _claimStoreManager = claimStoreManager;
            _projectModuleManager = projectModuleManager;
            _mapper = mapper;
        }
        // GET: ClaimStoresController
        [Authorize(Policy = "IndexClaimStoresPolicy")]
        public async Task<ActionResult> Index()
        {
            GlobalUI.PageTitle = "Claim Stores";
            string lastUpdatedBy = string.Empty;
            string lastUpdatedAt = string.Empty;

            var claims = await _claimStoreManager.GetAllAsync();
            var lastupdatedClaim = claims.OrderByDescending(s=>s.EditedAt).FirstOrDefault();
            ViewBag.LastUpdatedAt =lastUpdatedAt = lastupdatedClaim?.EditedAt.ToString("dd MMM yyyy hh:mm tt") ?? string.Empty;
            ViewBag.LastUpdatedBy =lastUpdatedBy = lastupdatedClaim?.EditedBy;

            ClaimStoreVM claimStoreVM = new ClaimStoreVM();
            claimStoreVM.ModuleSelectList = new SelectList(await _projectModuleManager.GetAllAsync(), "Id", "ModuleName");
            claimStoreVM.ClaimStoresList = claims.OrderBy(s => s.SubModule.SubModuleName).ToList();
            return View(claimStoreVM);
        }

        // POST: ClaimStoresController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Policy = "CreateClaimStoresPolicy")]
        public async Task<ActionResult> Create(ClaimStoreVM modelObject)
        {
            try
            {
                if (modelObject.ClaimStores == null)
                {
                    TempData["failed"] = "Data not found";
                    return RedirectToAction(nameof(Index));
                }
                //var claimStores = await _claimStoreManager.GetAllAsync();               
                if (await _claimStoreManager.IsExistAsync(modelObject.ClaimStores.ClaimValue, modelObject.ClaimStores.SubModuleId))
                {
                    TempData["failed"] = modelObject.ClaimStores.ClaimValue + " is already exist in this Sub Module";
                    return RedirectToAction(nameof(Index));
                }
                modelObject.ClaimStores.CreatedAt = DateTime.Now;
                modelObject.ClaimStores.CreatedBy = HttpContext.Session.GetString("UserId");
                modelObject.ClaimStores.EditedAt = DateTime.Now;
                modelObject.ClaimStores.EditedBy = HttpContext.Session.GetString("UserId");

                modelObject.ClaimStores.MACAddress = MACService.GetMAC();
                bool isAdded = await _claimStoreManager.AddAsync(modelObject.ClaimStores);
                if (isAdded)
                {
                    TempData["created"] = "New Claims Added Successfully";
                }
            }
            catch
            {
                return View();
            }
            return RedirectToAction(nameof(Index));
        }


        // POST: ClaimStoresController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditClaimStoresPolicy")]
        public async Task<ActionResult> Edit(int id, ClaimStoreVM modelObject)
        {
            try
            {
                ClaimStores existingClaimStore = await _claimStoreManager.GetByIdAsync(modelObject.ClaimStores.Id);
                if (existingClaimStore==null)
                {
                    TempData["failed"] = "Data not found";
                    return RedirectToAction("Index");
                }

                existingClaimStore.ClaimValue = modelObject.ClaimStores.ClaimValue;
                existingClaimStore.ClaimType = modelObject.ClaimStores.ClaimType;
                existingClaimStore.SubModuleId = modelObject.ClaimStores.SubModuleId;
                existingClaimStore.EditedAt = DateTime.Now; 
                existingClaimStore.EditedBy = HttpContext.Session.GetString("UserId");
                existingClaimStore.MACAddress = MACService.GetMAC();
                bool isUpdated = await _claimStoreManager.UpdateAsync(existingClaimStore);
                if (isUpdated) {
                    TempData["created"] = "Existing Claim Updated Successfully";
                }
                else
                {
                    TempData["failed"] = "Fail to Update";
                }
            }
            catch
            {
                TempData["failed"] = "Exception! Fail to Update";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ClaimStoresController/Delete/5
        [Authorize(Policy = "DeleteClaimStoresPolicy")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ClaimStoresController/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteClaimStoresPolicy")]
        public async Task<ActionResult> Delete(int id, ClaimStores claimStores)
        {
            try
            {
                var existingClaim = await _claimStoreManager.GetByIdAsync(claimStores.Id);
                if (existingClaim != null)
                {
                    bool isDeleted = await _claimStoreManager.RemoveAsync(existingClaim);
                    if (isDeleted) {
                        TempData["created"] = "Existing Claim Deleted Successfully";
                    }
                }                
            }
            catch
            {
                TempData["failed"] = "Exception! Fail to Delete";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
