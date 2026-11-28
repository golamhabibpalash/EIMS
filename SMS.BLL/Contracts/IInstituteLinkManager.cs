using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IInstituteLinkManager : IManager<InstituteLink>
    {
        /// <summary>All links for an institute, ordered for display.</summary>
        Task<List<InstituteLink>> GetByInstituteIdAsync(int instituteId);

        /// <summary>Only the active links for an institute, ordered for display.</summary>
        Task<List<InstituteLink>> GetActiveByInstituteIdAsync(int instituteId);
    }
}
