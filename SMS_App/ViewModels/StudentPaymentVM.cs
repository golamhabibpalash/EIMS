using SMS_App.ViewModels.PaymentVM;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System.Collections.Generic;

namespace SMS_App.ViewModels
{
    public class StudentPaymentVM
    {
        public int Id { get; set; }
        public StudentPayment StudentPayment { get; set; }
        public List<StudentPayment> StudentCurrentPayments { get; set; } = new List<StudentPayment>();
        public List<StudentPayment> StudentPreviousPayments { get; set; } = new List<StudentPayment>();
        public List<ClassFeeList> ClassFeeLists { get; set; }
        public int ClassFeeHeadId { get; set; }
        public int StudentId { get; set; }
        public bool IsSMSSend { get; set; }
        public AcademicSession CurrentAcademicSession { get; set; }
        public int SelectedSessionId { get; set; }
        public int SelectedClassId { get; set; }
        public List<AcademicSession> AvailableSessions { get; set; } = new List<AcademicSession>();
        public List<AcademicClass> AvailableClasses { get; set; } = new List<AcademicClass>();

        public StudentPaymentDetailVM PaymentVM { get; set; } = new StudentPaymentDetailVM();
    }
}
