using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class InstituteLinkManager : Manager<InstituteLink>, IInstituteLinkManager
    {
        private readonly IInstituteLinkRepository _instituteLinkRepository;

        public InstituteLinkManager(IInstituteLinkRepository instituteLinkRepository) : base(instituteLinkRepository)
        {
            _instituteLinkRepository = instituteLinkRepository;
        }

        public async Task<List<InstituteLink>> GetByInstituteIdAsync(int instituteId)
        {
            return await _instituteLinkRepository.GetByInstituteIdAsync(instituteId);
        }

        public async Task<List<InstituteLink>> GetActiveByInstituteIdAsync(int instituteId)
        {
            return await _instituteLinkRepository.GetActiveByInstituteIdAsync(instituteId);
        }
    }
}
