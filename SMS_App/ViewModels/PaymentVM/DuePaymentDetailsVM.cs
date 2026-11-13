using SMS.Entities;

namespace SMS_App.ViewModels.PaymentVM
{
    public class DuePaymentDetailsVM
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public double TotalDue { get; set; }
    }
}
