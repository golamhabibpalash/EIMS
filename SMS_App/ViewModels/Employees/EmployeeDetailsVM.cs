using SMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.Employees
{
    public class EmployeeDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "Employee Name"), Required, StringLength(40)]
        public string EmployeeName { get; set; }

        [Display(Name = "Emp. Name (Bangla)"), Required, StringLength(100)]
        public string EmployeeNameBangla { get; set; }


        [DataType(DataType.Date)]
        [Required, Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        public string Image { get; set; }

        public Gender Gender { get; set; }

        public Religion Religion { get; set; }

        public Nationality Nationality { get; set; }

        [Display(Name = "National ID")]
        [Range(100000000, 999999999999999999)]
        public long NIDNo { get; set; }

        public string NIDCard { get; set; }

        [Range(01300000000, 01999999999)]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Nominee Name")]
        public string Nominee { get; set; }

        [Range(01300000000, 01999999999)]
        public string NomineePhone { get; set; }

        [Display(Name = "Employee Type")]
        public int EmpTypeId { get; set; }

        [Display(Name = "Designation Name")]
        public Designation Designation { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Present Address")]
        public string PresentAddress { get; set; }

        [Display(Name = "Present Upazila")]
        public Upazila PresentUpazila { get; set; }

        [Display(Name = "Present District")]
        public District PresentDistrict { get; set; }

        [Display(Name = "Present Division")]
        public Division PresentDivision { get; set; }

        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }

        [Display(Name = "Permanent Upazila")]
        public Upazila PermanentUpazila { get; set; }

        [Display(Name = "Permanent District")]
        public District PermanentDistrict { get; set; }

        [Display(Name = "Division")]
        public Division PermanentDivision { get; set; }

        public ICollection<AttachDoc> Documents { get; set; }


        [Display(Name = "Active/Inactive")]
        public bool Status { get; set; }

        [Display(Name = "Blood Group")]
        public BloodGroup BloodGroup { get; set; }

    }
}
