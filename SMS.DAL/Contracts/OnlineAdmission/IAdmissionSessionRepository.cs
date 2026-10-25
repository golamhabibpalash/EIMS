using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAdmissionSessionRepository : IAdmissionRepository<AdmissionSession>
    {
        /// <summary>Session with its academic session, offered classes (class + fee head) and document list.</summary>
        Task<AdmissionSession> GetWithDetailsAsync(int id);
        Task<List<AdmissionSession>> GetAllWithProgramsAsync(bool includeArchived);
        /// <summary>Sessions visible to the public right now (published, past their publish-from date).</summary>
        Task<List<AdmissionSession>> GetPublishedAsync(DateTime now);
    }
}
