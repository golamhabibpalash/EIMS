using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class MobileDeviceTokenManager : Manager<MobileDeviceToken>, IMobileDeviceTokenManager
    {
        private readonly IMobileDeviceTokenRepository _deviceRepository;

        public MobileDeviceTokenManager(IMobileDeviceTokenRepository repository) : base(repository)
        {
            _deviceRepository = repository;
        }

        public async Task<bool> RegisterAsync(string userId, string token, string platform)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var now = DateTime.Now;
            var existing = await _deviceRepository.GetByTokenAsync(token);
            if (existing == null)
            {
                return await AddAsync(new MobileDeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = string.IsNullOrWhiteSpace(platform) ? "android" : platform,
                    LastSeenAt = now,
                    IsActive = true,
                    CreatedAt = now,
                    EditedAt = now
                });
            }

            existing.UserId = userId;
            existing.Platform = string.IsNullOrWhiteSpace(platform) ? existing.Platform : platform;
            existing.LastSeenAt = now;
            existing.IsActive = true;
            existing.EditedAt = now;

            return await UpdateAsync(existing);
        }

        public async Task<bool> UnregisterAsync(string token, string userId = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var existing = await _deviceRepository.GetByTokenAsync(token);
            if (existing == null || (userId != null && existing.UserId != userId))
            {
                return true;
            }

            existing.IsActive = false;
            existing.LastSeenAt = DateTime.Now;
            existing.EditedAt = DateTime.Now;
            await UpdateAsync(existing);
            return true;
        }

        public async Task<IReadOnlyCollection<MobileDeviceToken>> GetActiveByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Array.Empty<MobileDeviceToken>();
            }

            return await _deviceRepository.GetActiveByUserIdAsync(userId);
        }
    }
}
