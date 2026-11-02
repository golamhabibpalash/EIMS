using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.ViewModels.Certificates;

namespace SMS_App.Controllers;

/// <summary>
/// Shared plumbing for the on-demand student certificate generators (Testimonial, Transfer
/// Certificate): the class/section/keyword student filter, the dropdown data, and institute
/// image loading. No records are stored - each certificate is produced live from the student.
/// </summary>
public abstract class CertificateGeneratorController : Controller
{
    protected readonly IStudentManager _studentManager;
    protected readonly IInstituteManager _instituteManager;
    protected readonly IAcademicSessionManager _sessionManager;
    protected readonly IAcademicClassManager _classManager;
    protected readonly IAcademicSectionManager _sectionManager;
    protected readonly IWebHostEnvironment _host;

    protected CertificateGeneratorController(IStudentManager studentManager, IInstituteManager instituteManager,
        IAcademicSessionManager sessionManager, IAcademicClassManager classManager,
        IAcademicSectionManager sectionManager, IWebHostEnvironment host)
    {
        _studentManager = studentManager;
        _instituteManager = instituteManager;
        _sessionManager = sessionManager;
        _classManager = classManager;
        _sectionManager = sectionManager;
        _host = host;
    }

    protected async Task PopulateFilterDropdownsAsync(int? sessionId, int? classId, int? sectionId)
    {
        var current = await _sessionManager.GetCurrentAcademicSessionAsync();
        if (sessionId == null) sessionId = current?.Id;

        ViewBag.academicSessionId = new SelectList(await _sessionManager.GetAllAsync(), "Id", "Name", sessionId);
        var classes = await _classManager.GetAllAsync();
        ViewBag.academicClassId = new SelectList(classes.Where(c => c.Status == true), "Id", "Name", classId);
        if (classId != null)
        {
            var sections = await _sectionManager.GetAllByClassWithSessionId(classId.Value, sessionId ?? current?.Id ?? 0);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name", sectionId);
        }
    }

    protected async Task<CertificateFilterVM> BuildFilterVMAsync(int? sessionId, int? classId,
        int? sectionId, string search)
    {
        var current = await _sessionManager.GetCurrentAcademicSessionAsync();
        int? effSession = sessionId ?? current?.Id;

        var vm = new CertificateFilterVM
        {
            AcademicSessionId = effSession,
            AcademicClassId = classId,
            AcademicSectionId = sectionId,
            SearchString = search,
            HasQueried = classId != null || sectionId != null || !string.IsNullOrWhiteSpace(search),
        };
        if (!vm.HasQueried) return vm;

        var all = (await _studentManager.GetAllAsync()) ?? Enumerable.Empty<Student>();
        IEnumerable<Student> q = all.Where(s => s.Status && s.AcademicSessionId == effSession);
        if (classId != null) q = q.Where(s => s.AcademicClassId == classId.Value);
        if (sectionId != null) q = q.Where(s => s.AcademicSectionId == sectionId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            q = q.Where(s =>
                (s.Name?.ToLowerInvariant().Contains(term) ?? false)
                || (s.NameBangla?.Contains(search) ?? false)
                || s.ClassRoll.ToString().Contains(term)
                || (s.UniqueId?.ToLowerInvariant().Contains(term) ?? false)
                || (s.PhoneNo?.Contains(term) ?? false));
        }

        vm.Students = q
            .OrderBy(s => s.AcademicClass?.ClassSerial).ThenBy(s => s.ClassRoll)
            .Select(s => new CertificateStudentRowVM
            {
                Id = s.Id,
                Name = s.Name,
                UniqueId = s.UniqueId,
                ClassRoll = s.ClassRoll,
                ClassName = s.AcademicClass?.Name,
                SectionName = s.AcademicSection?.Name,
            }).ToList();

        return vm;
    }

    protected byte[] ReadInstituteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var path = Path.Combine(_host.WebRootPath, "Images", "Institute", fileName);
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllBytes(path) : null;
    }

    /// <summary>Loads the institute logo bytes (institute logo or the bundled default).</summary>
    protected async Task<(Institute Institute, byte[] Logo, byte[] Signature)> LoadBrandingAsync()
    {
        var institute = await _instituteManager.GetByIdAsync(1);
        var logo = ReadInstituteImage(string.IsNullOrWhiteSpace(institute?.Logo) ? "smslogo.png" : institute.Logo)
                   ?? ReadInstituteImage("smslogo.png");
        var signature = ReadInstituteImage("signature.jpg");
        return (institute, logo, signature);
    }
}
