using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IStudentFeeAllocationManager : IManager<StudentFeeAllocation>
    {
        Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdFeeHeadId(string uniqueId, int feeHeadId);
        Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdClassFeeId(string uniqueId, int classfeeId);
        Task<(int totalRecord, int filteredRecord, List<StudentFeeAllocation> data)> GetDataTableDataAsync(
            string searchValue, string orderColumn, string orderDirection, int start, int length);
        Task<List<StudentFeeAllocation>> GetStudentFeeAllocationsByStudentId(int studentId);
        Task<(bool canDelete, string message)> CanDeleteAsync(int allocationId);
        Task<(bool deleted, string message)> DeleteWithValidationAsync(int allocationId);
    }
}
