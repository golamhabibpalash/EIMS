using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IClassFeeListRepository : IRepository<ClassFeeList>
    {
        Task<ClassFeeList> GetByClassIdAndFeeHeadIdAsync(int classId, int feeHeadId, int sessionId);
        Task<List<ClassFeeList>> GetAllByClassIdAsync(int classId);
        Task<List<ClassFeeList>> GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(int classId, int feeHeadId, int sessionId);
        Task<List<ClassFeeList>> GetByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId);
        Task<List<ClassFeeList>> GetAllBySessionIdClassIdAsync(int sessionId, int classId);
        Task<List<ClassFeeList>> GetAllBySessionIdClassIdAsync(int sessionId, int classId, bool isResidential);
        Task<double> GetFeeAmountByFeeListSL(string uniquId, int sl);
        Task<List<ClassFeeList>> GetCurrentAllByUniqueId(string uniqueId);
    }
}
