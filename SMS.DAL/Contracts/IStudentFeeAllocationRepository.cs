using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IStudentFeeAllocationRepository : IRepository<StudentFeeAllocation>
    {
        Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdFeeHeadId(string uniqueId, int feeHeadId);
        Task<List<StudentFeeAllocation>> GetStudentFeeAllocationByUniqueIdSessionId(string uniqueId, int sessionId);
        Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdClassFeeId(string uniqueId, int classfeeId);
        Task<(int totalRecord, int filteredRecord, List<StudentFeeAllocation> data)> GetDataTableDataAsync(
            string searchValue, string orderColumn, string orderDirection, int start, int length);
    }
}
