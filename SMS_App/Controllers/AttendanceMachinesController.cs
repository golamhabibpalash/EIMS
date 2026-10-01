using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.ShortMessageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers;

[Authorize]
public class AttendanceMachinesController : Controller
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IEmployeeManager _employeeManager;
    private readonly IStudentManager _studentManager;
    private readonly IDesignationManager _designationManager;
    private readonly IAcademicClassManager _academicClassManager;
    private readonly IAcademicSessionManager _academicSessionManager;
    private readonly IPhoneSMSManager _phoneSMSManager; 
    private readonly IParamBusConfigManager _paramBusConfigManager;
    private readonly IInstituteManager _instituteManager;

    public AttendanceMachinesController(IHttpContextAccessor contextAccessor, IAttendanceMachineManager attendanceMachineManager, IEmployeeManager employeeManager, IStudentManager studentManager, IDesignationManager designationManager, IAcademicClassManager academicClassManager, IAcademicSessionManager academicSessionManager, IPhoneSMSManager phoneSMSManager, IInstituteManager instituteManager, IParamBusConfigManager paramBusConfigManager = null)
    {
        _contextAccessor = contextAccessor;
        _attendanceMachineManager = attendanceMachineManager;
        _employeeManager = employeeManager;
        _studentManager = studentManager;
        _designationManager = designationManager;
        _academicClassManager = academicClassManager;
        _academicSessionManager = academicSessionManager;
        _phoneSMSManager = phoneSMSManager;
        _instituteManager = instituteManager;
        _paramBusConfigManager = paramBusConfigManager;
    }
    // GET: AttendanceMachinesController
    [Authorize(Policy = "IndexAttendanceMachinesPolicy")]
    public async Task<ActionResult> Index(string attendanceFor, DateTime dateTime, string attendanceType,  int? aSessionId, int? aClassId, int? aSectionId)
    {
        string date = dateTime.ToString("yyyy-MM-dd");
        
        List<AttendanceVM> attendanceVMs = new List<AttendanceVM>();

        
        ViewBag.attendanceDate = Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd");
        attendanceType = String.IsNullOrEmpty(attendanceType) ? "all" : attendanceType;

        if (!string.IsNullOrEmpty(attendanceFor) && !string.IsNullOrEmpty(attendanceType))
        {
            ViewBag.attendanceFor = attendanceFor;
            ViewBag.attendanceType = attendanceType;
            ViewBag.aSessionId =aSessionId!=null? aSessionId:null;
            ViewBag.aClassId = aClassId!=null? aClassId:null;
            var result = await _attendanceMachineManager.GetAttendanceByDateAsync(attendanceFor, date, attendanceType, aSessionId, aClassId, aSectionId);
            attendanceVMs.AddRange(result);
        }
        // PunchTime is null for absent rows (attendanceType "all"/"absent"); sort those last.
        return View(attendanceVMs
            .OrderBy(a => a.Class_Designation ?? string.Empty)
            .ThenBy(n => string.IsNullOrEmpty(n.PunchTime) ? "99" : n.PunchTime.Substring(0, 2)));
    }


    [Authorize(Policy = "DetailsAttendanceMachinesPolicy")]
    public async Task<ActionResult> Details(int id)
    {
        var attendance = await _attendanceMachineManager.GetByIdAsync(id);
        return View(attendance);
    }


    [Authorize(Policy = "CreateAttendanceMachinesPolicy")]
    public async Task<ActionResult> Create()
    {
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(),"Id","Name");
        var configs = await _paramBusConfigManager.GetAllAsync();
        ViewBag.checkinStart = configs.FirstOrDefault(s => s.ParamSL == 1).ParamValue;
        ViewBag.checkinEnd = configs.FirstOrDefault(s => s.ParamSL == 2).ParamValue;
        ViewBag.checkoutStart = configs.FirstOrDefault(s => s.ParamSL == 3).ParamValue;
        ViewBag.checkoutEnd = configs.FirstOrDefault(s => s.ParamSL == 4).ParamValue;
        return View();
    }


    [HttpPost,ValidateAntiForgeryToken]
    [Authorize(Policy = "CreateAttendanceMachinesPolicy")]
    public async Task<ActionResult> Create(Tran_MachineRawPunch model, bool isSMSSend)
    {
        string msg = string.Empty;
        ViewData["AcademicClassList"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
        if (ModelState.IsValid)
        {
            try
            {
                string cardNo = model.CardNo;
                //model.CardNo = cardNo.PadLeft(8, '0');
                bool isSaved =await _attendanceMachineManager.AddAsync(model);
                if (isSaved)
                {
                    msg = "New attendance added manually for" + model.CardNo;
                    if (isSMSSend)
                    {
                        Student st = await _studentManager.GetStudentByClassRollAsync(Convert.ToInt32(model.CardNo));
                        if (st != null)
                        {
                            PhoneSMS phoneSMS = new PhoneSMS();
                            phoneSMS.MACAddress = MACService.GetMAC();
                            phoneSMS.CreatedAt = DateTime.Now;
                            phoneSMS.CreatedBy = HttpContext.Session.GetString("UserId");
                            phoneSMS.MobileNumber = st.GuardianPhone == null ? st.PhoneNo : st.GuardianPhone;
                            var smsInstitute = await _instituteManager.GetFirstOrDefaultAsync();
                            var smsInstituteName = smsInstitute == null || string.IsNullOrWhiteSpace(smsInstitute.ShortName)
                                ? smsInstitute?.Name?.Trim() ?? string.Empty
                                : smsInstitute.ShortName.Trim();
                            var smsNameSuffix = string.IsNullOrWhiteSpace(smsInstituteName) ? string.Empty : " -" + smsInstituteName;
                            phoneSMS.Text = st.NameBangla + " আজ " + model.PunchDatetime.ToString("hh:mm tt") + " মিনিটে স্কুলে উপস্থিত হয়েছে।" + smsNameSuffix + " ।";
                            phoneSMS.SMSType = "CheckIn";
                            bool isSend = await MobileSMS.SendSMS(phoneSMS.MobileNumber, phoneSMS.Text);                                
                            if (isSaved)
                            {
                               await _phoneSMSManager.AddAsync(phoneSMS);
                                msg ="New attendance added manually with sms for" + model.CardNo;
                            }

                        }
                    }
                    TempData["success"] = msg;
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["success"] = "Execption: " + ex.Message; 
            }
            
        }
        return View(model);
    }

    // GET: AttendanceMachinesController/Edit/5
    [Authorize(Policy = "EditAttendanceMachinesPolicy")]
    public async Task<ActionResult> Edit(int id)
    {
        var attendance = await _attendanceMachineManager.GetByIdAsync(id);
        return View(attendance);
    }

    // POST: AttendanceMachinesController/Edit/5
    [HttpPost,ValidateAntiForgeryToken]
    [Authorize(Policy = "EditAttendanceMachinesPolicy")]
    public async Task<ActionResult> Edit(int id, Tran_MachineRawPunch model)
    {
        if (id != model.Tran_MachineRawPunchId)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            await _attendanceMachineManager.UpdateAsync(model);
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // GET: AttendanceMachinesController/Delete/5
    [Authorize(Policy = "DeleteAttendanceMachinesPolicy")]
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: AttendanceMachinesController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DeleteAttendanceMachinesPolicy")]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

}
