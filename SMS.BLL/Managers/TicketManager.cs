using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Ticket workflow. Status changes and assignments are written into the
    /// discussion as system notes, so a ticket's history is readable in one place
    /// instead of needing a separate audit screen.
    /// </summary>
    public class TicketManager : Manager<Ticket>, ITicketManager
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketCommentRepository _ticketCommentRepository;
        private readonly ITicketAttachmentRepository _ticketAttachmentRepository;

        public TicketManager(
            ITicketRepository ticketRepository,
            ITicketCommentRepository ticketCommentRepository,
            ITicketAttachmentRepository ticketAttachmentRepository) : base(ticketRepository)
        {
            _ticketRepository = ticketRepository;
            _ticketCommentRepository = ticketCommentRepository;
            _ticketAttachmentRepository = ticketAttachmentRepository;
        }

        public async Task<IReadOnlyCollection<Ticket>> GetAllWithDetailsAsync()
        {
            return await _ticketRepository.GetAllWithDetailsAsync();
        }

        public async Task<Ticket> GetByIdWithDetailsAsync(int id)
        {
            return await _ticketRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<IReadOnlyCollection<Ticket>> GetByRaisedByAsync(string userId)
        {
            return await _ticketRepository.GetByRaisedByAsync(userId);
        }

        public async Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync()
        {
            return await _ticketRepository.GetStatusCountsAsync();
        }

        public async Task<bool> RaiseAsync(Ticket ticket, string userId, string macAddress)
        {
            if (ticket is null)
            {
                return false;
            }

            ticket.RaiseDate = ticket.RaiseDate == default ? DateTime.Now : ticket.RaiseDate;
            ticket.Status = TicketStatus.Pending;
            ticket.ClosedAt = null;
            ticket.RaisedByUserId = userId;
            ticket.CreatedBy = userId;
            ticket.CreatedAt = DateTime.Now;
            ticket.EditedBy = userId;
            ticket.EditedAt = DateTime.Now;
            ticket.MACAddress = macAddress;

            return await _ticketRepository.AddAsync(ticket);
        }

        public async Task<bool> ChangeStatusAsync(int ticketId, TicketStatus newStatus, string userId, string macAddress, string note = null)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket is null || ticket.Status == newStatus)
            {
                return false;
            }

            var previousStatus = ticket.Status;
            ticket.Status = newStatus;

            // Closed once means closed; reopening clears the stamp so the next close is accurate.
            bool isClosing = newStatus == TicketStatus.Solved || newStatus == TicketStatus.Rejected;
            ticket.ClosedAt = isClosing ? DateTime.Now : (DateTime?)null;

            ticket.EditedBy = userId;
            ticket.EditedAt = DateTime.Now;
            ticket.MACAddress = macAddress;

            bool isUpdated = await _ticketRepository.UpdateAsync(ticket);
            if (!isUpdated)
            {
                return false;
            }

            var message = $"Status changed from {previousStatus} to {newStatus}.";
            if (!string.IsNullOrWhiteSpace(note))
            {
                message += $" {note.Trim()}";
            }

            await AddSystemNoteAsync(ticketId, message, userId, macAddress);
            return true;
        }

        public async Task<bool> AssignAsync(int ticketId, string assignedToUserId, string userId, string macAddress)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket is null)
            {
                return false;
            }

            ticket.AssignedToUserId = assignedToUserId;
            ticket.EditedBy = userId;
            ticket.EditedAt = DateTime.Now;
            ticket.MACAddress = macAddress;

            bool isUpdated = await _ticketRepository.UpdateAsync(ticket);
            if (isUpdated)
            {
                await AddSystemNoteAsync(ticketId, "Ticket assignment updated.", userId, macAddress);
            }

            return isUpdated;
        }

        public async Task<IReadOnlyCollection<TicketComment>> GetCommentsAsync(int ticketId)
        {
            return await _ticketCommentRepository.GetByTicketIdAsync(ticketId);
        }

        public async Task<bool> AddCommentAsync(TicketComment comment, string userId, string macAddress)
        {
            if (comment is null || string.IsNullOrWhiteSpace(comment.Message))
            {
                return false;
            }

            comment.Message = comment.Message.Trim();
            comment.IsSystemNote = false;
            comment.CreatedBy = userId;
            comment.CreatedAt = DateTime.Now;
            comment.EditedBy = userId;
            comment.EditedAt = DateTime.Now;
            comment.MACAddress = macAddress;

            return await _ticketCommentRepository.AddAsync(comment);
        }

        public async Task<IReadOnlyCollection<TicketAttachment>> GetAttachmentsAsync(int ticketId)
        {
            return await _ticketAttachmentRepository.GetByTicketIdAsync(ticketId);
        }

        public async Task<TicketAttachment> GetAttachmentAsync(int attachmentId)
        {
            return await _ticketAttachmentRepository.GetByIdAsync(attachmentId);
        }

        public async Task<bool> AddAttachmentAsync(TicketAttachment attachment, string userId, string macAddress)
        {
            if (attachment is null)
            {
                return false;
            }

            attachment.CreatedBy = userId;
            attachment.CreatedAt = DateTime.Now;
            attachment.EditedBy = userId;
            attachment.EditedAt = DateTime.Now;
            attachment.MACAddress = macAddress;

            return await _ticketAttachmentRepository.AddAsync(attachment);
        }

        public async Task<bool> RemoveAttachmentAsync(TicketAttachment attachment)
        {
            if (attachment is null)
            {
                return false;
            }

            return await _ticketAttachmentRepository.RemoveAsync(attachment);
        }

        private async Task AddSystemNoteAsync(int ticketId, string message, string userId, string macAddress)
        {
            await _ticketCommentRepository.AddAsync(new TicketComment
            {
                TicketId = ticketId,
                Message = message,
                IsSystemNote = true,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                EditedBy = userId,
                EditedAt = DateTime.Now,
                MACAddress = macAddress
            });
        }
    }
}
