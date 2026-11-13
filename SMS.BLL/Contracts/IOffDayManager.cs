using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IOffDayManager:IManager<OffDay>
    {
        Task<List<DateTime>> GetMonthlyHolidaysAsync(string monthYear);
        Task<List<OffDay>> GetYearlyHolidaysAsync(int Year);
    }
}
