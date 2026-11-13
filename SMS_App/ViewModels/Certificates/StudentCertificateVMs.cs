using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using SMS.Entities.Enums;

namespace SMS_App.ViewModels.Certificates
{
    /// <summary>Backs the certificate list page (shared by both certificate types).</summary>
    public class StudentCertificateIndexVM
    {
        public CertificateType Type { get; set; }
        public string ControllerName { get; set; }
        public string Title { get; set; }

        public List<StudentCertificate> Items { get; set; } = new();

        // Compact filter state.
        public int? AcademicClassId { get; set; }
        public int? AcademicSectionId { get; set; }
        public string SearchString { get; set; }
    }

    /// <summary>Create/edit form for a certificate record (shared by both types).</summary>
    public class StudentCertificateFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a student."), Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date), Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        // Transfer-certificate-only fields.
        [DataType(DataType.Date), Display(Name = "Date of Leaving")]
        public DateTime? LeavingDate { get; set; }

        [StringLength(500), Display(Name = "Reason for Leaving")]
        public string Reason { get; set; }

        [StringLength(100), Display(Name = "Exam Name")]
        public string ExamName { get; set; }

        [StringLength(50), Display(Name = "Total Marks")]
        public string TotalMarks { get; set; }

        [StringLength(50), Display(Name = "GPA")]
        public string Gpa { get; set; }

        [StringLength(50), Display(Name = "Grade")]
        public string Grade { get; set; }

        [StringLength(50), Display(Name = "Merit Rank")]
        public string MeritRank { get; set; }

        [StringLength(150), Display(Name = "Signatory Name")]
        public string SignatoryName { get; set; }

        [StringLength(100), Display(Name = "Designation")]
        public string SignatoryDesignation { get; set; } = "Headmaster";

        public CertificateType Type { get; set; }
        public string ControllerName { get; set; }
        public string Title { get; set; }
        public bool IsTransfer => Type == CertificateType.TransferCertificate;

        public IEnumerable<SelectListItem> StudentList { get; set; } = new List<SelectListItem>();
    }
}
