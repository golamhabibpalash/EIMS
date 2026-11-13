using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ModuleSubModuleVM
{
    public class ProjectModuleVM
    {
        public ProjectModule ProjectModule { get; set; }
        public List<ProjectModule> ProjectModuleList { get; set; } = new List<ProjectModule>();
    }
}
