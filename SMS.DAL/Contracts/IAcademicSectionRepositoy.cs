using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAcademicSectionRepositoy : IRepository<AcademicSection>
    {
        Task<IReadOnlyCollection<AcademicSection>> GetAllByClassWithSessionId(int classId, int sessionId);
        Task<IReadOnlyCollection<AcademicSection>> GetAllByExamGroupId(int examGroupId, int classId, int sessionId);
        Task<AcademicSection> GetByNameAsync(string name);
        Task<bool> IsExistByNameWithClassNSessionAsync(AcademicSection academicSection);
        
    }
}
