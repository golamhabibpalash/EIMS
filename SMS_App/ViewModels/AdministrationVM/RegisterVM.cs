using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class RegisterVM
    {
        [Required, Display(Name ="Select User")]
        public int ReferenceId { get; set; }

        public char UserType { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name ="Confirm Password")]
        [Compare("Password", ErrorMessage ="Password and Confirmation Password does not match.")]
        public string ConfirmPassword { get; set; }
    }
}
