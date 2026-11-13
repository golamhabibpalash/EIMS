using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class LoginVm
    {
        [Required]
        [Display(Name = "User")]
        public string AppUser { get; set; }

        [DataType(DataType.Password), Required]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        [HiddenInput(DisplayValue = false)]
        public string ReturnUrl { get; set; }
        public string Error { get; set; }

    }
}
