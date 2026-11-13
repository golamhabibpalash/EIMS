using Microsoft.EntityFrameworkCore;
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
    public class AcademicExamGroupRepository:Repository<AcademicExamGroup>, IAcademicExamGroupRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public AcademicExamGroupRepository(ApplicationDbContext context):base(context)
        {
            _dbContext = context;
        }
        public override async Task<IReadOnlyCollection<AcademicExamGroup>> GetAllAsync()
        {
            var result = await _dbContext.AcademicExamGroups.Include(s => s.AcademicSession).Include(s => s.AcademicExamType).Include(s => s.AcademicExams).ToListAsync();
            return result;
        }
        public async Task<IReadOnlyCollection<AcademicExamGroup>> GetByMonthExamType(int monthId, int examTypeId)
        {
            var result = await _dbContext.AcademicExamGroups.Where(s => s.ExamMonthId == monthId && s.AcademicExamTypeId == examTypeId).ToListAsync();
            return result;
        }

        public override async Task<AcademicExamGroup> GetByIdAsync(int id)
        {
            var result = await _dbContext
                .AcademicExamGroups
                .Include(s => s.AcademicExamType)
                .Include(s => s.AcademicExams)
                    .ThenInclude(m => m.AcademicClass)
                .Include(s => s.AcademicExams).ThenInclude(c =>c.AcademicSubject)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicExams)
                    .ThenInclude(s => s.Employee)
                .Include(s => s.AcademicExams)
                    .ThenInclude(m => m.AcademicExamDetails)
                    .ThenInclude(d => d.Student)
                .FirstOrDefaultAsync(s => s.Id == id);
            return result;
        }
    }
}
