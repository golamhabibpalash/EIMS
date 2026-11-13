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
    public class TicketCommentRepository : Repository<TicketComment>, ITicketCommentRepository
    {
        public TicketCommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<TicketComment>> GetByTicketIdAsync(int ticketId)
        {
            return await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }
    }
}
