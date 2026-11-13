using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace SMS_App.ViewModels.ModuleSubModuleVM
{
    public class ProjectSubModuleVM
    {
        public ProjectSubModule ProjectSubModule { get; set; }
        public List<ProjectSubModule> SubModuleList { get; set; }

        [Display(Name = "Module Name")]
        public SelectList ModuleSelectList { get; set; }
    }
}
