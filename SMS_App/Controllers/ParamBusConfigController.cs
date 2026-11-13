using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.ConfigureVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ParamBusConfigController : Controller
    {
        private readonly IParamBusConfigManager _paramBusConfigManager;
        private readonly IMapper _mapper;
        public ParamBusConfigController(IParamBusConfigManager paramBusConfigManager, IMapper mapper)
        {
            _paramBusConfigManager = paramBusConfigManager;
            _mapper = mapper;
        }

        [Authorize(Policy = "IndexParamBusConfigPolicy")]
        public async Task<IActionResult> Index()
        {
            GlobalUI.PageTitle = "Configure Parameter";
            var configs = await _paramBusConfigManager.GetAllAsync();
            ParamBusConfigVM paramBusConfigVM = new ParamBusConfigVM();
            paramBusConfigVM.ParamBusConfigs = (IList<ParamBusConfig>)configs;
            return View(paramBusConfigVM);
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "UpSertParamBusConfigPolicy")]
        public async Task<IActionResult> UpSert(ParamBusConfig paramBusConfig)
        {
            if (paramBusConfig.Id>0)
            {
                GlobalUI.PageTitle = "Update Configure Parameter";
                ParamBusConfig existingParamBusConfig = await _paramBusConfigManager.GetByIdAsync(paramBusConfig.Id);
                if (existingParamBusConfig == null)
                {
                    TempData["failed"] = "Configuration data not found.";
                    return RedirectToAction(nameof(Index));
                }
                try
                {
                    existingParamBusConfig.EditedBy = HttpContext.Session.GetString("UserId");
                    existingParamBusConfig.EditedAt = DateTime.Now;
                    existingParamBusConfig.MACAddress = MACService.GetMAC();
                    existingParamBusConfig.ParamValue = paramBusConfig.ParamValue;
                    existingParamBusConfig.ParamSL = paramBusConfig.ParamSL;
                    existingParamBusConfig.ConfigName = paramBusConfig.ConfigName;
                    existingParamBusConfig.Remarks = paramBusConfig.Remarks;
                    existingParamBusConfig.IsActive = paramBusConfig.IsActive;
                    bool isUpdated = await _paramBusConfigManager.UpdateAsync(existingParamBusConfig);
                    if (isUpdated)
                    {
                        TempData["created"] = "Configuration data updated Successfully";
                    }
                    else
                    {
                        TempData["failed"] = "Fail to update configuration data.";
                    }
                }
                catch (Exception)
                {
                    TempData["failed"] = "Exception! Fail to update";
                }
            }
            else
            {
                GlobalUI.PageTitle = "Create Configure Parameter";
                ParamBusConfig existParamBusConfig = await _paramBusConfigManager.GetByParamSL(paramBusConfig.ParamSL);
                if (existParamBusConfig != null)
                {
                    TempData["failed"] = "Sorry! Same Config SL is already exist";
                    return RedirectToAction(nameof(Index));
                }
                try
                {
                    if (ModelState.IsValid)
                    {
                        paramBusConfig.CreatedAt = DateTime.Now;
                        paramBusConfig.CreatedBy = HttpContext.Session.GetString("UserId");
                        paramBusConfig.EditedBy = HttpContext.Session.GetString("UserId");
                        paramBusConfig.EditedAt = DateTime.Now;
                        paramBusConfig.MACAddress = MACService.GetMAC();
                        bool isSaved = await _paramBusConfigManager.AddAsync(paramBusConfig);
                        if (isSaved)
                        {
                            TempData["created"] = "New Configuration data added Successfully";
                        }
                    }
                }
                catch (Exception)
                {
                    TempData["failed"] = "Exception! Fail to create";
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
