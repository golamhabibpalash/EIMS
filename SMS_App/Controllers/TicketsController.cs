using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.Tickets;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

/// <summary>
/// Lets users raise tickets for the developer and hold the discussion in one
/// place. Status changes are recorded as system notes by the manager, so the
/// ticket reads as a history rather than needing a separate audit trail.
/// </summary>
public class TicketsController : Controller
{
    private const long MaxAttachmentBytes = 2 * 1024 * 1024;
    private const string MaxAttachmentLabel = "2 MB";

    // Images and PDF only - screenshots and scanned documents are what tickets actually need,
    // and keeping the allow-list narrow avoids ever having to serve an arbitrary file type inline.
    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"
    };

    private readonly ITicketManager _ticketManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _host;
    private readonly IAppLogger _appLogger;

    public TicketsController(
        ITicketManager ticketManager,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment host,
        IAppLogger appLogger)
    {
        _ticketManager = ticketManager;
        _userManager = userManager;
        _host = host;
        _appLogger = appLogger;
    }

    // GET: Tickets
    [Authorize(Policy = "IndexTicketsPolicy")]
    public async Task<IActionResult> Index(TicketStatus? statusFilter, TicketType? typeFilter, string searchText, bool mineOnly = false)
    {
        GlobalUI.PageTitle = "Tickets";

        var currentUserId = _userManager.GetUserId(User);
        var tickets = await _ticketManager.GetAllWithDetailsAsync();

        if (mineOnly && !string.IsNullOrEmpty(currentUserId))
        {
            tickets = tickets.Where(t => t.RaisedByUserId == currentUserId).ToList();
        }

        if (statusFilter.HasValue)
        {
            tickets = tickets.Where(t => t.Status == statusFilter.Value).ToList();
        }

        if (typeFilter.HasValue)
        {
            tickets = tickets.Where(t => t.TicketType == typeFilter.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            tickets = tickets.Where(t =>
                    (t.Title != null && t.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Description != null && t.Description.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // Resolve every author once instead of hitting the user store per row.
        var names = await BuildUserNameLookupAsync(
            tickets.Select(t => t.RaisedByUserId).Concat(tickets.Select(t => t.AssignedToUserId)));

        var counts = await _ticketManager.GetStatusCountsAsync();

        var model = new TicketListVM
        {
            StatusFilter = statusFilter,
            TypeFilter = typeFilter,
            SearchText = searchText,
            MineOnly = mineOnly,
            StatusList = BuildEnumSelectList<TicketStatus>(statusFilter),
            TypeList = BuildEnumSelectList<TicketType>(typeFilter),
            PendingCount = counts.TryGetValue(TicketStatus.Pending, out var p) ? p : 0,
            WorkingCount = counts.TryGetValue(TicketStatus.Working, out var w) ? w : 0,
            SolvedCount = counts.TryGetValue(TicketStatus.Solved, out var s) ? s : 0,
            RejectedCount = counts.TryGetValue(TicketStatus.Rejected, out var r) ? r : 0,
            Tickets = tickets.Select(t => new TicketListItemVM
            {
                Id = t.Id,
                Title = t.Title,
                TicketType = t.TicketType,
                Status = t.Status,
                RaiseDate = t.RaiseDate,
                RaisedByName = LookupName(names, t.RaisedByUserId),
                AssignedToName = LookupName(names, t.AssignedToUserId),
                CommentCount = t.TicketComments?.Count(c => !c.IsSystemNote) ?? 0,
                AttachmentCount = t.TicketAttachments?.Count ?? 0,
                ClosedAt = t.ClosedAt
            }).ToList()
        };

        return View(model);
    }

    // GET: Tickets/Details/5
    [Authorize(Policy = "DetailsTicketsPolicy")]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _ticketManager.GetByIdWithDetailsAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        return View(await BuildDetailsAsync(ticket));
    }

    // GET: Tickets/Create
    [Authorize(Policy = "CreateTicketsPolicy")]
    public IActionResult Create()
    {
        GlobalUI.PageTitle = "Raise a Ticket";
        return View(new TicketCreateVM { TicketTypeList = BuildEnumSelectList<TicketType>(TicketType.Issue) });
    }

    // POST: Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateTicketsPolicy")]
    public async Task<IActionResult> Create(TicketCreateVM model)
    {
        GlobalUI.PageTitle = "Raise a Ticket";

        if (!ModelState.IsValid)
        {
            model.TicketTypeList = BuildEnumSelectList<TicketType>(model.TicketType);
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Accounts");
        }

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            TicketType = model.TicketType,
            RaiseDate = DateTime.Now
        };

        if (!await _ticketManager.RaiseAsync(ticket, userId, MACService.GetMAC()))
        {
            ModelState.AddModelError(string.Empty, "The ticket could not be saved. Please try again.");
            model.TicketTypeList = BuildEnumSelectList<TicketType>(model.TicketType);
            return View(model);
        }

        var rejected = await SaveAttachmentsAsync(ticket.Id, model.Attachments, userId);

        TempData["success"] = rejected.Count == 0
            ? "Ticket raised successfully."
            : $"Ticket raised, but {rejected.Count} file(s) were not attached: {string.Join("; ", rejected)}";

        await _appLogger.InfoAsync($"Ticket #{ticket.Id} raised by {User?.Identity?.Name}");
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    // GET: Tickets/Edit/5
    [Authorize(Policy = "EditTicketsPolicy")]
    public async Task<IActionResult> Edit(int id)
    {
        var ticket = await _ticketManager.GetByIdWithDetailsAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        GlobalUI.PageTitle = "Edit Ticket";
        return View(new TicketEditVM
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            TicketType = ticket.TicketType,
            TicketTypeList = BuildEnumSelectList<TicketType>(ticket.TicketType),
            ExistingAttachments = ticket.TicketAttachments?.ToList() ?? new List<TicketAttachment>()
        });
    }

    // POST: Tickets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditTicketsPolicy")]
    public async Task<IActionResult> Edit(int id, TicketEditVM model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var ticket = await _ticketManager.GetByIdAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.TicketTypeList = BuildEnumSelectList<TicketType>(model.TicketType);
            model.ExistingAttachments = (await _ticketManager.GetAttachmentsAsync(id)).ToList();
            return View(model);
        }

        var userId = _userManager.GetUserId(User);

        ticket.Title = model.Title.Trim();
        ticket.Description = model.Description.Trim();
        ticket.TicketType = model.TicketType;
        ticket.EditedBy = userId;
        ticket.EditedAt = DateTime.Now;
        ticket.MACAddress = MACService.GetMAC();

        if (!await _ticketManager.UpdateAsync(ticket))
        {
            ModelState.AddModelError(string.Empty, "The ticket could not be updated. Please try again.");
            model.TicketTypeList = BuildEnumSelectList<TicketType>(model.TicketType);
            model.ExistingAttachments = (await _ticketManager.GetAttachmentsAsync(id)).ToList();
            return View(model);
        }

        var rejected = await SaveAttachmentsAsync(id, model.Attachments, userId);
        TempData["success"] = rejected.Count == 0
            ? "Ticket updated."
            : $"Ticket updated, but {rejected.Count} file(s) were not attached: {string.Join("; ", rejected)}";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Tickets/ChangeStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ChangeStatusTicketsPolicy")]
    public async Task<IActionResult> ChangeStatus(int id, TicketStatus newStatus, string statusNote)
    {
        var userId = _userManager.GetUserId(User);
        bool isChanged = await _ticketManager.ChangeStatusAsync(id, newStatus, userId, MACService.GetMAC(), statusNote);

        TempData[isChanged ? "success" : "fail"] = isChanged
            ? $"Ticket moved to {newStatus}."
            : "Status unchanged - the ticket is already in that status.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Tickets/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CommentTicketsPolicy")]
    public async Task<IActionResult> AddComment(int id, string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment))
        {
            TempData["fail"] = "Write something before posting a comment.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User);
        var comment = new TicketComment { TicketId = id, Message = newComment };

        TempData[await _ticketManager.AddCommentAsync(comment, userId, MACService.GetMAC()) ? "success" : "fail"]
            = "Comment posted.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Tickets/DownloadAttachment/5
    [Authorize(Policy = "DetailsTicketsPolicy")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var attachment = await _ticketManager.GetAttachmentAsync(id);
        if (attachment is null)
        {
            return NotFound();
        }

        var path = Path.Combine(AttachmentFolder(), attachment.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            TempData["fail"] = "That file is no longer on the server.";
            return RedirectToAction(nameof(Details), new { id = attachment.TicketId });
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(bytes, attachment.ContentType ?? "application/octet-stream", attachment.OriginalFileName);
    }

    // GET: Tickets/ViewAttachment/5
    /// <summary>
    /// Same file as DownloadAttachment, but served "inline" instead of "attachment" so the
    /// browser renders an image or PDF itself rather than downloading it - used for the
    /// thumbnail and "View" link on the details page.
    /// </summary>
    [Authorize(Policy = "DetailsTicketsPolicy")]
    public async Task<IActionResult> ViewAttachment(int id)
    {
        var attachment = await _ticketManager.GetAttachmentAsync(id);
        if (attachment is null)
        {
            return NotFound();
        }

        var path = Path.Combine(AttachmentFolder(), attachment.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(path);

        // The stored name is user input (the original upload's file name) - strip quotes and
        // control characters before it goes into a response header, rather than trust it as-is.
        var safeName = new string((attachment.OriginalFileName ?? "attachment")
            .Where(c => !char.IsControl(c) && c != '"')
            .ToArray());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "attachment";
        }

        Response.Headers["Content-Disposition"] = $"inline; filename=\"{safeName}\"";
        return File(bytes, attachment.ContentType ?? "application/octet-stream");
    }

    // POST: Tickets/DeleteAttachment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EditTicketsPolicy")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await _ticketManager.GetAttachmentAsync(id);
        if (attachment is null)
        {
            return NotFound();
        }

        int ticketId = attachment.TicketId;

        if (await _ticketManager.RemoveAttachmentAsync(attachment))
        {
            // Remove the row first; a file left behind is harmless, a row pointing
            // at a missing file is not.
            var path = Path.Combine(AttachmentFolder(), attachment.StoredFileName);
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (IOException ex)
            {
                await _appLogger.WarningAsync($"Ticket attachment row {id} deleted but file remains", ex.Message);
            }

            TempData["success"] = "Attachment removed.";
        }
        else
        {
            TempData["fail"] = "The attachment could not be removed.";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    // GET: Tickets/Delete/5
    [Authorize(Policy = "DeleteTicketsPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _ticketManager.GetByIdWithDetailsAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        GlobalUI.PageTitle = "Delete Ticket";
        return View(await BuildDetailsAsync(ticket));
    }

    // POST: Tickets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteTicketsPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ticket = await _ticketManager.GetByIdWithDetailsAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        var storedNames = ticket.TicketAttachments?.Select(a => a.StoredFileName).ToList() ?? new List<string>();

        if (!await _ticketManager.RemoveAsync(ticket))
        {
            TempData["fail"] = "The ticket could not be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var name in storedNames)
        {
            try
            {
                var path = Path.Combine(AttachmentFolder(), name);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (IOException ex)
            {
                await _appLogger.WarningAsync($"Ticket {id} deleted but attachment {name} remains", ex.Message);
            }
        }

        TempData["success"] = "Ticket deleted.";
        return RedirectToAction(nameof(Index));
    }

    #region helpers

    private string AttachmentFolder() => Path.Combine(_host.WebRootPath, "Uploads", "Tickets");

    /// <summary>
    /// Saves the uploaded files and returns a message for each one that was
    /// refused, so the caller can tell the user exactly what did not attach.
    /// </summary>
    private async Task<List<string>> SaveAttachmentsAsync(int ticketId, List<IFormFile> files, string userId)
    {
        var rejected = new List<string>();
        if (files is null || files.Count == 0)
        {
            return rejected;
        }

        var folder = AttachmentFolder();

        try
        {
            // Never created at deploy time (unlike Images/Student/Photo etc.), so this is the
            // first thing to fail if the app's process account can't write under wwwroot -
            // on Linux that's a permission/ownership mismatch, on Windows an IIS app-pool ACL.
            Directory.CreateDirectory(folder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await _appLogger.ErrorAsync($"Ticket {ticketId} attachment folder could not be created: {folder}", ex.Message);
            rejected.AddRange(files.Where(f => f is not null && f.Length > 0)
                .Select(f => $"{f.FileName} (attachments are not available on this server right now)"));
            return rejected;
        }

        foreach (var file in files.Where(f => f is not null && f.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                rejected.Add($"{file.FileName} (file type not allowed)");
                continue;
            }

            if (file.Length > MaxAttachmentBytes)
            {
                rejected.Add($"{file.FileName} (larger than {MaxAttachmentLabel})");
                continue;
            }

            // Never reuse the uploaded name on disk - it is user input.
            var storedName = $"T{ticketId}_{Guid.NewGuid():N}{extension}";

            try
            {
                await using (var stream = new FileStream(Path.Combine(folder, storedName), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _ticketManager.AddAttachmentAsync(new TicketAttachment
                {
                    TicketId = ticketId,
                    StoredFileName = storedName,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    ContentType = file.ContentType,
                    FileSize = file.Length
                }, userId, MACService.GetMAC());
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                rejected.Add($"{file.FileName} (could not be saved)");
                await _appLogger.ErrorAsync($"Ticket {ticketId} attachment failed", ex.Message);
            }
        }

        return rejected;
    }

    private async Task<TicketDetailsVM> BuildDetailsAsync(Ticket ticket)
    {
        var comments = (await _ticketManager.GetCommentsAsync(ticket.Id)).ToList();

        var names = await BuildUserNameLookupAsync(
            comments.Select(c => c.CreatedBy)
                    .Append(ticket.RaisedByUserId)
                    .Append(ticket.AssignedToUserId));

        return new TicketDetailsVM
        {
            Ticket = ticket,
            RaisedByName = LookupName(names, ticket.RaisedByUserId),
            AssignedToName = LookupName(names, ticket.AssignedToUserId),
            Attachments = (await _ticketManager.GetAttachmentsAsync(ticket.Id)).ToList(),
            NewStatus = ticket.Status,
            StatusList = BuildEnumSelectList<TicketStatus>(ticket.Status),
            CanManage = (await AuthorizeStatusChangeAsync()),
            Comments = comments.Select(c => new TicketCommentVM
            {
                Id = c.Id,
                Message = c.Message,
                IsSystemNote = c.IsSystemNote,
                CreatedAt = c.CreatedAt,
                AuthorName = LookupName(names, c.CreatedBy)
            }).ToList()
        };
    }

    private async Task<bool> AuthorizeStatusChangeAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return false;
        }

        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Any(c => c.Type == "Change Ticket Status");
    }

    /// <summary>
    /// Maps user ids to display names in one pass. Ids that no longer resolve to a
    /// user are left out; callers fall back to a placeholder rather than throwing.
    /// </summary>
    private async Task<Dictionary<string, string>> BuildUserNameLookupAsync(IEnumerable<string> userIds)
    {
        var lookup = new Dictionary<string, string>();

        foreach (var id in userIds.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct())
        {
            if (lookup.ContainsKey(id))
            {
                continue;
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user is not null)
            {
                lookup[id] = user.UserName;
            }
        }

        return lookup;
    }

    private static string LookupName(Dictionary<string, string> names, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "-";
        }

        return names.TryGetValue(userId, out var name) ? name : userId;
    }

    private static SelectList BuildEnumSelectList<TEnum>(object selected) where TEnum : struct, Enum
    {
        var items = Enum.GetValues<TEnum>()
            .Select(v => new { Id = Convert.ToInt32(v), Name = v.ToString() })
            .ToList();

        return new SelectList(items, "Id", "Name", selected is null ? null : Convert.ToInt32(selected));
    }

    #endregion
}
