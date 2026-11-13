#nullable enable
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.Employees;
public class EmployeeEditVM
{
    public int Id { get; set; }

    [Display(Name = "Employee Name"), Required, StringLength(40)]
    public required string EmployeeName { get; set; }

    [Display(Name = "Emp. Name (Bangla)"), Required, StringLength(100)]
    public required string EmployeeNameBangla { get; set; }

    [Display(Name = "Father's Name"), StringLength(100)]
    public string? FatherName { get; set; }

    [Display(Name = "Mother's Name"), StringLength(100)]
    public string? MotherName { get; set; }

    [DataType(DataType.Date)]
    [Required, Display(Name = "Date of Birth")]
    public DateTime DOB { get; set; }

    public string? Image { get; set; }

    [Display(Name = "Gender")]
    public int GenderId { get; set; }

    [Display(Name = "Religion")]
    public int ReligionId { get; set; }

    [Display(Name = "Nationality")]
    public int NationalityId { get; set; }

    [Display(Name = "National ID")]
    [Range(100000000, 999999999999999999)]
    public long NIDNo { get; set; }

    public string? NIDCard { get; set; }

    [Range(01300000000, 01999999999)]
    public required string Phone { get; set; }

    [EmailAddress]
    public required string Email { get; set; }

    [Display(Name = "Nominee Name")]
    public string? Nominee { get; set; }

    [Range(01300000000, 01999999999,ErrorMessage = "Enter a valid phone number.")]
    public string? NomineePhone { get; set; }

    [Display(Name = "Employee Type")]
    public int EmpTypeId { get; set; }

    [Display(Name = "Designation Name")]
    public int DesignationId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Joining Date")]
    public DateTime JoiningDate { get; set; }

    [Display(Name = "Present Address")]
    public required string PresentAddress { get; set; }

    [Display(Name = "Upazila")]
    public int PresentUpazilaId { get; set; }

    [Display(Name = "District")]
    public int PresentDistrictId { get; set; }

    [Display(Name = "Division")]
    public int PresentDivisionId { get; set; }

    [Display(Name = "Permanent Address")]
    public string? PermanentAddress { get; set; }

    [Display(Name = "Upazila")]
    public int PermanentUpazilaId { get; set; }

    [Display(Name = "District")]
    public int PermanentDistrictId { get; set; }

    [Display(Name = "Division")]
    public int PermanentDivisionId { get; set; }

    public ICollection<AttachDoc>? Documents { get; set; }

    //[Display(Name = "Created By")]
    //public required string CreatedBy { get; set; }

    //[Display(Name = "Created At")]
    //public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated By")]
    public string? EditedBy { get; set; }

    [Display(Name = "Updated At")]
    public DateTime EditedAt { get; set; }

    [Display(Name = "Active/Inactive")]
    public bool Status { get; set; }

    [Display(Name = "Blood Group")]
    public int BloodGroupId { get; set; }



    public List<SelectListItem>? GenderList { get; set; }
    public List<SelectListItem>? ReligionList { get; set; }
    public List<SelectListItem>? NationalityList { get; set; }
    public List<SelectListItem>? EmpTypeList { get; set; }
    public List<SelectListItem>? DesignationList { get; set; }
    public List<SelectListItem>? DivisionList { get; set; }
    public List<SelectListItem>? BloodGroupList { get; set; }
}
