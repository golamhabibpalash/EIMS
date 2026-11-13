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
    public class AdmissionProgramRepository : AdmissionRepository<AdmissionProgram>, IAdmissionProgramRepository
    {
        public AdmissionProgramRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionProgram> GetWithDetailsAsync(int id)
        {
            return _context.AdmissionPrograms
                .Include(p => p.AcademicClass)
                .Include(p => p.AdmissionFeeHead)
                .Include(p => p.AdmissionSession)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<List<AdmissionProgram>> GetBySessionAsync(int admissionSessionId)
        {
            return _context.AdmissionPrograms
                .Include(p => p.AcademicClass)
                .Where(p => p.AdmissionSessionId == admissionSessionId)
                .OrderBy(p => p.AcademicClass.ClassSerial)
                .ToListAsync();
        }
    }
}
