using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class ForgotPasswordVM
    {
        public required string Email { get; set; }
        public string Name { get; set; }
        public string VerificationBy { get; set; }
    }
}
