using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.BLL.Managers;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize]
    public class SubjectEnrollmentController : Controller
    {
        private readonly ISubjectEnrollmentManager _subjectEnrollmentManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IStudentManager _studentManager;
        private readonly IAcademicClassSubjectManager _academicClassSubjectManager;
        private readonly IAcademicSubjectManager _academicSubjectManager;
        private readonly ISubjectEnrollmentDetailManager _subjectEnrollmentDetailManager;
        
        public SubjectEnrollmentController(ISubjectEnrollmentManager subjectEnrollmentManager, IAcademicClassManager academicClassManager, IStudentManager studentManager, IAcademicClassSubjectManager academicClassSubjectManager, IAcademicSubjectManager academicSubjectManager, ISubjectEnrollmentDetailManager subjectEnrollmentDetailManager)
        {
            _subjectEnrollmentManager = subjectEnrollmentManager;
            _academicClassManager = academicClassManager;
            _studentManager = studentManager;
            _academicClassSubjectManager = academicClassSubjectManager;
            _academicSubjectManager = academicSubjectManager;
            _subjectEnrollmentDetailManager = subjectEnrollmentDetailManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "SubjectEnrollSubjectEnrollmentPolicy")]
        public async Task<IActionResult> SubjectEnroll(int? stRoll) 
        {
            GlobalUI.PageTitle = "Subject Enrollment";
            SubjectEnrollment subjectEnroll = new SubjectEnrollment();
            List<SubjectEnrollmentDetail> subjectEnrollmentDetails = new List<SubjectEnrollmentDetail>();
            Student student = new Student();
            if (stRoll!=null)
            {
                ViewBag.roll = stRoll;
                try
                {
                    student = await _studentManager.GetStudentByClassRollAsync(stRoll.Value);
                    subjectEnroll.Student = student;
                    subjectEnrollmentDetails = await _subjectEnrollmentManager.GetAllDetailsByStudentIdAsync(student.Id);
                    subjectEnroll.Student = student;
                    subjectEnroll.StudentId = student.Id;
                    subjectEnroll.EnrolledSubjects = subjectEnrollmentDetails;
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            List<AcademicSubject> academicSubjects =await _academicSubjectManager.GetSubjectsByClassIdAsync(student.AcademicClassId);
            ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            ViewData["AcademicSubjectList"] = new SelectList(academicSubjects.Where(s => s.AcademicSubjectType.SubjectTypeName.ToLower()=="optional"),"Id","SubjectName");

            return View(subjectEnroll);
        }
        public async Task<IActionResult> DefaultEnroll(int StudentId)
        {
            Student student = await _studentManager.GetByIdAsync(StudentId);
            if (student != null)
            {
                SubjectEnrollment subjectEnrollment = new SubjectEnrollment();
                //subjectEnrollment.Student = student;
                subjectEnrollment.StudentId = StudentId;
                subjectEnrollment.MACAddress = MACService.GetMAC();
                subjectEnrollment.CreatedAt = DateTime.Now;
                subjectEnrollment.CreatedBy = HttpContext.Session.GetString("UserId");

                List<SubjectEnrollmentDetail> subjectEnrollmentDetails = new List<SubjectEnrollmentDetail>();
                List<AcademicSubject> academicSubjects = await _academicClassSubjectManager.GetSubjectsByClassIdAsync(student.AcademicClassId);
                if (academicSubjects!=null && academicSubjects.Count>0)
                {
                    bool optionalSubjectSet = false;
                    foreach (var item in academicSubjects)
                    {
                        SubjectEnrollmentDetail subjectEnrollmentDetail = new SubjectEnrollmentDetail();
                        subjectEnrollmentDetail.AcademicSubjectId = item.Id;
                        subjectEnrollmentDetail.MACAddress = MACService.GetMAC();
                        subjectEnrollmentDetail.AcademicSubjectTypeId = item.AcademicSubjectTypeId;
                        subjectEnrollmentDetail.CreatedAt = DateTime.Now;
                        subjectEnrollmentDetail.CreatedBy = HttpContext.Session.GetString("UserId");
                        if (item.AcademicSubjectType.SubjectTypeName.ToLower() == "optional" && optionalSubjectSet==false)
                        {
                            subjectEnrollmentDetail.IsOptional = true;
                            optionalSubjectSet = true;
                        }
                        subjectEnrollmentDetails.Add(subjectEnrollmentDetail);
                    }
                    subjectEnrollment.EnrolledSubjects = subjectEnrollmentDetails;
                }
                try
                {
                    bool isSaved = await _subjectEnrollmentManager.AddAsync(subjectEnrollment);
                    if (isSaved)
                    {
                        TempData["saved"] = "Subject are enrolled by default value";
                    }
                    else
                    {
                        TempData["error"] = "Failed! Something wrong.";
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                TempData["error"] = "Failed! Student Not Found.";
            }
            return RedirectToAction("SubjectEnroll", "SubjectEnrollment", new {stRoll= student.ClassRoll});
        }
        [HttpPost]
        [Authorize(Policy = "SetOptionalSubjectSubjectEnrollmentPolicy")]
        public async Task<IActionResult> SetOptionalSubject(int StudentId, int subjectId) 
        {
            Student student = await _studentManager.GetByIdAsync(StudentId);
            SubjectEnrollment subjectEnrollment = await _subjectEnrollmentManager.GetByStudentIdAsync(StudentId);
            if (subjectEnrollment != null)
            {
                subjectEnrollment.EditedAt = DateTime.Now;
                subjectEnrollment.EditedBy = HttpContext.Session.GetString("UserId");
                subjectEnrollment.MACAddress = MACService.GetMAC();

                if (subjectEnrollment.EnrolledSubjects.Count>0)
                {
                    foreach (var item in subjectEnrollment.EnrolledSubjects)
                    {
                        item.EditedAt = DateTime.Now;
                        item.EditedBy = HttpContext.Session.GetString("UserId");
                        item.MACAddress = MACService.GetMAC();

                        item.IsOptional = false;
                        if (item.AcademicSubjectId==subjectId)
                        {
                            item.IsOptional = true;
                        }
                        try
                        {
                            await _subjectEnrollmentDetailManager.UpdateAsync(item);
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }
            }            
            
            return RedirectToAction("SubjectEnroll", "SubjectEnrollment", new { stRoll = student.ClassRoll });
        }
    }
}
