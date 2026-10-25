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
    public class AdmissionSessionRepository : AdmissionRepository<AdmissionSession>, IAdmissionSessionRepository
    {
        public AdmissionSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionSession> GetWithDetailsAsync(int id)
        {
            return _context.AdmissionSessions
                .Include(s => s.AcademicSession)
                .Include(s => s.Programs).ThenInclude(p => p.AcademicClass)
                .Include(s => s.Programs).ThenInclude(p => p.AdmissionFeeHead)
                .Include(s => s.DocumentRequirements)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<List<AdmissionSession>> GetAllWithProgramsAsync(bool includeArchived)
        {
            var query = _context.AdmissionSessions
                .Include(s => s.AcademicSession)
                .Include(s => s.Programs).ThenInclude(p => p.AcademicClass)
                .AsSplitQuery();

            if (!includeArchived)
            {
                query = query.Where(s => s.Status != AdmissionSessionStatus.Archived);
            }

            return query.OrderByDescending(s => s.ApplicationStartDate).ToListAsync();
        }

        public Task<List<AdmissionSession>> GetPublishedAsync(DateTime now)
        {
            return _context.AdmissionSessions
                .Include(s => s.AcademicSession)
                .Include(s => s.Programs.Where(p => p.IsActive)).ThenInclude(p => p.AcademicClass)
                .Where(s => s.Status == AdmissionSessionStatus.Published
                         && (s.PublishFrom == null || s.PublishFrom <= now))
                .OrderByDescending(s => s.ApplicationStartDate)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}
