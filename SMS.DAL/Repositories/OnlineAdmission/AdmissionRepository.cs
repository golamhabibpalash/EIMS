using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public abstract class AdmissionRepository<T> : Base.Repository<T>, IAdmissionRepository<T> where T : CommonProps
    {
        protected AdmissionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public virtual Task<T> GetTrackedAsync(int id)
        {
            return Entity.AsTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public Task CommitAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<AdmissionResult> ExecuteInTransactionAsync(Func<Task<AdmissionResult>> work, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    // A retried attempt must not see entities left over from the failed one.
                    _context.ChangeTracker.Clear();
                    await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);

                    var result = await work();
                    if (result != null && result.Succeeded)
                    {
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _context.ChangeTracker.Clear();
                    }
                    return result;
                });
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return AdmissionResult.Fail("This record was changed by someone else at the same time. Please reload and try again.");
            }
            catch (InvalidOperationException ex) when (ex.InnerException is DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                return AdmissionResult.Fail("This record was changed by someone else at the same time. Please reload and try again.");
            }
        }
    }
}
