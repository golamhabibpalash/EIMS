using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Contracts.Base;
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
    public class AcademicSectionRepository : Repository<AcademicSection>, IAcademicSectionRepositoy 
    {
        
        public AcademicSectionRepository(ApplicationDbContext db):base(db)
        {

        }
        public override async Task<IReadOnlyCollection<AcademicSection>> GetAllAsync()
        {
            return await _context.AcademicSection
                .Include(m => m.AcademicClass)
                .Include(m => m.AcademicSession)
                .ToListAsync();
        }
        public async override Task<AcademicSection> GetByIdAsync(int id)
        {
            return await _context.AcademicSection
                .Include(a => a.AcademicClass)
                .Include(a => a.AcademicSession)
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<AcademicSection>> GetAllByClassWithSessionId(int classId, int sessionId)
        {
            return await _context.AcademicSection
                .Where(s => s.AcademicClassId == classId && s.AcademicSessionId == sessionId)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<AcademicSection>> GetAllByExamGroupId(int examGroupId, int classId, int sessionId)
        {
            var sectionIds = await _context.AcademicExams
                .Where(ae => ae.AcademicExamGroupId == examGroupId && ae.AcademicClassId == classId && ae.AcademicSectionId != null)
                .Select(ae => ae.AcademicSectionId)
                .Distinct()
                .ToListAsync();

            return await _context.AcademicSection
                .Where(s => s.AcademicClassId == classId && s.AcademicSessionId == sessionId && sectionIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<AcademicSection> GetByNameAsync(string name)
        {
            return await _context.AcademicSection.Where(a => a.Name.Trim() == name.Trim()).FirstOrDefaultAsync();
        }

        public async Task<bool> IsExistByNameWithClassNSessionAsync(AcademicSection academicSection)
        {
            var existAcademicSection = await _context.AcademicSection.Where(s => s.AcademicClassId == academicSection.AcademicClassId && s.AcademicSessionId == academicSection.AcademicSessionId && s.Name.Trim() == academicSection.Name.Trim()).FirstOrDefaultAsync();

            if (existAcademicSection != null)
            {
                return true;
            }
            return false;
        }

    }
}
