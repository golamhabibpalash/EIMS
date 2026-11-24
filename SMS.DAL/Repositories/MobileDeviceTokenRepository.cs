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
    public class MobileDeviceTokenRepository : Repository<MobileDeviceToken>, IMobileDeviceTokenRepository
    {
        public MobileDeviceTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MobileDeviceToken> GetByTokenAsync(string token)
        {
            return await _context.MobileDeviceTokens
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<List<MobileDeviceToken>> GetActiveByUserIdAsync(string userId)
        {
            return await _context.MobileDeviceTokens
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.IsActive)
                .OrderByDescending(t => t.LastSeenAt)
                .ToListAsync();
        }
    }
}
