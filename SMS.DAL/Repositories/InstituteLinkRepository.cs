using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class InstituteLinkRepository : Repository<InstituteLink>, IInstituteLinkRepository
    {
        public InstituteLinkRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<InstituteLink>> GetByInstituteIdAsync(int instituteId)
        {
            return await _context.InstituteLinks
                .AsNoTracking()
                .Where(l => l.InstituteId == instituteId)
                .OrderBy(l => l.DisplayOrder)
                .ThenBy(l => l.LinkTitle)
                .ToListAsync();
        }

        public async Task<List<InstituteLink>> GetActiveByInstituteIdAsync(int instituteId)
        {
            return await _context.InstituteLinks
                .AsNoTracking()
                .Where(l => l.InstituteId == instituteId && l.IsActive)
                .OrderBy(l => l.DisplayOrder)
                .ThenBy(l => l.LinkTitle)
                .ToListAsync();
        }
    }
}
