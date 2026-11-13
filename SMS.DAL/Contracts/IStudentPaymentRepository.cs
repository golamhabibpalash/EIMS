using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.Entities.AdditionalModels.Finance;

namespace SMS.DAL.Contracts;

public interface IStudentPaymentRepository : IRepository<StudentPayment>
{
    Task<IReadOnlyCollection<StudentPayment>> GetAllByStudentIdAsync(int id);
    Task<List<StudentPaymentSummeryVM>> GetPaymentSummeryByMonthYear(string monthYear);
    Task<List<StudentPaymentSummeryVM>> GetPaymentSummeryByDate(string date);
    Task<List<StudentPaymentScheduleVM>> GetStudentPaymentSchedule(int studId);
    Task<List<StudentPaymentSchedulePaidVM>> GetStudentPaymentSchedulePaid(int studId);
    Task<List<StudentPaymentSummerySMS_VM>> GetStudentPaymentSummerySMS_VMsAsync(DateTime date);
    Task<IEnumerable<StudentPayment>> GetAllByStudentUniqueIdAsync(string uniqueId);
    Task<List<PaidAmountResult>> GetPaidAmountByFeeHead(string uniqueId, int sessionId, int isResidential, int classId, int feeHeadId);
    Task<List<PreviouisPaymentDetailsDto>> GetAllStudentsPaymentSummeryAsync();
    Task<DuePaymentBulkResult> GetBulkDuePaymentsAsync(int sessionId, List<int> studentIds);
}
