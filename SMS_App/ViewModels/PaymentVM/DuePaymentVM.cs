using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.PaymentVM
{
    public class DuePaymentVM
    {
        public AcademicClass AcademicClass { get; set; }
        public int AcademicClassId { get; set; }
        public int? AcademicSectionId { get; set; }
        public List<DuePaymentDetailsVM> DuePayments { get; set; }
        public List<SelectListItem> AcademicClassList { get; set; }
        public List<SelectListItem> AcademicSectionList { get; set; }
        public List<SelectListItem> StudentStatusSelectList { get; set; }
        public List<SelectListItem> StudentCategorySelectList { get; set; }
        public Institute Institute { get; set; }
        public double GrandTotal { get; set; }
        public int StudentStatus { get; set; } = 1; //0=inactive, 1=active, 2=all;
        public int ResidentialStatus { get; set; } = 2; //0=NonResidential, 1=Residential, 2=all;
        public int? ShowCount { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
