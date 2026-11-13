using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class AppliedStudent : CommonProps
    {
        [Display(Name = "Student Name")]
        public string Name { get; set; }

        [Display(Name = "Student Name (বাংলা)")]
        public string NameBangla { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }

        [Display(Name = "Father's Name (বাংলা)")]
        public string FatherNameBangla { get; set; }

        [Display(Name = "Father's NID")]
        public string FatherNID { get; set; }

        [Display(Name = "Occupation")]
        public string FatherOccupation { get; set; }

        [Display(Name = "Monthly Income")]
        public string FatherMonthlyIncome { get; set; }

        [Display(Name = "Phone No"), StringLength(11)]
        public string FatherPhoneNo { get; set; }

        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Mother's Name (বাংলা)")]
        public string MotherNameBangla { get; set; }

        [Display(Name = "Mother's NID")]
        public string MotherNID { get; set; }

        [Display(Name = "Occupation")]
        public string MotherOccupation { get; set; }

        [Display(Name = "Monthly Income")]
        public string MotherMonthlyIncome { get; set; }

        [Display(Name = "Phone No"), StringLength(11)]
        public string MotherPhoneNo { get; set; }

        public string Email { get; set; }

        public string Photo { get; set; }

        [Display(Name = "Birth Certificate No"), StringLength(17, MinimumLength = 17)]
        public string BirthCertificateNo { get; set; }

        [Display(Name = "Religion")]
        public int? ReligionId { get; set; }

        [Display(Name = "Gender")]
        public int? GenderId { get; set; }

        [Display(Name = "Blood Group")]
        public int? BloodGroupId { get; set; }

        [Display(Name = "Nationality")]
        public int? NationalityId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PresentAddressArea { get; set; }

        [Display(Name = "Address Info")]
        public string AddressInfo { get; set; } = String.Empty;

        [Display(Name = "Post Office")]
        public string PresentAddressPO { get; set; }

        [Display(Name = "Upazila")]
        public int? PresentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int? PresentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int? PresentDivisionId { get; set; }

        [Display(Name = "Vill/Area")]
        public string PermanentAddressArea { get; set; }

        [Display(Name = "Post Office")]
        public string PermanentAddressPO { get; set; }

        [Display(Name = "Upazila")]
        public int? PermanentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int? PermanentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int? PermanentDivisionId { get; set; }

        [Display(Name = "Session")]
        public int? AcademicSessionId { get; set; }
        public string AppliedStudentStatus { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }
        public int? PreviousSchoolClassId { get; set; }
        public int InterestedAppliedClassId { get; set; }
        public string AimInLife { get; set; }
        public bool Status { get; set; } = true;
        public Division PresentDivision { get; set; }
        public District PresentDistrict { get; set; }
        public Upazila PresentUpazila { get; set; }
        public Division PermanentDivision { get; set; }
        public District PermanentDistrict { get; set; }
        public Upazila PermanentUpazila { get; set; }
        public AcademicSession AcademicSession { get; set; }
        public Nationality Nationality { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
    }
}
