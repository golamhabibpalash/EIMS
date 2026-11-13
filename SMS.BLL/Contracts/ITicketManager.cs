using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface ITicketManager : IManager<Ticket>
    {
        Task<IReadOnlyCollection<Ticket>> GetAllWithDetailsAsync();

        Task<Ticket> GetByIdWithDetailsAsync(int id);

        Task<IReadOnlyCollection<Ticket>> GetByRaisedByAsync(string userId);

        Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync();

        /// <summary>
        /// Raises a ticket, stamping the raise date and opening status.
        /// </summary>
        Task<bool> RaiseAsync(Ticket ticket, string userId, string macAddress);

        /// <summary>
        /// Moves a ticket to a new status and records the change as a system note
        /// in the discussion. Returns false when the status is unchanged.
        /// </summary>
        Task<bool> ChangeStatusAsync(int ticketId, TicketStatus newStatus, string userId, string macAddress, string note = null);

        /// <summary>Assigns the ticket to a developer and notes it in the discussion.</summary>
        Task<bool> AssignAsync(int ticketId, string assignedToUserId, string userId, string macAddress);

        Task<IReadOnlyCollection<TicketComment>> GetCommentsAsync(int ticketId);

        Task<bool> AddCommentAsync(TicketComment comment, string userId, string macAddress);

        Task<IReadOnlyCollection<TicketAttachment>> GetAttachmentsAsync(int ticketId);

        Task<TicketAttachment> GetAttachmentAsync(int attachmentId);

        Task<bool> AddAttachmentAsync(TicketAttachment attachment, string userId, string macAddress);

        Task<bool> RemoveAttachmentAsync(TicketAttachment attachment);
    }
}
