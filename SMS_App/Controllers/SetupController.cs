using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.ShortMessageService;
using SMS_App.ViewModels.SetupVM;
using SMS_App.ViewModels.Students;
using SMS.BLL.Contracts;
using SMS.BLL.Managers;
using SMS.Entities;
using SMS.Entities.RptModels.AttendanceVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SetupController : Controller
    {
        private readonly ISetupMobileSMSManager _setupMobileSMSManager;
        private readonly IMapper _mapper;
        private readonly IStudentManager _studentManager;  
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IInstituteManager _instituteManager;
        private readonly IPhoneSMSManager _phoneSMSManager;
        public SetupController(ISetupMobileSMSManager setupMobileSMSManager, IMapper mapper, IStudentManager studentManager, IAcademicClassManager academicClassManager, IInstituteManager instituteManager, IPhoneSMSManager phoneSMSManager)
        {
            _setupMobileSMSManager = setupMobileSMSManager;
            _mapper = mapper;
            _studentManager = studentManager;
            _academicClassManager = academicClassManager;
            _instituteManager = instituteManager;
            _phoneSMSManager = phoneSMSManager;
        }

        [Authorize(Policy = "IndexSetupPolicy")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "SMSControlSetupPolicy")]
        public async Task<IActionResult> SMSControl()
        {
            SetupMobileSMS setupMobileSMS = await _setupMobileSMSManager.GetByIdAsync(1);
            AttendanceSetupVM attendanceSetupVM = new AttendanceSetupVM();
            try
            {
                attendanceSetupVM = _mapper.Map<AttendanceSetupVM>(setupMobileSMS);
            }
            catch (System.Exception)
            {
                throw;
            }
            
            return View(attendanceSetupVM);
        }

        [HttpPost]
        [Authorize(Policy = "SMSControlSetupPolicy")]
        public async Task<IActionResult> SMSControl(AttendanceSetupVM attendanceSetupVM)
        {
            string msg = "";
            if (attendanceSetupVM!=null)
            {
                SetupMobileSMS objSetupMobileSMS = await _setupMobileSMSManager.GetByIdAsync(attendanceSetupVM.Id);

                objSetupMobileSMS = _mapper.Map<SetupMobileSMS>(attendanceSetupVM);

                objSetupMobileSMS.EditedAt = DateTime.Now;
                objSetupMobileSMS.EditedBy = HttpContext.Session.GetString("UserId");
                objSetupMobileSMS.MACAddress = MACService.GetMAC();
                try
                {
                    bool isUpdated = await _setupMobileSMSManager.UpdateAsync(objSetupMobileSMS);
                    if (isUpdated)
                    {
                        msg = "SMS Setup Updated successful";
                        TempData["edited"] = msg;
                        return RedirectToAction("AttendanceBackgroundJob", "Hangfire");
                        //TempData["edited"] = msg;
                        //attendanceSetupVM.EditedAt = DateTime.Now;
                        //attendanceSetupVM.EditedBy = HttpContext.Session.GetString("UserId");   
                        //return View(attendanceSetupVM);
                    }
                    else
                    {
                        msg = "Fail to save";
                    }
                }
                catch (System.Exception)
                {

                    throw;
                }
            }
            return View(attendanceSetupVM);
        }

        [HttpGet]
        [Authorize(Policy = "SMSControlSetupPolicy")]
        public async Task<IActionResult> TestSms()
        {
            GlobalUI.PageTitle = "SMS Dry-Run Test";
            var model = new SmsTestVM { SampleName = "টেস্ট শিক্ষার্থী" };
            model.InstituteName = AttendanceSmsText.DisplayName(await _instituteManager.GetFirstOrDefaultAsync());
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "SMSControlSetupPolicy")]
        public async Task<IActionResult> TestSms(SmsTestVM model, string command)
        {
            GlobalUI.PageTitle = "SMS Dry-Run Test";
            model.InstituteName = AttendanceSmsText.DisplayName(await _instituteManager.GetFirstOrDefaultAsync());
            if (string.IsNullOrWhiteSpace(model.SampleName))
            {
                ModelState.AddModelError(nameof(model.SampleName), "Sample name is required.");
                return View(model);
            }

            string time = DateTime.Now.ToString("hh:mm tt");
            string today = DateTime.Now.ToString("dd MMM yyyy");
            model.PreviewCheckIn = AttendanceSmsText.CheckIn(model.SampleName, time, model.InstituteName);
            model.PreviewCheckOut = AttendanceSmsText.CheckOut(model.SampleName, time, model.InstituteName);
            model.PreviewAbsent = AttendanceSmsText.AbsentToday(model.SampleName, today, model.InstituteName);

            if (command == "send")
            {
                string number = (model.TestNumber ?? string.Empty).Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(number, @"^01[3-9]\d{8}$"))
                {
                    model.Result = "Enter a valid 11-digit mobile number starting with 013-019.";
                    model.Sent = false;
                    return View(model);
                }
                bool sent = await MobileSMS.SendSMS(number, model.PreviewCheckIn);
                model.Sent = sent;
                model.Result = sent ? $"Test SMS sent to {number}." : "Test SMS failed: gateway balance finished or provider problem.";
                if (sent)
                {
                    await _phoneSMSManager.AddAsync(new PhoneSMS
                    {
                        Text = model.PreviewCheckIn,
                        MobileNumber = number,
                        SMSType = "Test",
                        MACAddress = MACService.GetMAC(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = HttpContext.Session.GetString("UserId")
                    });
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "StudentWiseSMSServiceSetupPolicy")]
        public async Task<IActionResult> StudentWiseSMSService(int? academicClassId)
        {
            var students = await _studentManager.GetAllAsync();
            students = students.Where(s => s.Status == true).ToList();
            StudentWiseSMSServiceVM studentWiseSMSServiceVM = new StudentWiseSMSServiceVM();
            if (academicClassId!=null)
            {
                students = students.Where(s => s.AcademicClassId == academicClassId).ToList();
                studentWiseSMSServiceVM.Students = (List<Student>)students;
            }

            ViewBag.academicClassId = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name", academicClassId);

            return View(studentWiseSMSServiceVM); 
        }

        [HttpPost]
        [Authorize(Policy = "StudentWiseSMSServiceSetupPolicy")]
        public async Task<IActionResult> StudentWiseSMSService(StudentWiseSMSServiceVM studentWiseSMSServiceVM)
        {
            if (studentWiseSMSServiceVM!=null)
            {
                try
                {
                    int totalUpdated = 0;
                    foreach (var student in studentWiseSMSServiceVM.Students)
                    {
                        Student student1 = await _studentManager.GetByIdAsync(student.Id);
                        if (student1.SMSService != student.SMSService)
                        {
                            student1.SMSService = student.SMSService;
                            await _studentManager.UpdateAsync(student1);
                            totalUpdated++; 
                        }
                    }
                    ViewBag.totalUpdated = totalUpdated;
                    TempData["updated"] = "Total " + totalUpdated + " data updated";
                }
                catch (Exception)
                {
                    throw;
                }
            }
            ViewBag.academicClassId = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            return View();
        }
    }
}
