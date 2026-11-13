using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMS.BLL.Contracts.Base;
using SMS.Entities;

namespace SMS.BLL.Contracts
{
    public interface IStudentFeeHeadManager : IManager<StudentFeeHead>
    {
        Task<StudentFeeHead> GetByNameAsync(string name);
        Task<List<StudentFeeHead>> GetAllByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId);
    }
}
