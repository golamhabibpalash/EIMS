using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class MobileSliderManager : Manager<MobileSlider>, IMobileSliderManager
    {
        private readonly IMobileSliderRepository _mobileSliderRepository;

        public MobileSliderManager(IMobileSliderRepository mobileSliderRepository)
            : base(mobileSliderRepository)
        {
            _mobileSliderRepository = mobileSliderRepository;
        }

        public async Task<IReadOnlyCollection<MobileSlider>> GetPublishedAsync(int? take = null)
        {
            return await _mobileSliderRepository.GetPublishedAsync(take);
        }
    }
}
