using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IClassFeeListManager : IManager<ClassFeeList>
    {
        Task<ClassFeeList> GetByClassIdAndFeeHeadIdAsync(int classId, int feeHeadId, int sessionId);
        Task<List<ClassFeeList>> GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(int classId, int feeHeadId, int sessionId);
        Task<List<ClassFeeList>> GetAllByClassIdAsync(int classId);
        Task<double> GetFeeAmountByFeeListSlAsync(string uniquId, int sl);
        Task<List<ClassFeeList>> GetByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId);
        Task<List<ClassFeeList>> GetClassFeeByUniquId(string uniquId);
        Task<List<ClassFeeList>>GetAllByStudentId(int stuId);
        Task<List<ClassFeeList>>GetAllBySessionClassTypeAsync(int sessionId, int classId, bool isResidential); //type = residential or Nonresidential >> bool
    }
}
