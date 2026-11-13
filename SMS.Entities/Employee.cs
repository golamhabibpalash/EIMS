using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.Entities
{
    public class Employee : CommonProps
    {
        [Display(Name = "Employee Name"),Required,StringLength(40)]
        public string EmployeeName { get; set; }

        [Display(Name = "Emp. Name (Bangla)"),Required,StringLength(100)]
        public string EmployeeNameBangla { get; set; }

#pragma warning disable CS8632
        [Display(Name = "Father's Name"),StringLength(100)]
        public string? FatherName { get; set; }

        [Display(Name = "Mother's Name"),StringLength(100)]
        public string? MotherName { get; set; }
#pragma warning restore CS8632

        [DataType(DataType.Date)]
        [Required, Display(Name ="Date of Birth")]
        public DateTime DOB { get; set; }

        public int BloodGroupId { get; set; }

        public string Image { get; set; }

        [Display(Name ="Gender")]
        public int GenderId { get; set; }

        [Display(Name = "Religion")]
        public int ReligionId { get; set; }

        [Display(Name = "Nationality")]
        public int NationalityId { get; set; }

        [Display(Name ="National ID")]
        [Range(100000000,999999999999999999)]
        public long NIDNo { get; set; }

        [Display(Name ="NID Card")]
        public string NIDCard { get; set; }

        [Range(01300000000, 01999999999)] 
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Display(Name ="Nominee Name")]
        public string Nominee { get; set; }

        [Range(01300000000, 01999999999), Display(Name ="Nominee Phone")]
        public long NomineePhone { get; set; }

        [Display(Name = "Employee Type")]
        public int EmpTypeId { get; set; }
        
        [Display(Name = "Designation Name")]
        public int DesignationId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Present Address")]
        public string PresentAddress { get; set; }

        [Display(Name = "Upazila")]
        public int PresentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PresentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int? PresentDivisionId { get; set; }

        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }

        [Display(Name = "Upazila")]
        public int PermanentUpazilaId { get; set; }

        [Display(Name = "District")]
        public int PermanentDistrictId { get; set; }

        [Display(Name = "Division")]
        public int PermanentDivisionId { get; set; }

        public ICollection<AttachDoc> Documents { get; set; }

        [Display(Name ="Active/Inactive")]
        public bool Status { get; set; }

        [ForeignKey("PresentDivisionId")]
        public Division PresentDivision { get; set; }

        [ForeignKey("PresentDistrictId")]
        public District PresentDistrict { get; set; }

        [ForeignKey("PresentUpazilaId")]
        public Upazila PresentUpazila { get; set; }

        [ForeignKey("PermanentDivisionId")]
        public Division PermanentDivision { get; set; }

        [ForeignKey("PermanentDistrictId")]
        public District PermanentDistrict { get; set; }

        [ForeignKey("PermanentUpazilaId")]
        public Upazila PermanentUpazila { get; set; }

        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
        public Nationality Nationality { get; set; }
        public EmpType EmpType { get; set; }
        public Designation Designation { get; set; }
        public BloodGroup BloodGroup { get; set; }

        public List<Attendance> Attendances { get; set; }

        [Display(Name = "Machine User ID")]
        [StringLength(20)]
        public string MachineUserId { get; set; }
    }
}
