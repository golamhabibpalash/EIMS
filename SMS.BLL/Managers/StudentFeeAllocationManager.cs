using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
public class StudentFeeAllocationManager : Manager<StudentFeeAllocation>, IStudentFeeAllocationManager
    {
        private readonly IStudentFeeAllocationRepository _feeAllocationRepository;
        private readonly IStudentPaymentDetailsRepository _paymentDetailsRepository;
        private readonly IClassFeeListRepository _classFeeListRepository;
        private readonly IStudentRepository _studentRepository;

        public StudentFeeAllocationManager(
            IStudentFeeAllocationRepository repository,
            IStudentPaymentDetailsRepository paymentDetailsRepository,
            IClassFeeListRepository classFeeListRepository,
            IStudentRepository studentRepository) : base(repository)
        {
            _feeAllocationRepository = repository;
            _paymentDetailsRepository = paymentDetailsRepository;
            _classFeeListRepository = classFeeListRepository;
            _studentRepository = studentRepository;
        }

        /// <summary>
        /// An allocation can be deleted only while no payment has been recorded against the fee it
        /// overrides. Once the student has paid (even 0 for a free head), deleting it would silently
        /// change what they owe for an already-settled fee.
        /// </summary>
        public async Task<(bool canDelete, string message)> CanDeleteAsync(int allocationId)
        {
            var allocation = await _feeAllocationRepository.GetByIdAsync(allocationId);
            if (allocation == null)
            {
                return (false, "This allocation no longer exists. It may have been deleted already.");
            }

            var uniqueId = allocation.UniqueId;
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                uniqueId = (await _studentRepository.GetByIdAsync(allocation.StudentId))?.UniqueId;
            }
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                return (false, "The student for this allocation could not be found, so its payments can't be checked. Deactivate it instead.");
            }

            ClassFeeList classFee = null;
            if (allocation.ClassFeeListId is int classFeeId && classFeeId > 0)
            {
                classFee = await _classFeeListRepository.GetByIdAsync(classFeeId);
            }

            var paymentDetails = await _paymentDetailsRepository.GetAllByStudentAsync(uniqueId);
            var paymentsForThisFee = paymentDetails.Where(d =>
                classFee != null
                    // Payments linked to this exact class fee, or older rows saved without a class
                    // fee link that match the same head in the same session.
                    ? d.ClassFeeId == classFee.Id
                      || (d.ClassFeeId == 0
                          && d.StudentFeeHeadId == allocation.StudentFeeHeadId
                          && ((d.StudentPayment?.AcademicSessionId ?? 0) == 0
                              || d.StudentPayment.AcademicSessionId == classFee.AcademicSessionId))
                    // Legacy allocation with no class fee link: any payment for the same head.
                    : d.StudentFeeHeadId == allocation.StudentFeeHeadId)
                .ToList();

            if (paymentsForThisFee.Count > 0)
            {
                var paid = paymentsForThisFee.Sum(d => d.PaidAmount);
                return (false, $"Cannot delete: {paymentsForThisFee.Count} payment(s) totalling {paid:0.##} have already been recorded for this fee. Deactivate the allocation instead.");
            }

            return (true, string.Empty);
        }

        public async Task<(bool deleted, string message)> DeleteWithValidationAsync(int allocationId)
        {
            var (canDelete, message) = await CanDeleteAsync(allocationId);
            if (!canDelete)
            {
                return (false, message);
            }

            var allocation = await _feeAllocationRepository.GetByIdAsync(allocationId);
            var removed = allocation != null && await _feeAllocationRepository.RemoveAsync(allocation);
            return removed
                ? (true, "Allocation deleted successfully.")
                : (false, "The allocation could not be deleted. Please try again.");
        }

        public Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdClassFeeId(string uniqueId, int classfeeId)
        {
            return _feeAllocationRepository.GetStudentFeeAllocationByUniqueIdClassFeeId(uniqueId, classfeeId);
        }

        public async Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdFeeHeadId(string uniqueId, int feeHeadId)
        {
            return await _feeAllocationRepository.GetStudentFeeAllocationByUniqueIdFeeHeadId(uniqueId, feeHeadId);
        }

        public async Task<(int totalRecord, int filteredRecord, List<StudentFeeAllocation> data)> GetDataTableDataAsync(
            string searchValue, string orderColumn, string orderDirection, int start, int length)
        {
            return await _feeAllocationRepository.GetDataTableDataAsync(searchValue, orderColumn, orderDirection, start, length);
        }

        public override async Task<IReadOnlyCollection<StudentFeeAllocation>> GetAllAsync()
        {
            return await _feeAllocationRepository.Table
                .Include(s => s.ClassFeeList)
                    .ThenInclude(m => m.AcademicSession)
                .Include(c => c.Student)
                    .ThenInclude(l => l.AcademicClass)
                .Include(c => c.StudentFeeHead)
                .ToListAsync();
        }

        public async Task<List<StudentFeeAllocation>> GetStudentFeeAllocationsByStudentId(int studentId)
        {
            return await _feeAllocationRepository.Table
                .Where(c => c.StudentId == studentId)
                .Include(s => s.ClassFeeList)
                    .ThenInclude(m => m.AcademicSession)
                .Include(c => c.Student)
                    .ThenInclude(l => l.AcademicClass)
                .Include(c => c.StudentFeeHead)
                .ToListAsync();
        }
    }
}
