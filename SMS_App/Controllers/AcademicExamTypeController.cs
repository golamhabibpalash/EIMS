using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem;
using SMS.BLL.Contracts;
using SMS.Entities;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AcademicExamTypeController : Controller
    {
        private readonly IAcademicExamTypeManager _academicExamTypeManager;
        public AcademicExamTypeController(IAcademicExamTypeManager academicExamTypeManager)
        {
            _academicExamTypeManager = academicExamTypeManager;
        }
        // GET: AcademicExamTypeController
        [Authorize(Policy = "IndexAcademicExamTypePolicy")]
        public async Task<ActionResult> Index()
        {
            var allExamTypes = await _academicExamTypeManager.GetAllAsync();
            return View(allExamTypes);
        }

        // GET: AcademicExamTypeController/Details/5
        [Authorize(Policy = "DetailsAcademicExamTypePolicy")]
        public async Task<ActionResult> Details(int id)
        {
            AcademicExamType academicExamType = await _academicExamTypeManager.GetByIdAsync(id);
            return View(academicExamType);
        }

        // GET: AcademicExamTypeController/Create
        [Authorize(Policy = "CraeteAcademicExamTypePolicy")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: AcademicExamTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CraeteAcademicExamTypePolicy")]
        public async Task<ActionResult> Create(AcademicExamType academicExamType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var types = await _academicExamTypeManager.GetAllAsync();
                    AcademicExamType existingAcademicExamType = types.FirstOrDefault(s => s.ExamTypeName == academicExamType.ExamTypeName);
                    if (existingAcademicExamType != null)
                    {
                        TempData["error"] = "Exam types is already exist!";
                        return View(academicExamType);
                    }
                    var isSaved = await _academicExamTypeManager.AddAsync(academicExamType);
                    if (isSaved)
                    {
                        TempData["created"] = "Created Successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    TempData["error"] = "Failed! ";
                    return View(academicExamType);
                }

                return View(academicExamType);
                
            }
            catch
            {
                return View();
            }
        }

        // GET: AcademicExamTypeController/Edit/5
        [Authorize(Policy = "EditAcademicExamTypePolicy")]
        public async Task<ActionResult> Edit(int id)
        {
            AcademicExamType academicExamType = await _academicExamTypeManager.GetByIdAsync(id);
            return View(academicExamType);
        }

        // POST: AcademicExamTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditAcademicExamTypePolicy")]
        public async Task<ActionResult> Edit(int id, AcademicExamType academicExamType)
        {
            try
            {
                if (id !=academicExamType.Id)
                {
                    return View(academicExamType);
                }
                if (ModelState.IsValid)
                {
                    var isSaved = await _academicExamTypeManager.UpdateAsync(academicExamType);
                    if (isSaved)
                    {
                        TempData["updated"] = "Updated Successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(academicExamType);
            }
            catch
            {
                return View();
            }
        }

        // GET: AcademicExamTypeController/Delete/5
        [Authorize(Policy = "DeleteAcademicExamTypePolicy")]
        public async Task<ActionResult> Delete(int id)
        {
            GlobalUI.PageTitle = "Delete Academic Exam Type";
            AcademicExamType academicExamType = await _academicExamTypeManager.GetByIdAsync(id);
            return View(academicExamType);
        }

        // POST: AcademicExamTypeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteAcademicExamTypePolicy")]
        public async Task<ActionResult> Delete(int id, AcademicExamType aExamType)
        {
            try
            {
                bool isRemoved =await _academicExamTypeManager.RemoveAsync(aExamType);
                if (isRemoved)
                {
                    TempData["deleted"] = "deleted Successfully";
                    return RedirectToAction(nameof(Index));
                }
                return View(aExamType);
            }
            catch
            {
                return View();
            }
        }
    }
