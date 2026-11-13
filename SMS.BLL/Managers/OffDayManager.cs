using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers;

public class OffDayManager:Manager<OffDay>,IOffDayManager
{
    private readonly IOffDayRepository _offDayRepository;
    public OffDayManager(IOffDayRepository offDayRepository):base(offDayRepository)
    {
        _offDayRepository = offDayRepository;
    }

    public async Task<List<DateTime>> GetMonthlyHolidaysAsync(string monthYear)
    {
        List<DateTime> dateTimes = new List<DateTime>();
        dateTimes = await _offDayRepository.GetMonthlyHolidaysAsync(monthYear);
        return dateTimes;
    }

    public async Task<List<OffDay>> GetYearlyHolidaysAsync(int Year)
    {
        List<OffDay> dateTimes = new List<OffDay>();
        dateTimes = await _offDayRepository.Table.AsNoTracking().Where(s => s.OffDayStartingDate.Year ==Year).ToListAsync();
        return dateTimes;
    }

}

