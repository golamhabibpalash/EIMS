using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface ITicketCommentRepository : IRepository<TicketComment>
    {
        /// <summary>A ticket's discussion in the order it happened.</summary>
        Task<IReadOnlyCollection<TicketComment>> GetByTicketIdAsync(int ticketId);
    }
}
