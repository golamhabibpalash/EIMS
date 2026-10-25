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
    public class AdmissionApplicationDocumentRepository : AdmissionRepository<AdmissionApplicationDocument>, IAdmissionApplicationDocumentRepository
    {
        public AdmissionApplicationDocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<AdmissionApplicationDocument>> GetByApplicationAsync(int admissionApplicationId)
        {
            return _context.AdmissionApplicationDocuments
                .Include(d => d.AdmissionDocumentRequirement)
                .Where(d => d.AdmissionApplicationId == admissionApplicationId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }
    }
}
