using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class OTPVM
    {
        public int OTP { get; set; }
        public string Link { get; set; }
        public required string Email { get; set; }
    }
}
