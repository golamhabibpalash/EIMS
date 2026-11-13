using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.PaymentVM
{
    public class PaymentItemVM
    {
        public int Id { get; set; }
        public int ClassFeeListId { get; set; }
        public string ClassFeeListName { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public string Status { get; set; } //Paid,Unpaid,Partial
        public List<PaymentItemDetailVM>    PaymentItemDetailVMs { get; set; } = new List<PaymentItemDetailVM>();
        public string UniqueId { get; set; }
    }
}
