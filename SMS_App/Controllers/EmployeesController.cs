using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.ShortMessageService;
using SMS_App.ViewModels.Employees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SMS_App.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IWebHostEnvironment _host;
    private readonly IEmployeeManager _employeeManager;
    private readonly IMapper _mapper;
    private readonly IGenderManager _genderManager;
    private readonly IReligionManager _religionManager;
    private readonly INationalityManager _nationalityManager;
    private readonly IEmpTypeManager _empTypeManager;
    private readonly IDesignationManager _designationManager;
    private readonly IDivisionManager _divisionManager;
    private readonly IBloodGroupManager _bloodGroupManager;
    private readonly IUpazilaManager _upazilaManager;
    private readonly IDistrictManager _districtManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneSMSManager _phoneSMSManager;
    private readonly IInstituteManager _instituteManager;
    private readonly ILogManager _logManager;
    private readonly IAppLogger _appLogger;

    private readonly SMS_App.MultiTenancy.ITenantFileStorage _tenantFiles;

    public EmployeesController(IWebHostEnvironment host, IEmployeeManager employeeManager, IGenderManager genderManager, IReligionManager religionManager, IMapper mapper, INationalityManager nationalityManager, IEmpTypeManager empTypeManager, IDesignationManager designationManager, IDivisionManager divisionManager, IDistrictManager districtManager, IUpazilaManager upazilaManager, IBloodGroupManager bloodGroupManager, UserManager<ApplicationUser> userManager, IPhoneSMSManager phoneSMSManager, IInstituteManager instituteManager, ILogManager logManager, IAppLogger appLogger, SMS_App.MultiTenancy.ITenantFileStorage tenantFiles)
    {
        _tenantFiles = tenantFiles;
        _host = host;
        _employeeManager = employeeManager;
        _genderManager = genderManager;
        _mapper = mapper;
        _religionManager = religionManager;
        _nationalityManager = nationalityManager;
        _empTypeManager = empTypeManager;
        _designationManager = designationManager;
        _divisionManager = divisionManager;
        _bloodGroupManager = bloodGroupManager;
        _districtManager = districtManager;
        _upazilaManager = upazilaManager;
        _userManager = userManager;
        _phoneSMSManager = phoneSMSManager;
        _instituteManager = instituteManager;
        _logManager = logManager;
        _appLogger = appLogger;
    }


    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "IndexEmployeesPolicy")]
    public async Task<IActionResult> Index()
    {
        var empList = await _employeeManager.GetAllAsync();
        return View(empList.OrderByDescending(e => e.Status).ThenBy(e => e.JoiningDate));
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "IndexEmployeesPolicy")]
    public async Task<IActionResult> Export(string fields, string status = "all")
    {
        var employees = await _employeeManager.GetAllAsync();

        if (status == "active")
            employees = employees.Where(e => e.Status).ToList();
        else if (status == "inactive")
            employees = employees.Where(e => !e.Status).ToList();

        employees = employees.OrderBy(e => e.Designation?.SortingOrder ?? 99999).ThenBy(e => e.EmployeeName).ToList();

        var selectedFields = string.IsNullOrEmpty(fields) 
            ? new List<string>() 
            : fields.Split(',').ToList();

        var exportList = employees.Select(e => new EmployeeExportVM
        {
            EmployeeId = e.Id.ToString("D5"),
            EmployeeName = selectedFields.Contains("EmployeeName") ? e.EmployeeName : null,
            EmployeeNameBangla = selectedFields.Contains("EmployeeNameBangla") ? e.EmployeeNameBangla : null,
            FatherName = selectedFields.Contains("FatherName") ? e.FatherName : null,
            MotherName = selectedFields.Contains("MotherName") ? e.MotherName : null,
            Phone = selectedFields.Contains("Phone") ? e.Phone : null,
            Email = selectedFields.Contains("Email") ? e.Email : null,
            Designation = selectedFields.Contains("Designation") ? e.Designation?.DesignationName : null,
            EmpType = selectedFields.Contains("EmpType") ? e.EmpType?.Name : null,
            Gender = selectedFields.Contains("Gender") ? e.Gender?.Name : null,
            BloodGroup = selectedFields.Contains("BloodGroup") ? e.BloodGroup?.Name : null,
            Religion = selectedFields.Contains("Religion") ? e.Religion?.Name : null,
            DOB = selectedFields.Contains("DOB") ? e.DOB.ToString("yyyy-MM-dd") : null,
            NIDNo = selectedFields.Contains("NIDNo") ? e.NIDNo.ToString() : null,
            JoiningDate = selectedFields.Contains("JoiningDate") ? e.JoiningDate.ToString("yyyy-MM-dd") : null,
            PresentAddress = selectedFields.Contains("PresentAddress") ? e.PresentAddress : null,
            PermanentAddress = selectedFields.Contains("PermanentAddress") ? e.PermanentAddress : null,
            Nominee = selectedFields.Contains("Nominee") ? e.Nominee : null,
            NomineePhone = selectedFields.Contains("NomineePhone") ? e.NomineePhone.ToString() : null,
            Status = selectedFields.Contains("Status") ? (e.Status ? "Active" : "Inactive") : null
        }).ToList();

        ViewBag.SelectedFields = selectedFields;
        ViewBag.FieldsList = new List<string>
        {
            "EmployeeName", "EmployeeNameBangla", "FatherName", "MotherName", "Phone", "Email", "Designation",
            "EmpType", "Gender", "BloodGroup", "Religion", "DOB", "NIDNo",
            "JoiningDate", "PresentAddress", "PermanentAddress", "Nominee", "NomineePhone", "Status"
        };
        ViewBag.Status = status;

        return View(exportList);
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "IndexEmployeesPolicy")]
    [HttpPost]
    public async Task<IActionResult> DownloadCsv(string fields, string status = "all")
    {
        var employees = await _employeeManager.GetAllAsync();

        if (status == "active")
            employees = employees.Where(e => e.Status).ToList();
        else if (status == "inactive")
            employees = employees.Where(e => !e.Status).ToList();

        employees = employees.OrderBy(e => e.Designation?.SortingOrder ?? 99999).ThenBy(e => e.EmployeeName).ToList();

        var selectedFields = string.IsNullOrEmpty(fields) 
            ? new List<string>() 
            : fields.Split(',').ToList();

        var builder = new StringBuilder();
        
        var headers = new List<string> { "Employee Id" };
        if (selectedFields.Contains("EmployeeName")) headers.Add("Employee Name");
        if (selectedFields.Contains("EmployeeNameBangla")) headers.Add("Name (Bangla)");
        if (selectedFields.Contains("FatherName")) headers.Add("Father Name");
        if (selectedFields.Contains("MotherName")) headers.Add("Mother Name");
        if (selectedFields.Contains("Phone")) headers.Add("Phone");
        if (selectedFields.Contains("Email")) headers.Add("Email");
        if (selectedFields.Contains("Designation")) headers.Add("Designation");
        if (selectedFields.Contains("EmpType")) headers.Add("Employee Type");
        if (selectedFields.Contains("Gender")) headers.Add("Gender");
        if (selectedFields.Contains("BloodGroup")) headers.Add("Blood Group");
        if (selectedFields.Contains("Religion")) headers.Add("Religion");
        if (selectedFields.Contains("DOB")) headers.Add("Date of Birth");
        if (selectedFields.Contains("NIDNo")) headers.Add("NID No");
        if (selectedFields.Contains("JoiningDate")) headers.Add("Joining Date");
        if (selectedFields.Contains("PresentAddress")) headers.Add("Present Address");
        if (selectedFields.Contains("PermanentAddress")) headers.Add("Permanent Address");
        if (selectedFields.Contains("Nominee")) headers.Add("Nominee");
        if (selectedFields.Contains("NomineePhone")) headers.Add("Nominee Phone");
        if (selectedFields.Contains("Status")) headers.Add("Status");

        builder.AppendLine(string.Join(",", headers));

        foreach (var e in employees)
        {
            var values = new List<string> { e.Id.ToString("D5") };
            if (selectedFields.Contains("EmployeeName")) values.Add(EscapeCsvValue(e.EmployeeName));
            if (selectedFields.Contains("EmployeeNameBangla")) values.Add(EscapeCsvValue(e.EmployeeNameBangla));
            if (selectedFields.Contains("FatherName")) values.Add(EscapeCsvValue(e.FatherName));
            if (selectedFields.Contains("MotherName")) values.Add(EscapeCsvValue(e.MotherName));
            if (selectedFields.Contains("Phone")) values.Add(EscapeCsvValue(e.Phone));
            if (selectedFields.Contains("Email")) values.Add(EscapeCsvValue(e.Email));
            if (selectedFields.Contains("Designation")) values.Add(EscapeCsvValue(e.Designation?.DesignationName));
            if (selectedFields.Contains("EmpType")) values.Add(EscapeCsvValue(e.EmpType?.Name));
            if (selectedFields.Contains("Gender")) values.Add(EscapeCsvValue(e.Gender?.Name));
            if (selectedFields.Contains("BloodGroup")) values.Add(EscapeCsvValue(e.BloodGroup?.Name));
            if (selectedFields.Contains("Religion")) values.Add(EscapeCsvValue(e.Religion?.Name));
            if (selectedFields.Contains("DOB")) values.Add(e.DOB.ToString("yyyy-MM-dd"));
            if (selectedFields.Contains("NIDNo")) values.Add(e.NIDNo.ToString());
            if (selectedFields.Contains("JoiningDate")) values.Add(e.JoiningDate.ToString("yyyy-MM-dd"));
            if (selectedFields.Contains("PresentAddress")) values.Add(EscapeCsvValue(e.PresentAddress));
            if (selectedFields.Contains("PermanentAddress")) values.Add(EscapeCsvValue(e.PermanentAddress));
            if (selectedFields.Contains("Nominee")) values.Add(EscapeCsvValue(e.Nominee));
            if (selectedFields.Contains("NomineePhone")) values.Add(e.NomineePhone.ToString());
            if (selectedFields.Contains("Status")) values.Add(e.Status ? "Active" : "Inactive");

            builder.AppendLine(string.Join(",", values));
        }

        DateTime today = DateTime.Today;
        return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", today.ToString("yyMMdd") + "_Employee_List.csv");
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    [Authorize(Roles = "SuperAdmin, Admin,Teacher")]
    [Authorize(Policy = "DetailsEmployeesPolicy")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Employee employee = await _employeeManager.GetByIdAsync((int)id);
        if (employee == null)
        {
            return NotFound();
        }
        var employeeDetailsVM = _mapper.Map<EmployeeDetailsVM>(employee);

        employeeDetailsVM.Designation = await _designationManager.GetByIdAsync((int)employee.DesignationId);
        return View(employeeDetailsVM);
    }

    // GET: Employees/Create
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateEmployeesPolicy")]
    public async Task<IActionResult> Create()
    {
        EmployeeCreateVM employee = new();
        employee.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name").ToList();
        employee.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name").ToList();
        employee.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name").ToList();
        employee.EmpTypeList = new SelectList(await _empTypeManager.GetAllAsync(), "Id", "Name").ToList();
        employee.DesignationList = new SelectList(await _designationManager.GetAllAsync(), "Id", "DesignationName").ToList();
        employee.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name").ToList();
        employee.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name").ToList();

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "CreateEmployeesPolicy")]
    public async Task<IActionResult> Create([Bind("Id,EmployeeName,EmployeeNameBangla,FatherName,MotherName,DOB,Image,GenderId,ReligionId,NationalityId,NIDNo,NIDCard,Phone,Email,Nominee,NomineePhone,EmpTypeId,DesignationId,JoiningDate,PresentAddress,PresentUpazilaId,PresentDistrictId,PresentDivisionId,PermanentAddress,PermanentUpazilaId,PermanentDistrictId,PermanentDivisionId,CreatedBy,CreatedAt,EditedBy,EditedAt,Status,BloodGroupId")] EmployeeCreateVM employeeVM, IFormFile empImage, IFormFile nidCard)
    {
        var employee1 = employeeVM;
        var bloodGroupList = await _bloodGroupManager.GetAllAsync();
        employee1.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", employee1.GenderId).ToList();
        employee1.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", employee1.ReligionId).ToList();
        employee1.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", employee1.NationalityId).ToList();
        employee1.EmpTypeList = new SelectList(await _empTypeManager.GetAllAsync(), "Id", "Name", employee1.EmpTypeId).ToList();
        employee1.DesignationList = new SelectList(await _designationManager.GetAllAsync(), "Id", "DesignationName", employee1.DesignationId).ToList();
        employee1.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name", employee1.DivisionList).ToList();
        employee1.BloodGroupList = new SelectList(bloodGroupList.OrderBy(b => b.Name), "Id", "Name", employee1.BloodGroupId).ToList();


        string empPhoto = "";
        string nidPhoto = "";
        bool isPhoneNoExist = false;
        bool isEmailExist = false;
        bool isNIDExist = false;
        var employees = await _employeeManager.GetAllAsync();

        var phoneDuplicate = employees.FirstOrDefault(e => e.Phone.ToString() == employeeVM.Phone.ToString());
        isPhoneNoExist = phoneDuplicate != null ? true : false;

        var emailDuplicate = employees.FirstOrDefault(e => e.Email == employeeVM.Email);
        isEmailExist = emailDuplicate != null ? true : false;

        var NIDDuplicate = employees.FirstOrDefault(e => e.NIDNo == employeeVM.NIDNo);
        isNIDExist = NIDDuplicate != null ? true : false;
        if (isPhoneNoExist == true || isEmailExist == true || isNIDExist == true)
        {
            if (isPhoneNoExist == true)
            {
                ViewBag.msg = "Provided phone number is already exist";
            }
            if (isEmailExist == true)
            {
                ViewBag.msg = "Provided email is already exist";
            }
            if (isNIDExist == true)
            {
                ViewBag.msg = "Provided NID is already exist";
            }
            return View(employee1);
        }


        var employee = _mapper.Map<Employee>(employeeVM);

        if (ModelState.IsValid)
        {

            if (empImage != null)
            {
                string root = _tenantFiles.WebRootPath;
                if (string.IsNullOrEmpty(root))
                {
                    root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                string folder = "Images/Employee/photo";
                string fileExtension = Path.GetExtension(empImage.FileName);
                empPhoto = "e_" + DateTime.Today.ToString("yyyy") + "_" + employeeVM.Phone + fileExtension;
                string pathCombine = Path.Combine(root, folder, empPhoto);
                using var stream = new FileStream(pathCombine, FileMode.Create);
                await empImage.CopyToAsync(stream);
            }

            if (nidCard != null)
            {
                string root = _tenantFiles.WebRootPath;
                string folder = "Images/Employee/NID";

                string fileExtension = Path.GetExtension(nidCard.FileName);
                nidPhoto = "e_NID_" + employeeVM.NIDNo + fileExtension;
                string pathCombine = Path.Combine(root, folder, nidPhoto);
                using var stream = new FileStream(pathCombine, FileMode.Create);
                await nidCard.CopyToAsync(stream);
            }

            employee.CreatedBy = HttpContext.Session.GetString("UserId");
            employee.CreatedAt = DateTime.Now;
            employee.Image = empPhoto;
            employee.NIDCard = nidPhoto;
            bool isSaved = await _employeeManager.AddAsync(employee);
            if (isSaved)
            {
                TempData["saved"] = "Saved Successful";
                var user = await _userManager.FindByEmailAsync(employee.Email);
                if (user == null)
                {
                    ApplicationUser identityUser = new ApplicationUser()
                    {
                        UserName = employee.Email,
                        NormalizedUserName = employee.EmployeeName,
                        Email = employee.Email,
                        PhoneNumber = employee.Phone,
                        PhoneNumberConfirmed = true,
                        NormalizedEmail = employee.Email,
                        ReferenceId = employee.Id,
                        UserType = 'e',
                        EmailConfirmed = true
                    };
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    var random = new Random();
                    string autoGeneratedPassword = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
                    var result = await _userManager.CreateAsync(identityUser, autoGeneratedPassword);
                    if (result.Succeeded)
                    {
                        await _logManager.AddAsync(new Log {
                            Level = "User Info",
                            Exception = "N/A",
                            MessageTemplate = "Employees Controller Create Method",
                            Message = $"User created for employee: {employee.EmployeeName} with email: {employee.Email} and password: {autoGeneratedPassword}",
                            Timestamp = DateTime.Now,
                        });

                        var nResult = await _userManager.AddToRoleAsync(identityUser, "Teacher");
                        if (nResult.Succeeded)
                        {
                            var instituteInfo = await _instituteManager.GetAllAsync();
                            string text = "Dear,\n" + employee.EmployeeName + ",\nYour Username :" + employee.Email + "\nPassword:" + autoGeneratedPassword + "\n" + instituteInfo.FirstOrDefault().Name;
                            bool smsSend = await MobileSMS.SendSMS(employee.Phone, text);
                            if (smsSend == true)
                            {
                                PhoneSMS phoneSMS = new()
                                {
                                    Text = text,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = employee.Email,
                                    MobileNumber = employee.Phone,
                                    MACAddress = MACService.GetMAC(),
                                    SMSType = "NewUser"
                                };

                                await _phoneSMSManager.AddAsync(phoneSMS);
                            }
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
        }
        if(!ModelState.IsValid)
        {
            var firstError = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .Select(ms => new { Field = ms.Key, Error = ms.Value.Errors.First().ErrorMessage })
                .FirstOrDefault();

            if (firstError != null)
            {
                TempData["deleted"] = $"{firstError.Field}: {firstError.Error}";
            }
            else
            {
                TempData["deleted"] = "Validation error.";
            }
            ViewBag.msg = $"Validation error. {firstError?.Error}";
            await _appLogger.InfoAsync($"Validation error: {firstError?.Field} - {firstError?.Error}");
        }
        return View(employee1);
    }

    // GET: Employees/Edit/5
    [Authorize]
    [Authorize(Policy = "EditEmployeesPolicy")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        ApplicationUser user = await _userManager.GetUserAsync(User);

        //bool isAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
        if (user.ReferenceId == id || await _userManager.IsInRoleAsync(user, "SuperAdmin") || await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var employee = await _employeeManager.GetByIdAsync((int)id);

            var employee1 = _mapper.Map<EmployeeEditVM>(employee);
            employee1.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", employee.GenderId).ToList();
            employee1.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", employee.ReligionId).ToList();
            employee1.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", employee.NationalityId).ToList();
            employee1.EmpTypeList = new SelectList(await _empTypeManager.GetAllAsync(), "Id", "Name", employee.EmpTypeId).ToList();
            employee1.DesignationList = new SelectList(await _designationManager.GetAllAsync(), "Id", "DesignationName", employee.DesignationId).ToList();
            employee1.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name").ToList();
            employee1.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name", employee.BloodGroupId).ToList();

            ViewData["PermanentDistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", employee.PermanentDistrictId);
            ViewData["PermanentUpazilaId"] = new SelectList(await _upazilaManager.GetAllAsync(), "Id", "Name", employee.PermanentUpazilaId);
            ViewData["PresentDistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", employee.PresentDistrictId);
            ViewData["PresentUpazilaId"] = new SelectList(await _upazilaManager.GetAllAsync(), "Id", "Name", employee.PresentUpazilaId);


            if (employee1 == null)
            {
                return NotFound();
            }
            TempData["Image"] = employee.Image;
            TempData["NIDCard"] = employee.NIDCard;
            employee1.Image = employee.Image;
            employee1.NIDCard = employee.NIDCard;

            return View(employee1);

        }
        else
        {
            return RedirectToAction("AccessDenied", "Accounts");
        }
    }

    // POST: Employees/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Authorize(Policy = "EditEmployeesPolicy")]
    public async Task<IActionResult> Edit(
    int id,
    EmployeeEditVM employeeVM,
    IFormFile Image,
    IFormFile NIDCard)
    {

        await _appLogger.InfoAsync($"Enter method");
        var currectUser = HttpContext.Session.GetString("UserId");
        if (id != employeeVM.Id)
            return NotFound();

        var existingEmployee = await _employeeManager.GetByIdAsync(id);
        if (existingEmployee == null)
            return NotFound();

        await _appLogger.InfoAsync($"Before Upload Files.");
        // FILE UPLOAD HANDLING
        employeeVM.Image = await SaveFileIfProvided(Image, $"{id.ToString()}_image_", "Images/Employee/photo", existingEmployee.Image);
        employeeVM.NIDCard = await SaveFileIfProvided(NIDCard, $"{id.ToString()}_NID_", "Images/Employee/NID", existingEmployee.NIDCard);

        await _appLogger.InfoAsync($"Employee image {employeeVM.Image} uploaded.");

        if (!ModelState.IsValid)
        {
            await _appLogger.InfoAsync($"ModelState is not valid");
            var firstError = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .Select(ms => new { Field = ms.Key, Error = ms.Value.Errors.First().ErrorMessage })
                .FirstOrDefault();

            if (firstError != null)
            {
                TempData["deleted"] = $"{firstError.Field}: {firstError.Error}";
            }
            else
            {
                TempData["deleted"] = "Validation error.";
            }
            ViewBag.msg = "Validation error.";
            await LoadDropdowns(employeeVM);
            return View(employeeVM);
        }


        try
        {
            // MAP updated fields from VM to entity
            _mapper.Map(employeeVM, existingEmployee);

            existingEmployee.EditedAt = DateTime.Now;
            existingEmployee.EditedBy = currectUser;
            await _appLogger.InfoAsync($"before update employee");
            bool isUpdated = await _employeeManager.UpdateAsync(existingEmployee);
            await _appLogger.InfoAsync($"after update employee");

            if (isUpdated)
                TempData["edited"] = "Update Successfully";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(employeeVM.Id))
                return NotFound();

            throw;
        }
    }


    // GET: Employees/Delete/5

    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "DeleteEmployeesPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        int myId = Convert.ToInt32(id);
        var employee = await _employeeManager.GetByIdAsync(myId);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // POST: Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "DeleteEmployeesPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var emp = await _employeeManager.GetByIdAsync(id);
        bool isDeleted = await _employeeManager.RemoveAsync(emp);
        if (isDeleted)
        {
            TempData["deleted"] = "Deleted successfully";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        var employee = _employeeManager.GetByIdAsync(id);
        if (employee != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IActionResult EmployeeEvaluation()
    {
        return View();
    }

    private async Task<string> SaveFileIfProvided(
    IFormFile file,
    string nidNo,
    string folder,
    string existingFileName)
    {
        if (file == null)
            return existingFileName ?? string.Empty;

        // Normalize folder (no leading slash)
        folder = folder.TrimStart('/', '\\');

        string root = _tenantFiles.WebRootPath;
        if (string.IsNullOrEmpty(root))
            root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        Directory.CreateDirectory(Path.Combine(root, folder));

        string extension = Path.GetExtension(file.FileName);
        string newFileName = $"e_{nidNo}{extension}";
        string fullPath = Path.Combine(root, folder, newFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        return newFileName;
    }


    private async Task LoadDropdowns(EmployeeEditVM vm)
    {
        vm.GenderList = new SelectList(await _genderManager.GetAllAsync(), "Id", "Name", vm.GenderId).ToList();
        vm.ReligionList = new SelectList(await _religionManager.GetAllAsync(), "Id", "Name", vm.ReligionId).ToList();
        vm.NationalityList = new SelectList(await _nationalityManager.GetAllAsync(), "Id", "Name", vm.NationalityId).ToList();
        vm.EmpTypeList = new SelectList(await _empTypeManager.GetAllAsync(), "Id", "Name", vm.EmpTypeId).ToList();
        vm.DesignationList = new SelectList(await _designationManager.GetAllAsync(), "Id", "DesignationName", vm.DesignationId).ToList();
        vm.DivisionList = new SelectList(await _divisionManager.GetAllAsync(), "Id", "Name").ToList();
        vm.BloodGroupList = new SelectList(await _bloodGroupManager.GetAllAsync(), "Id", "Name", vm.BloodGroupId).ToList();

        ViewData["PermanentDistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", vm.PermanentDistrictId);
        ViewData["PresentDistrictId"] = new SelectList(await _districtManager.GetAllAsync(), "Id", "Name", vm.PresentDistrictId);
        ViewData["PresentUpazilaId"] = new SelectList(await _upazilaManager.GetAllAsync(), "Id", "Name", vm.PresentUpazilaId);
    }

}

