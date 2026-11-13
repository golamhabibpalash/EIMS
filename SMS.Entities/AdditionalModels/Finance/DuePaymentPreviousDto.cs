using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels.Finance;

public class DuePaymentPreviousDto
{
    public int AcademicClassId { get; set; }
    public int AcademicSectionId { get; set; }
    public int StudentId { get; set; }
    public string Status { get; set; }

    public AcademicClass AcademicClass { get; set; }
    public AcademicSection AcademicSection { get; set; }
    public Student Student { get; set; }
    public Institute Institute { get; set; }

    public List<DuePayment> DuePayments { get; set; }

    public List<SelectListItem> AcademicClassList { get; set; }
    public List<SelectListItem> AcademicSectionList { get; set; }
    public List<SelectListItem> StudentList { get; set; }
}
public class DuePayment
{
    public Student Student { get; set; }
    public double TotalDue { get; set; }
}
public class PreviouisPaymentDetailsDto
{
    public string UniqueId { get; set; }
    public int StudentId { get; set; }
    public int? AcademicSectionId { get; set; }
    public int CurrentClassId { get; set; }
    public double PayableAmount { get; set; }
    public double PaidAmount { get; set; }
    public double DueAmount { get; set; }
    public bool Status { get; set; }
}

public class DuePaymentBulkResult
{
    public List<Student> Students { get; set; }
    public List<ClassFeeList> ClassFees { get; set; }
    public List<StudentFeeAllocation> Allocations { get; set; }
    public List<StudentPayment> Payments { get; set; }
    public AcademicSession CurrentSession { get; set; }
}