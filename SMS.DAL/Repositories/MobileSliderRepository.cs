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
    public class MobileSliderRepository : Repository<MobileSlider>, IMobileSliderRepository
    {
        public MobileSliderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<MobileSlider>> GetPublishedAsync(int? take = null)
        {
            var now = DateTime.Now;
            IQueryable<MobileSlider> query = _context.MobileSliders
                .AsNoTracking()
                .Where(s => s.IsPublished
                            && (s.PublishedFrom == null || s.PublishedFrom <= now)
                            && (s.PublishedTo == null || s.PublishedTo >= now))
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ThenByDescending(s => s.Id);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
}
