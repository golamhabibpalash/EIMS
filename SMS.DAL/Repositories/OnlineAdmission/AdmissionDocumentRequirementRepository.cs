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
    public class AdmissionDocumentRequirementRepository : AdmissionRepository<AdmissionDocumentRequirement>, IAdmissionDocumentRequirementRepository
    {
        public AdmissionDocumentRequirementRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<AdmissionDocumentRequirement>> GetBySessionAsync(int admissionSessionId)
        {
            return _context.AdmissionDocumentRequirements
                .Where(d => d.AdmissionSessionId == admissionSessionId)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Id)
                .ToListAsync();
        }
    }
}
