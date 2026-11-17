using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IMobileSliderManager : IManager<MobileSlider>
    {
        Task<IReadOnlyCollection<MobileSlider>> GetPublishedAsync(int? take = null);
    }
}
