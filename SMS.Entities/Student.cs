using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SMS.Entities
{
    public class Student : CommonProps
    {
        [Display(Name ="Student Name")]
        public string Name { get; set; }

        [Display(Name ="Student Name (Bangla)")]
        public string NameBangla { get; set; }

        [Display(Name = "Class Roll"), NotNull]
        public int ClassRoll { get; set; }

        [Display(Name = "Academic Class")]
        public int AcademicClassId { get; set; }

        [Display(Name = "Section")]
        public int? AcademicSectionId { get; set; }

        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }

        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; }

        public string Email { get; set; }

        [Display(Name = "Phone No"), StringLength(11)]
        public string PhoneNo { get; set; }

        [Display(Name = "Guardian Phone"), StringLength(11)]        
        public string GuardianPhone { get; set; }

        public string Photo { get; set; }

        public DateTime DOB { get; set; }

        [Display(Name ="Birth Certificate No"), StringLength(17, MinimumLength =17)]
        public string BirthCertificateNo { get; set; }

        [Display(Name ="Birth Certificate Image")]
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

        [Display(Name = "Upazila")]
        public int PresentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PresentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int? PresentDivisionId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PermanentAddressArea { get; set; }

        [Display(Name = "Post Office")]
        public string PermanentAddressPO { get; set; }

        [Display(Name = "Upazila")]
        public int PermanentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PermanentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int? PermanentDivisionId { get; set; }

        [Display(Name = "Session")]
        public int AcademicSessionId { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }
        public bool IsResidential { get; set; } = false;
        public bool SMSService { get; set; } = true;

        public ICollection<AttachDoc> Documents { get; set; }

        public bool Status { get; set; } = true;

        public Division PresentDivision { get; set; }

        public District PresentDistrict { get; set; }

        public Upazila PresentUpazila { get; set; }

        public Division PermanentDivision { get; set; }

        public District PermanentDistrict { get; set; }

        public Upazila PermanentUpazila { get; set; }

        public AcademicSession AcademicSession { get; set; }
        public Nationality  Nationality{ get; set; }
        public AcademicSection AcademicSection { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }

        [NotNull, Required]
        public string UniqueId { get; set; }

        public List<Attendance> Attendances { get; set; }
        //public string LastAction { get; set; } = string.Empty;

        [Display(Name = "Machine User ID")]
        [StringLength(20)]
        public string MachineUserId { get; set; }
    }
}
