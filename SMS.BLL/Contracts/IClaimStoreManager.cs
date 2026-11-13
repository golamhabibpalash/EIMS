using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IClaimStoreManager:IManager<ClaimStores>
    {
        Task<bool> IsExistAsync(string claimValue, int subModuleId);
        Task<List<string>> GetUserClaimsAsync(ClaimsPrincipal user);
    }
}
