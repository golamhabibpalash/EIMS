using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class AdmissionApplicationHistoryRepository : AdmissionRepository<AdmissionApplicationHistory>, IAdmissionApplicationHistoryRepository
    {
        public AdmissionApplicationHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<AdmissionApplicationHistory>> GetByApplicationAsync(int admissionApplicationId, bool includeInternal)
        {
            var query = _context.AdmissionApplicationHistories.Where(h => h.AdmissionApplicationId == admissionApplicationId);
            if (!includeInternal)
            {
                query = query.Where(h => !h.IsInternal);
            }
            return query.OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).ToListAsync();
        }
    }
}
