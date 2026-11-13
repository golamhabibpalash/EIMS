using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAdmissionGuardianAccountRepository : IAdmissionRepository<AdmissionGuardianAccount>
    {
        Task<AdmissionGuardianAccount> GetByMobileAsync(string mobileNumber, bool tracked = false);
    }
}
