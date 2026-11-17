using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class MobileNoticeManager : Manager<MobileNotice>, IMobileNoticeManager
    {
        private readonly IMobileNoticeRepository _mobileNoticeRepository;

        public MobileNoticeManager(IMobileNoticeRepository mobileNoticeRepository)
            : base(mobileNoticeRepository)
        {
            _mobileNoticeRepository = mobileNoticeRepository;
        }

        public async Task<IReadOnlyCollection<MobileNotice>> GetPublishedAsync(int? take = null)
        {
            return await _mobileNoticeRepository.GetPublishedAsync(take);
        }
    }
}
