using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.Students
{
    public class StudentFeeAllocationVM
    {
        public List<StudentFeeAllocation> StudentFeeAllocations { get; set; } = new List<StudentFeeAllocation>();
        public StudentFeeAllocation SFAllocation { get; set; } = new StudentFeeAllocation();
        public SelectList FeeList { get; set; }
        public SelectList AcademicClassList { get; set; }
        public SelectList AcademicSectionList { get; set; }
        public AcademicSession CurrentSession { get; set; }
        public Dictionary<string, string> UsersDictionary { get; set; } = new Dictionary<string, string>();
    }

    public class DataTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<StudentFeeAllocation> data { get; set; } = new List<StudentFeeAllocation>();
    }
}
