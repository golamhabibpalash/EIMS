using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMS.Entities.Enums;

namespace SMS.Entities
{
    /// <summary>
    /// A requested/issued student certificate (Testimonial or Transfer Certificate). A record is
    /// created when a student applies or an admin adds one; staff then update, view and print it.
    /// The student's identifying fields are SNAPSHOT at save time so a printed certificate never
    /// changes if the student record is later edited. Exam fields apply to Transfer Certificates.
    /// </summary>
    public class StudentCertificate : CommonProps
    {
        [Display(Name = "Type")]
        public CertificateType Type { get; set; }

        [Required, Display(Name = "Student")]
        public int StudentId { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        // --- Transfer-Certificate-specific ---
        [Display(Name = "Date of Leaving")]
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

        // --- Signatory ---
        [StringLength(150), Display(Name = "Signatory Name")]
        public string SignatoryName { get; set; }

        [StringLength(100), Display(Name = "Designation")]
        public string SignatoryDesignation { get; set; } = "Headmaster";

        // --- Snapshot of the student at save time ---
        public string StudentNameSnapshot { get; set; }
        public string FatherNameSnapshot { get; set; }
        public string MotherNameSnapshot { get; set; }
        public DateTime DobSnapshot { get; set; }
        public string ClassNameSnapshot { get; set; }
        public string SectionNameSnapshot { get; set; }
        public int ClassRollSnapshot { get; set; }
        public string SessionNameSnapshot { get; set; }
        public string UniqueIdSnapshot { get; set; }
        public string GenderNameSnapshot { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
    }
}
