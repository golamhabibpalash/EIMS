using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IInstituteLinkRepository : IRepository<InstituteLink>
    {
        /// <summary>All links for an institute, ordered for display (DisplayOrder, then title).</summary>
        Task<List<InstituteLink>> GetByInstituteIdAsync(int instituteId);

        /// <summary>Only the active links for an institute, ordered for display. Used by the mobile API.</summary>
        Task<List<InstituteLink>> GetActiveByInstituteIdAsync(int instituteId);
    }
}
