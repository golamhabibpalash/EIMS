using System;

namespace SMS_App.ViewModels.PaymentVM
{
    public class PaymentItemDetailVM
    {
        public int PaymentDetailId { get; set; }
        public int PaymentId { get; set; }
        public string Receipt { get; set; }
        public double PaymentAmount { get; set; }
        public DateTime  PaidDate { get; set; }
        public string ReceivedBy { get; set; }
        public string PaidMode { get; set; }
        public int StudentFeeHeadId { get; set; }
        public string PaymentRemarks { get; set; }
    }
}
