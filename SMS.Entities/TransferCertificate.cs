using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMS.Entities.Enums;

namespace SMS.Entities
{
    /// <summary>
    /// An official Transfer Certificate issued when a student leaves the institution. The record
    /// keeps the live <see cref="StudentId"/> FK but also stores a point-in-time SNAPSHOT of the
    /// student's identifying fields, so that a later edit/transfer of the student never alters an
    /// already-issued certificate. See <see cref="TransferCertificateStatus"/> for the lifecycle.
    /// </summary>
    public class TransferCertificate : CommonProps
    {
        [Required, Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required, StringLength(50), Display(Name = "Certificate No")]
        public string CertificateNo { get; set; }

        [Display(Name = "Date of Issue")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Display(Name = "Date of Leaving")]
        public DateTime? LeavingDate { get; set; }

        [StringLength(500), Display(Name = "Reason for Leaving")]
        public string Reason { get; set; }

        [StringLength(500), Display(Name = "Conduct / Character")]
        public string Conduct { get; set; }

        [StringLength(1000), Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Status")]
        public TransferCertificateStatus Status { get; set; } = TransferCertificateStatus.Draft;

        // ---- Point-in-time snapshot of the student, captured when the record is saved ----
        [Display(Name = "Student Name")]
        public string StudentNameSnapshot { get; set; }

        [Display(Name = "Student ID")]
        public string UniqueIdSnapshot { get; set; }

        [Display(Name = "Class")]
        public string ClassNameSnapshot { get; set; }

        [Display(Name = "Roll")]
        public int ClassRollSnapshot { get; set; }

        [Display(Name = "Session")]
        public string SessionNameSnapshot { get; set; }

        [Display(Name = "Admission Date")]
        public DateTime? AdmissionDateSnapshot { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
    }
}
