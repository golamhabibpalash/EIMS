using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels
{
    public class StudentPaymentDetailVM
    {
        public List<SinglePaymentVM> Payments { get; set; } = new List<SinglePaymentVM>();
    }

    public class SinglePaymentVM
    {
        public string CurrentSession { get; set; }
        public string PaymentsTitle { get; set; }
        public string AcademicSession { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPaidAmount { get; set; }
        public double TotalDueAmount { get; set; }
        public List<SessionWisePaymentVM> SessionWisePaymentVMs { get; set; } = new List<SessionWisePaymentVM>();
    }

    public class SessionWisePaymentVM
    {
        public int PaymentId { get; set; }
        public string FeeHeadName { get; set; }
        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public double Balance { get; set; }
        public string Status { get; set; }
        public List<SessionWisePaymentDetails> SessionWisePaymentDetails { get; set; } = new List<SessionWisePaymentDetails>();
    }

    public class SessionWisePaymentDetails
    {
        public int PaymentDetailId { get; set; }
        public string PaidDate { get; set; }
        public string ReceiptNo { get; set; }
        public double PayableAmount { get; set; }
        public double PaidAmount { get; set; }
        public double DueAmount { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public int PaymentId { get; set; }
    }
}
