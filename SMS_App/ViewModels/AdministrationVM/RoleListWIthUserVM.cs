using Microsoft.AspNetCore.Identity;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class RoleListWIthUserVM
    {
        public IdentityRole IdentityRole { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}
