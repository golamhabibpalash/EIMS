using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IMobileNoticeRepository : IRepository<MobileNotice>
    {
        Task<IReadOnlyCollection<MobileNotice>> GetPublishedAsync(int? take = null);
    }
}
