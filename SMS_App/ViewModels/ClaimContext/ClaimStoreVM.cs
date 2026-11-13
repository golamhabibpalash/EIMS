using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace SMS_App.ViewModels.ClaimContext
{
    public class ClaimStoreVM
    {
        
        public ClaimStores ClaimStores { get; set; }
        public List<ClaimStores> ClaimStoresList { get; set; }

        [Display(Name = "Sub Module")] 
        public SelectList SubModuleSelectList { get; set; }

        [Display(Name = "Module")] 
        public SelectList ModuleSelectList { get; set; }
    }
}
