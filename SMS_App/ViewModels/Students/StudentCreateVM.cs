using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.Students;

public class StudentCreateVM
{

    [Display(Name = "Student Name*"), StringLength(30), Required(ErrorMessage ="Student Name field is required")]
    public string Name { get; set; }

    [Display(Name ="Student Name(Bangla)")]
    public string NameBangla { get; set; }

    [Display(Name = "Class Roll*"),Range(1,999, ErrorMessage ="Please enter a valid roll")]
    public int ClassRoll { get; set; }

    [Display(Name = "Class*"),Required (ErrorMessage ="Select Academic Class")]
    public int AcademicClassId { get; set; }

    [Display(Name = "Section")]
    public int? AcademicSectionId { get; set; }

    [Display(Name = "Father's Name"), StringLength(30)]
    public string FatherName { get; set; }

    [Display(Name = "Mother's Name"), StringLength(30)]
    public string MotherName { get; set; }

    [Display(Name = "Addmission Date*"), DataType(DataType.Date)]
    public DateTime AdmissionDate { get; set; }

    [Display(Name = "Email Address"), EmailAddress]
    public string Email { get; set; }

    [Display(Name = "Phone*"), Required(ErrorMessage = "Phone field is required"), StringLength(11, MinimumLength =11, ErrorMessage = "Phone Number allow only 11 digit")]
    [Range(01300000000, 01999999999, ErrorMessage ="Please Insert a valid phone number")]
    public string PhoneNo { get; set; }

    [Display(Name = "Guardian Phone"), StringLength(11, MinimumLength =11, ErrorMessage = "Phone Number allow only 11 digit")]
    [Range(01300000000, 01999999999, ErrorMessage = "Please Insert a valid phone number")]
    public string GuardianPhone { get; set; }

    [Display(Name = "Student Image")]
    public string Photo { get; set; }

    //[MinimumAge(10)]
    [Display(Name = "Date of Birth*"), DataType(DataType.Date)]
    public DateTime DOB { get; set; }

    [Display(Name = "Birth Certificate No"), StringLength(17, MinimumLength = 17,ErrorMessage = "Please Insert a 17 digit valid number")]
    public string BirthCertificateNo { get; set; }

    [Display(Name = "Birth Certificate Image")]
    public string BirthCertificateImage { get; set; }

    [Display(Name = "Religion"), Required(ErrorMessage = "Select religion from the list")]
    public int ReligionId { get; set; }

    [Display(Name = "Gender")]
    public int GenderId { get; set; }

    [Display(Name = "Blood Group")]
    public int BloodGroupId { get; set; }

    [Display(Name = "Nationality"), Required(ErrorMessage = "Select Nationality from the list")]
    public int NationalityId { get; set; }

    [Display(Name = "Vill/Area")]
    public string PresentAddressArea { get; set; }

    [Display(Name = "Address Info")]
    public string AddressInfo { get; set; } = String.Empty;

    [Display(Name = "Post Office")]
    public string PresentAddressPO { get; set; }

    [Display(Name = "Upazila/Police Station*")]
    public int PresentUpazilaId { get; set; }

    [Display(Name = "Present District*")]
    public int PresentDistrictId { get; set; }

    [Display(Name = "Present Division*")]
    public int PresentDivisionId { get; set; }

    [Display(Name = "Vill/Area")]
    public string PermanentAddressArea { get; set; }

    [Display(Name = "Post Office")]
    public string PermanentAddressPO { get; set; }

    [Display(Name = "Upazila/Police Station*")]
    public int PermanentUpazilaId { get; set; }

    [Display(Name = "Permanent District*")]
    public int PermanentDistrictId { get; set; }

    [Display(Name = "Permanent Division*")]
    public int PermanentDivisionId { get; set; }

    [Display(Name = "Academic Session*"), Required(ErrorMessage = "Select Academic Session fromt list")]
    public int AcademicSessionId { get; set; }

    [Display(Name = "Previous School")]
    public string PreviousSchool { get; set; }
    public bool IsResidential { get; set; } = false;
    [Display(Name ="SMS Service")]
    public bool SMSService { get; set; } = true;
    public ICollection<AttachDoc> Documents { get; set; }

    [Display(Name = "Active/Inactive")]
    public bool Status { get; set; }

    [Display(Name = "Created By")]
    public string CreatedBy { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Edited By")]
    public string EditedBy { get; set; }

    [Display(Name = "Edited at")]
    public DateTime EditedAt { get; set; }

    [Display(Name = "Unique ID")]
    public string UniqueId { get; set; }

    public List<SelectListItem> AcademicSessionList { get; set; }
    public List<SelectListItem> AcademicClassList { get; set; }
    public List<SelectListItem> BloodGroupList { get; set; }
    public List<SelectListItem> GenderList { get; set; }
    public List<SelectListItem> NationalityList { get; set; }
    public List<SelectListItem> ReligionList { get; set; }
    public List<SelectListItem> DivisionList { get; set; }
}
