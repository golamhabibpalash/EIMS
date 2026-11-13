using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.Finance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.Entities.AdditionalModels.StudentVM;

namespace SMS.BLL.Contracts
{
    public interface IStudentPaymentManager : IManager<StudentPayment>
    {
        Task<IReadOnlyCollection<StudentPayment>> GetAllByStudentIdAsync(int id);
        Task<string> GetNewReceipt(int studentId, int feeHeadId);
        Task<IReadOnlyCollection<StudentPaymentSummeryVM>> GetPaymentSummeryByMonthYear(string monthYear);
        Task<IReadOnlyCollection<StudentPaymentSummeryVM>> GetPaymentSummeryByDate(string date);
        Task<List<StudentPaymentScheduleVM>> GetStudentPaymentSchedule(int studId);
        Task<List<StudentPaymentSchedulePaidVM>> GetStudentPaymentSchedulePaid(int studId);
        Task<List<StudentPaymentSummerySMS_VM>> GetStudentPaymentSummerySMS_VMsAsync(DateTime date);
        Task<double> GetStudentCurrentDue(int stuId);
        Task<List<StudentPayment>> GetPaymentByStudentUniqueId(string uniqueId);
        Task<List<PaidAmountResult>> GetPaidAmountByFeeHeadAsync(string uniqueId, int sessionId, int isResidential, int classId, int feeHeadId);
        Task<StudentPaymentDetailVM> GetAllDetailPaymentByUniqueId(string studentUniqueId);
        Task<List<DuePayment>> GetPreviousDuesAsync(int? studentId, int? sectionId, int academicClassId, bool? status);
        Task<ProfilePayment> GetProfilePaymentAsync(int studentId);
        Task<List<DuePaymentDetails>> GetBulkDuePaymentsAsync(int sessionId, List<Student> students);
    }
}

public class DuePaymentDetails
{
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public double TotalDue { get; set; }
}
