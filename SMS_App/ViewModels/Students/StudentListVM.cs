using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.Students
{
    public class StudentListVM
    {
        public int Id { get; set; }

        [Display(Name = "Student Name"), StringLength(30), Required]
        public string Name { get; set; }
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

        [Display(Name = "Image")]
        public string Photo { get; set; }

        [MinimumAge(10)]
        [Display(Name = "Date of Birth*"), DataType(DataType.Date)]
        [Required(ErrorMessage = "Minimum Age(10) is Requered")]
        public DateTime DOB { get; set; }

        [Display(Name = "Religion")]
        public int ReligionId { get; set; }

        [Display(Name = "Gender")]
        public int GenderId { get; set; }

        [Display(Name = "Blood Group")]
        public int BloodGroupId { get; set; }

        [Display(Name = "Nationality")]
        public int NationalityId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PresentAddressArea { get; set; }

        [Display(Name = "Post Office")]
        public string PresentAddressPO { get; set; }

        [Display(Name = "Upazila/Police Station")]
        public int PresentUpazilaId { get; set; }

        [Display(Name = "Present District")]
        public int PresentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int PresentDivisiontId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PermanentAddressArea { get; set; }

        [Display(Name = "Post Office")]
        public string PermanentAddressPO { get; set; }

        [Display(Name = "Upazila/Police Station")]
        public int PermanentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PermanentDistrictId { get; set; }


        [Display(Name = "Academic Session")]
        public int AcademicSessionId { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }

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

        [ForeignKey("PresentDivisiontId")]
        public Division PresentDivision { get; set; }

        [ForeignKey("PresentDistrictId")]
        public District PresentDistrict { get; set; }

        [ForeignKey("PresentUpazilaId")]
        public Upazila PresentUpazila { get; set; }

        [ForeignKey("PermanentDivisiontId")]
        public Division PermanentDivision { get; set; }

        [ForeignKey("PermanentDistrictId")]
        public District PermanentDistrict { get; set; }

        [ForeignKey("PermanentUpazilaId")]
        public Upazila PermanentUpazila { get; set; }

        public AcademicSession AcademicSession { get; set; }
        public Nationality Nationality { get; set; }
        public AcademicSection AcademicSection { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
    }
}
