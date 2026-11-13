using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace SMS_App.ViewModels.Students;

public class StudentFeeAllocationGroupVM
{
    
    public int FeeHeadId { get; set; }
    public List<StudentRow> Students { get; set; }
    public double AllocationAmount { get; set; }
    public int AcademicSectionId { get; set; }
    public int AcademicSessionId { get; set; }
    public int AcademicClassId { get; set; }
    public StudentFeeAllocation SFAllocation { get; set; } = new StudentFeeAllocation();
    public SelectList AcademicClassList { get; set; }
    public SelectList AcademicSessionList { get; set; }
}

public class StudentRow
{
    public bool IsChecked { get; set; }
    public Student Student { get; set; }
}
