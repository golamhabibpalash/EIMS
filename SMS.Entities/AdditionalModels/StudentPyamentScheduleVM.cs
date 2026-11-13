using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Entities.AdditionalModels
{
    public class StudentPaymentScheduleVM
    {
        public string PaymentType { get; set; }
        public double Amount { get; set; }
        public int yearlyFrequency { get; set; } = 0;
        public bool IsResidential { get; set; }
        public int SL { get; set; }
        public int FeeHeadId { get; set; }
        public int ClassFeeId { get; set; }
    }
}