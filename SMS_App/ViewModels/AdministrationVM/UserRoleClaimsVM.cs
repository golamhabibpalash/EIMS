using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AdministrationVM
{
    public class UserRoleClaimsVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public List<SelectListItem> ApplicationRoles { get; set; }
        public List<SelectListItem> ApplicationClaims { get; set; }
    }
}
