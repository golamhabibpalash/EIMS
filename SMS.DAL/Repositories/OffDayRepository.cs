using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class OffDayRepository:Repository<OffDay>,IOffDayRepository
    {
        private new readonly ApplicationDbContext _context;
        public OffDayRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }

        public async Task<List<DateTime>> GetMonthlyHolidaysAsync(string monthYear)
        {
            int a = Convert.ToInt32(monthYear.Substring(monthYear.Length - 4, 4));
            int b = Convert.ToInt32(monthYear[..2]);
            DateTime dateTime = new(a, b,1);
            List<DateTime> monthlyHolidays = new();
            List<OffDay> holidays = new();
            holidays= await _context.OffDays.Where(m =>m.OffDayStartingDate.Month==dateTime.Month && m.OffDayStartingDate.Year == dateTime.Year).ToListAsync();
            if (holidays!=null)
            {
                foreach (var holiday in holidays)
                {
                    DateTime sd = holiday.OffDayStartingDate;
                    DateTime ed = holiday.OffDayEndDate;

                    DateTime nextDate = sd;
                    for (int i = 1; i <= holiday.TotalDays; i++)
                    {
                        bool isExist = (from m in monthlyHolidays
                                       where m.Date.ToString("ddMMyyyy") == sd.ToString("ddMMyyyy")
                                       select m).Any();
                        if (isExist)
                        {
                            sd = sd.AddDays(1);
                            continue;
                        }
                        else
                        {
                            monthlyHolidays.Add(sd);
                        }
                        sd = sd.AddDays(1);                        
                    }
                }
            }
            return monthlyHolidays;
        }
    }
}
