using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class StudentPayment : CommonProps
    {
        [Display(Name = "Student Name")]
        public int StudentId { get; set; }

        [Display(Name = "Receipt No"), Required]
        public string ReceiptNo { get; set; }

        [Display(Name = "Paid Date"), Required]
        [DataType(DataType.Date)]
        public DateTime PaidDate { get; set; }

        [Display(Name = "Waiver")]
        public double WaiverAmount { get; set; }

        [Display(Name = "Waiver For")]
        public string WaiverFor { get; set; }

        public string Attachment { get; set; }

        [Display(Name = "Total Paid")]
        public double TotalPayment { get; set; }
        public string Remarks { get; set; }


        public Student Student { get; set; }
        [Required]
        public string UniqueId { get; set; }
        public int? AcademicSessionId { get; set; } = 0;
        //public virtual AcademicSession AcademicSession { get; set; }

        public List<StudentPaymentDetails> StudentPaymentDetails { get; set; } = new List<StudentPaymentDetails>();

    }
}
