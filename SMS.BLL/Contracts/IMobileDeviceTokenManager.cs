using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IMobileDeviceTokenManager : IManager<MobileDeviceToken>
    {
        Task<bool> RegisterAsync(string userId, string token, string platform);
        Task<bool> UnregisterAsync(string token, string userId = null);
        Task<IReadOnlyCollection<MobileDeviceToken>> GetActiveByUserIdAsync(string userId);
    }
}
