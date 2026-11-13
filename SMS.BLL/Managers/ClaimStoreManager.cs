using BLL.Managers.Base;
using Microsoft.AspNetCore.Identity;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class ClaimStoreManager:Manager<ClaimStores>, IClaimStoreManager
    {
        private readonly IClaimStoreRepository _claimStoreRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public ClaimStoreManager(IClaimStoreRepository claimStoreRepository, UserManager<ApplicationUser> userManager) : base(claimStoreRepository)
        {
            _claimStoreRepository = claimStoreRepository;
            _userManager = userManager;
        }

        public async Task<bool> IsExistAsync(string claimValue, int subModuleId)
        {
            var cStores = await _claimStoreRepository.GetAllAsync();
            var cStore = cStores.FirstOrDefault(s => s.ClaimValue == claimValue && s.SubModuleId == subModuleId);

            return cStore != null?true:false;
        }
        public async Task<List<string>> GetUserClaimsAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null) return new List<string>();

            var claims = await _userManager.GetClaimsAsync(appUser);
            return claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
        }
    }
}
