using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.Students
{
    public class StudentEditVM
    {
        public int Id { get; set; }

        [Display(Name = "Student Name"), StringLength(30), Required]
        public string Name { get; set; }

        [Display(Name ="Student Name (Bangla)")]
        public string NameBangla { get; set; }

        [Display(Name = "Class Roll")]
        public int ClassRoll { get; set; }

        [Display(Name = "Class")]
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

        [Display(Name = "Phone")]
        [Range(01300000000, 01999999999)]
        public long PhoneNo { get; set; }

        [Display(Name = "Guardian Phone")]
        [Range(01300000000, 01999999999)]
        public long GuardianPhone { get; set; }

        [Display(Name = "Student Image")]
        public string Photo { get; set; }


        [Display(Name = "Date of Birth*"), DataType(DataType.Date)]
        [Required(ErrorMessage = "Minimum Age(10) is Requered")]
        public DateTime DOB { get; set; }

        [Display(Name = "Birth Certificate No"), StringLength(17, MinimumLength = 17)]
        public string BirthCertificateNo { get; set; }

        [Display(Name = "Birth Certificate Image")]
        public string BirthCertificateImage { get; set; }

        [Display(Name = "Religion")]
        public int ReligionId { get; set; }

        [Display(Name = "Gender")]
        public int GenderId { get; set; }

        [Display(Name = "Blood Group")]
        public int? BloodGroupId { get; set; }

        [Display(Name = "Nationality")]
        public int NationalityId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PresentAddressArea { get; set; }

        [Display(Name = "Address Info")]
        public string AddressInfo { get; set; } = String.Empty;

        [Display(Name = "Post Office")]
        public string PresentAddressPO { get; set; }

        [Display(Name = "Upazila/Police Station")]
        public int PresentUpazilaId { get; set; }

        [Display(Name = "Present District")]
        public int PresentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int PresentDivisionId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PermanentAddressArea { get; set; }

        [Display(Name = "Post Office")]
        public string PermanentAddressPO { get; set; }

        [Display(Name = "Upazila/Police Station")]
        public int PermanentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PermanentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int PermanentDivisionId { get; set; }

        [Display(Name = "Academic Session")]
        public int AcademicSessionId { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }
        public bool IsResidential { get; set; } = false;
        [Display(Name = "SMS Service")]
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

        [Display(Name = "Unique ID")] 
        public string UniqueId { get; set; }

        [Display(Name = "Edited at")]
        public DateTime EditedAt { get; set; }
        public List<SelectListItem> AcademicSessionList { get; set; }
        public List<SelectListItem> AcademicClassList { get; set; }
        public List<SelectListItem> AcademicSectionList { get; set; }
        public List<SelectListItem> BloodGroupList { get; set; }
        public List<SelectListItem> GenderList { get; set; }
        public List<SelectListItem> NationalityList { get; set; }
        public List<SelectListItem> ReligionList { get; set; }
        public List<SelectListItem> PresentDivisionList { get; set; }
        public List<SelectListItem> PermanentDivisionList { get; set; }
    }
}
