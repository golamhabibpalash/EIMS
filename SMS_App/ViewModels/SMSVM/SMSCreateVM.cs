using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.SMSVM
{
    public class SMSCreateVM
    {
        public string SMSFor { get; set; }
        public string SMSType { get; set; }
        public int DesignationId { get; set; }
        public int AcademicSessionId { get; set; }
        public int AcademicClassId { get; set; }
        public int EmployeeId { get; set; }
        public int StudentId { get; set; }
        public string SMSText { get; set; }
        public string PhoneNo { get; set; }
    }
}
