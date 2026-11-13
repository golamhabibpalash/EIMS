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
    public class AdmissionGuardianAccountRepository : AdmissionRepository<AdmissionGuardianAccount>, IAdmissionGuardianAccountRepository
    {
        public AdmissionGuardianAccountRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionGuardianAccount> GetByMobileAsync(string mobileNumber, bool tracked = false)
        {
            var query = tracked ? _context.AdmissionGuardianAccounts.AsTracking() : _context.AdmissionGuardianAccounts;
            return query.FirstOrDefaultAsync(a => a.MobileNumber == mobileNumber);
        }
    }
}
