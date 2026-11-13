using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IStudentActivateHistManager : IManager<StudentActivateHist>
    {
        Task<bool> IsStudentActive(int id, string date);
        Task<List<StudentActivateHist>> GetActivityListByUniqueId(string uniqueId);
    }
}
