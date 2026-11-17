using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IMobileNoticeManager : IManager<MobileNotice>
    {
        Task<IReadOnlyCollection<MobileNotice>> GetPublishedAsync(int? take = null);
    }
}
