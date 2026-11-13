using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        /// <summary>Tickets with their comments and attachments loaded, newest first.</summary>
        Task<IReadOnlyCollection<Ticket>> GetAllWithDetailsAsync();

        /// <summary>One ticket with its discussion and attachments, or null.</summary>
        Task<Ticket> GetByIdWithDetailsAsync(int id);

        /// <summary>Tickets raised by one user, newest first.</summary>
        Task<IReadOnlyCollection<Ticket>> GetByRaisedByAsync(string userId);

        /// <summary>Count per status, for the list screen's summary tiles.</summary>
        Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync();
    }
}
