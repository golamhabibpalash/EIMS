using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface ITicketAttachmentRepository : IRepository<TicketAttachment>
    {
        Task<IReadOnlyCollection<TicketAttachment>> GetByTicketIdAsync(int ticketId);
    }
}
