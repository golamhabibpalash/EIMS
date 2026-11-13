using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class EditRoleVM
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public List<string> ApplicationUsers { get; set; }
    }
}
