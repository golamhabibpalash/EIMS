using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class AdmissionSettingRepository : AdmissionRepository<AdmissionSetting>, IAdmissionSettingRepository
    {
        public AdmissionSettingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionSetting> GetCurrentAsync()
        {
            return _context.AdmissionSettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
        }
    }
}
