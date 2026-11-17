using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class MobileNoticeRepository : Repository<MobileNotice>, IMobileNoticeRepository
    {
        public MobileNoticeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<MobileNotice>> GetPublishedAsync(int? take = null)
        {
            var now = DateTime.Now;
            IQueryable<MobileNotice> query = _context.MobileNotices
                .AsNoTracking()
                .Where(n => n.IsPublished
                            && (n.PublishedFrom == null || n.PublishedFrom <= now)
                            && (n.PublishedTo == null || n.PublishedTo >= now))
                .OrderBy(n => n.DisplayOrder)
                .ThenByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
}
