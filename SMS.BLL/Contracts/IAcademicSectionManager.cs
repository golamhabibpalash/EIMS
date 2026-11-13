using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IAcademicSectionManager:IManager<AcademicSection>
    {
        Task<IReadOnlyCollection<AcademicSection>> GetAllByClassWithSessionId(int classId, int sessionId);
        Task<IReadOnlyCollection<AcademicSection>> GetAllByExamGroupIdClassIdSessionId(int examGroupId, int classId, int sessionId);
    }
}
