using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Entities.AdditionalModels
{
    public class StudentPaymentSchedulePaidVM
    {
        public string PaymentType { get; set; }
        public int PaymentCount { get; set; }
        public double PaidAmount { get; set; }
        public int FeeHeadSL { get; set; }
    }
}
