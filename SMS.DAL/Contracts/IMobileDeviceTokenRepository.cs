using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IMobileDeviceTokenRepository : IRepository<MobileDeviceToken>
    {
        Task<MobileDeviceToken> GetByTokenAsync(string token);
        Task<List<MobileDeviceToken>> GetActiveByUserIdAsync(string userId);
    }
}
