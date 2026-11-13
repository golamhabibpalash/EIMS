using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS_App.Utilities.Pagination;
using SMS_App.ViewModels.LogVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

public class LogsController : Controller
{
    private readonly ILogManager _logManager;
    private readonly IMapper _mapper;
    public LogsController(ILogManager logManager, IMapper mapper = null)
    {
        _logManager = logManager;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(
    int pageIndex = 1,
    int pageSize = 50,
    string searchText = null,
    string sortOrder = "date_desc")
    {
        // Get all logs
        var logs = await _logManager.GetAllAsync();
        logs = logs.ToList().OrderByDescending(s => s.Timestamp).ToList();
        // Apply search filter
        if (!string.IsNullOrEmpty(searchText))
        {
            logs = logs.Where(log =>
                (log.Message != null && log.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (log.Level != null && log.Level.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (log.Exception != null && log.Exception.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Map to view models
        var activityLogs = _mapper.Map<List<ActivityLogVM>>(logs);

        // Apply sorting
        activityLogs = sortOrder switch
        {
            "date_asc" => activityLogs.OrderBy(x => x.Timestamp).ToList(),
            "date_desc" => activityLogs.OrderByDescending(x => x.Timestamp).ToList(),
            "level_asc" => activityLogs.OrderBy(x => x.Level).ToList(),
            "level_desc" => activityLogs.OrderByDescending(x => x.Level).ToList(),
            "message_asc" => activityLogs.OrderBy(x => x.Message).ToList(),
            "message_desc" => activityLogs.OrderByDescending(x => x.Message).ToList(),
            _ => activityLogs.OrderByDescending(x => x.Timestamp).ToList()
        };

        // Store filter/sort parameters for view
        ViewData["CurrentFilter"] = searchText;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["DateSort"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["LevelSort"] = sortOrder == "level_asc" ? "level_desc" : "level_asc";
        ViewData["MessageSort"] = sortOrder == "message_asc" ? "message_desc" : "message_asc";
        ViewData["TotalData"] = activityLogs.Count;
        ViewData["PageSize"] = pageSize;

        // Create paginated list
        var paginatedLogs = PaginatedList<ActivityLogVM>.Create(activityLogs, pageIndex, pageSize);

        return View(paginatedLogs);
    }
}
