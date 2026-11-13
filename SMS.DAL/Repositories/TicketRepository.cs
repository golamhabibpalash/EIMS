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
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Ticket>> GetAllWithDetailsAsync()
        {
            // Attachments and comment counts are shown on the list, so pull them in
            // one round trip rather than letting the view query per row.
            return await _context.Tickets
                .Include(t => t.TicketComments)
                .Include(t => t.TicketAttachments)
                .OrderByDescending(t => t.RaiseDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<Ticket> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.TicketComments)
                .Include(t => t.TicketAttachments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IReadOnlyCollection<Ticket>> GetByRaisedByAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.TicketComments)
                .Include(t => t.TicketAttachments)
                .Where(t => t.RaisedByUserId == userId)
                .OrderByDescending(t => t.RaiseDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }
    }
}
