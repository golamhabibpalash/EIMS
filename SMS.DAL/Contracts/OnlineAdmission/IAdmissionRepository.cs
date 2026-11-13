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
    /// <summary>
    /// Base for the Online Admission repositories. Adds explicit unit-of-work control: workflow
    /// steps load tracked rows, change several of them, then commit once (optionally inside a
    /// transaction). Unlike the base Add/Update, <see cref="CommitAsync"/> lets errors surface.
    /// </summary>
    public interface IAdmissionRepository<T> : IRepository<T> where T : CommonProps
    {
        Task<T> GetTrackedAsync(int id);
        Task CommitAsync();

        /// <summary>
        /// Runs <paramref name="work"/> in a database transaction under EF's retry strategy (retry-on-failure
        /// is enabled, which rejects bare user transactions). The change tracker is cleared before each
        /// attempt, so <paramref name="work"/> must load what it changes. Commits only when the result
        /// succeeded; a failed result rolls everything back. A write conflict that survives the retries
        /// comes back as a failed result rather than an exception.
        /// </summary>
        Task<AdmissionResult> ExecuteInTransactionAsync(Func<Task<AdmissionResult>> work, IsolationLevel isolationLevel = IsolationLevel.Serializable);
    }
}
