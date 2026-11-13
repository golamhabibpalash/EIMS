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
    public class TicketAttachmentRepository : Repository<TicketAttachment>, ITicketAttachmentRepository
    {
        public TicketAttachmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<TicketAttachment>> GetByTicketIdAsync(int ticketId)
        {
            return await _context.TicketAttachments
                .Where(a => a.TicketId == ticketId)
                .OrderBy(a => a.Id)
                .ToListAsync();
        }
    }
}
