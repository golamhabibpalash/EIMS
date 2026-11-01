using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities.Enums;

namespace SMS_App.ViewModels.TransferCertificates
{
    public class TransferCertificateCreateVM
    {
        [Required(ErrorMessage = "Please select a student."), Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Certificate number is required."), StringLength(50), Display(Name = "Certificate No")]
        public string CertificateNo { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Date of Issue")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date), Display(Name = "Date of Leaving")]
        public DateTime? LeavingDate { get; set; }

        [StringLength(500), Display(Name = "Reason for Leaving")]
        public string Reason { get; set; }

        [StringLength(500), Display(Name = "Conduct / Character")]
        public string Conduct { get; set; }

        [StringLength(1000), Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Status")]
        public TransferCertificateStatus Status { get; set; } = TransferCertificateStatus.Draft;

        public IEnumerable<SelectListItem> StudentList { get; set; } = new List<SelectListItem>();
    }

    public class TransferCertificateEditVM : TransferCertificateCreateVM
    {
        public int Id { get; set; }

        // Snapshot captured at save time - shown read-only on the edit screen.
        public string StudentNameSnapshot { get; set; }
        public string UniqueIdSnapshot { get; set; }
        public string ClassNameSnapshot { get; set; }
        public int ClassRollSnapshot { get; set; }
        public string SessionNameSnapshot { get; set; }
    }
}
