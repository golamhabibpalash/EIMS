using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.ShortMessageService;
using SMS_App.ViewModels.SMSVM;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{

    [Authorize(Roles = "SuperAdmin, Admin")]
    public class PhoneSMSController : Controller
    {
        private readonly IPhoneSMSManager _phoneSMSManager;
        private readonly IEmployeeManager _employeeManager;
        private readonly IStudentManager _studentManager;
        private readonly SmsApiService _smsApiService;

        public PhoneSMSController(IPhoneSMSManager phoneSMSManager, IEmployeeManager employeeManager, IStudentManager studentManager, SmsApiService smsApiService)
        {
            _phoneSMSManager = phoneSMSManager;
            _employeeManager = employeeManager;
            _studentManager = studentManager;
            _smsApiService = smsApiService;
        }

        [Authorize(Policy = "IndexPhoneSMSPolicy")]
        public async Task<IActionResult> Index(string smsType, string smsText,string phoneNo, int rowCount,string fromDate, string toDate)
        {
            int dataCount = 0;
            int totalCount = 0;
            
            var allSMS = await _phoneSMSManager.GetAllAsync();
            totalCount = allSMS.Count;
            if (allSMS != null && allSMS.Any())
            {
                ViewBag.minDate = allSMS.OrderBy(s => s.CreatedAt).Select(s => s.CreatedAt.ToString("yyyy-MM-dd")).FirstOrDefault();
                ViewBag.maxDate = allSMS.OrderByDescending(s => s.CreatedAt).Select(s => s.CreatedAt.ToString("yyyy-MM-dd")).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(fromDate))
            {
                allSMS = allSMS.Where(s => s.CreatedAt >= Convert.ToDateTime(fromDate)).ToList();
                ViewBag.fromDate = fromDate;
                totalCount = allSMS.Count;
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                allSMS = allSMS.Where(s => s.CreatedAt == Convert.ToDateTime(toDate) || s.CreatedAt > Convert.ToDateTime(toDate)).ToList();
                ViewBag.toDate = toDate;
                totalCount = allSMS.Count;
            }
            if (!string.IsNullOrEmpty(smsType))
            {
                allSMS = allSMS.Where(s => s.SMSType==smsType).ToList();
                totalCount = allSMS.Count;
            }
            if (!string.IsNullOrEmpty(smsText))
            {
                allSMS = allSMS.Where(s => s.Text.Contains(smsText)).ToList();
                totalCount = allSMS.Count;
            }
                ViewBag.smsText = smsText;
            if (!string.IsNullOrEmpty(phoneNo))
            {
                allSMS = allSMS.Where(s => s.MobileNumber.Contains(phoneNo)).ToList();
                totalCount = allSMS.Count;
            }
                ViewBag.phoneNo = phoneNo;
            if (rowCount>0)
            {
                dataCount = rowCount;
                ViewBag.rowCount = rowCount;
            }
            else
            {
                dataCount = 20;
                ViewBag.rowCount = dataCount;
            }
            List<string> smsTypes = new List<string>();
            if (allSMS!=null)
            {
                smsTypes = allSMS.Select(x => x.SMSType).Distinct().ToList();
            }
            ViewBag.totalCount = totalCount;
            smsTypes = smsTypes.Where(x => x!=null).ToList();
            ViewData["smsTypeList"] = new SelectList(smsTypes);
            return View(allSMS.OrderByDescending(a => a.CreatedAt).Take(dataCount));
        }

        [HttpGet]
        [Authorize(Policy = "CreatePhoneSMSPolicy")]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Policy = "CreatePhoneSMSPolicy")]
        public async Task<IActionResult> Create(SMSCreateVM model)
        {
            if (ModelState.IsValid)
            {
                List<string> phoneNumbers = new List<string>();
                string[] phoneNumberArray = model.PhoneNo.Split(',');
                int sentSMSCount = 0;
                foreach (var number in phoneNumberArray)
                {
                    if (number.Length==11)
                    {
                        phoneNumbers.Add(number);
                    }
                }
                if (phoneNumbers.Count>0)
                {
                    foreach (var item in phoneNumbers)
                    {
                        bool smsSend =await MobileSMS.SendSMS(item, model.SMSText);
                        string user = HttpContext.Session.GetString("UserId");
                        
                        if (smsSend)
                        {
                            PhoneSMS phoneSMS = new() { 
                                Text = model.SMSText, 
                                CreatedAt = DateTime.Now, 
                                CreatedBy = user, 
                                MobileNumber = item,
                                MACAddress = MACService.GetMAC(),
                                SMSType = "Custom"
                            };
                            await _phoneSMSManager.AddAsync(phoneSMS);
                            sentSMSCount++;
                        }
                    }
                }
                if (sentSMSCount > 0)
                {
                    TempData["smsSent"] = "Total " + sentSMSCount + " SMS has been sent.";
                }
                else
                {
                    TempData["smsNotSent"] = "Sorry, no SMS has been sent.";
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "GetAPIDataPhoneSMSPolicy")]
        public IActionResult GetAPIData()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "GetAPIDataPhoneSMSPolicy")]
        public async Task<IActionResult> GetSmsData()
        {

            var result = await _smsApiService.GetSmsDashboardData();
            return Json(result);
        }
        
        [AllowAnonymous]
        public async Task<JsonResult> GetPhoneNumbers(string smsType, int? designationId, int? sessionId, int? classId)
        {

            List<string> phoneNumbers = new();

            if (smsType != null)
            {
                if (smsType=="e")
                {
                    var allEmployee = await _employeeManager.GetAllAsync();
                    if (designationId != null)
                    {
                        allEmployee = allEmployee.Where(e => e.DesignationId == designationId).ToList();
                    }
                    foreach (var employee in allEmployee)
                    {
                        phoneNumbers.Add(employee.Phone);
                    }
                }
                else if (smsType=="s")
                {
                    var allStudents = await _studentManager.GetAllAsync();
                    if (sessionId!=null)
                    {
                        allStudents =(from s in allStudents
                                     where s.AcademicSessionId == sessionId
                                     select s).ToList();
                        if (classId != null)
                        {
                            allStudents = allStudents.Where(s => s.AcademicClassId == classId).ToList();
                        }

                    }
                    foreach(var student in allStudents)
                    {
                        phoneNumbers.Add(student.PhoneNo);
                    }
                }
                else if(smsType=="g")
                {
                    var allStudents = await _studentManager.GetAllAsync();
                    if (sessionId != null)
                    {
                        allStudents = (from s in allStudents
                                       where s.AcademicSessionId == sessionId
                                       select s).ToList();
                        if (classId != null)
                        {
                            allStudents = allStudents.Where(s => s.AcademicClassId == classId).ToList();
                        }

                    }
                    foreach (var student in allStudents)
                    {
                        phoneNumbers.Add(student.GuardianPhone);
                    }
                }
            }
            return Json(phoneNumbers);
        }

        

    }
}
