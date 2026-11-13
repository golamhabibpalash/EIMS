using System.Collections.Generic;

namespace SMS_App.ViewModels.PaymentVM
{
    public class PaymentVM
    {
        public List<SinglePaymentVM> Payments { get; set; } = new List<SinglePaymentVM>();
    }
    public class SinglePaymentVM
    {
        public string PaymentsTitle { get; set; }
        public string AcademicSession { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPaidAmount { get; set; }
        public double TotalDueAmount { get; set; }
        public List<SessionWisePaymentVM> SessionWisePaymentVMs { get; set; } = new List<SessionWisePaymentVM>();
    }

    public class SessionWisePaymentVM
    {
        public string FeeHeadName { get; set; }
        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public double Balance { get; set; }
        public string Status { get; set; }
        public List<SessionWisePaymentDetails> SessionWisePaymentDetails { get; set; } = new List<SessionWisePaymentDetails>();
    }

    public class SessionWisePaymentDetails
    {
        public string PaidDate { get; set; }
        public string ReceiptNo { get; set; }
        public string PaidAmount { get; set; }
        public string DueAmount { get; set; }
        public string Status { get; set; }
    }
}
