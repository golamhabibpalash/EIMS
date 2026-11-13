using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SMS_App.ViewModels.ModuleSubModuleVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Utilities.ViewComponents;

public class NavigationViewComponent : ViewComponent
{
    private readonly SiteMap _siteMap;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProjectModuleManager _projectModuleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public NavigationViewComponent(
        SiteMap siteMap,
        IHttpContextAccessor httpContextAccessor,
        IProjectModuleManager projectModuleManager,
        UserManager<ApplicationUser> userManager)
    {
        _siteMap = siteMap;
        _httpContextAccessor = httpContextAccessor;
        _projectModuleManager = projectModuleManager;
        _userManager = userManager;
    }

    // viewName selects the markup flavor: "Default" (admin Smart theme) or
    // "Gentelella" (teacher/student layouts). Filtering is identical - user
    // claims + active DB modules - so every layout is permission-driven from
    // the same SiteMap source with no per-layout hardcoding.
    public async Task<IViewComponentResult> InvokeAsync(string viewName = "Default")
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return View(new SiteMap { Modules = new List<Module>() });

        // Get all active modules from DB
        var modules = await _projectModuleManager.GetAllAsync();
        var allActiveModules = modules.Where(m => m.Status).ToList();

        // Filter sitemap modules by active DB modules
        var activeModulesFromSiteMap = _siteMap.Modules
            .Where(m => allActiveModules.Any(am => am.ModuleName == m.SystemName))
            .ToList();

        // Collect user claims (values only)
        var userClaims = user.Claims
                            .Select(c => c.Value.Trim())
                            .ToList();

        // Get current route for active highlighting
        var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
        var currentController = routeData?.Values["controller"]?.ToString();
        var currentAction = routeData?.Values["action"]?.ToString();

        ViewData["CurrentController"] = currentController;
        ViewData["CurrentAction"] = currentAction;

        // Filter modules → submodules → items
        var filteredModules = activeModulesFromSiteMap
            .Select(m => new Module
            {
                SystemName = m.SystemName,
                DisplayName = m.DisplayName,
                Icon = m.Icon,
                Permission = m.Permission,
                Submodules = m.Submodules.Select(s => new Submodule
                {
                    SystemName = s.SystemName,
                    DisplayName = s.DisplayName,
                    Icon = s.Icon,
                    Items = s.Items
                    .Where(i => userClaims.Contains(i.Claim?.Trim()))
                    .Select(i => new Item
                    {
                        SystemName = i.SystemName,
                        DisplayName = i.DisplayName,
                        Controller = i.Controller,
                        Action = i.Action,
                        Claim = i.Claim
                    })
                    .ToList()
                }).ToList()
            })
            .ToList();

        var filteredSiteMap = new SiteMap { Modules = filteredModules };

        return View(viewName, filteredSiteMap);
    }
}
