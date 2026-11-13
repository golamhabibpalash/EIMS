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
    public class ExamResultRepository:Repository<ExamResult>,IExamResultRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ExamResultRepository(ApplicationDbContext context):base(context)
        {
            _dbContext = context;
        }

        public async Task<List<ExamResult>> GetExamResultsByExamGroupNClassId(int examGroupId, int classId)
        {
            List<ExamResult> examResults = new List<ExamResult>();
            try
            {
                examResults = await _dbContext
                    .ExamResults
                    .Include(s => s.ExamResultDetails)
                        .ThenInclude(m => m.AcademicSubject)
                    .Include(s => s.Student)
                        .ThenInclude(s => s.AcademicClass)
                     .Include(s => s.Student.Gender)
                     .Include(s => s.Student.AcademicSection)
                    .Where(s => s.AcademicExamGroupId == examGroupId && s.AcademicClassId == classId)
                    .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return examResults;
        }

        public bool IsResultProcessedAsync(int examGroupId, int classId)
        {
            bool isExist = _dbContext.ExamResults.Any(s => s.AcademicExamGroupId == examGroupId && s.AcademicClassId == classId);
            return isExist;
        }

        public async Task<bool> DeleteByGroupAndClassAsync(int groupId, int classId)
        {
            var results = await _dbContext.ExamResults
                .Include(r => r.ExamResultDetails)
                .Where(r => r.AcademicExamGroupId == groupId && r.AcademicClassId == classId)
                .ToListAsync();

            if (!results.Any()) return false;

            var details = results.SelectMany(r => r.ExamResultDetails).ToList();
            if (details.Any())
            {
                _dbContext.ExamResultDetails.RemoveRange(details);
            }
            _dbContext.ExamResults.RemoveRange(results);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<Dictionary<(int ExamGroupId, int ClassId), bool>> GetResultProcessedStatusBulkAsync(IEnumerable<(int ExamGroupId, int ClassId)> examGroupAndClassPairs)
        {
            var pairs = examGroupAndClassPairs.ToList();
            if (!pairs.Any()) return new Dictionary<(int, int), bool>();

            var existingPairs = await _dbContext.ExamResults
                .Select(s => new { s.AcademicExamGroupId, s.AcademicClassId })
                .ToListAsync();

            var existingSet = existingPairs
                .Select(s => (s.AcademicExamGroupId, s.AcademicClassId))
                .ToHashSet();

            return pairs.ToDictionary(p => p, p => existingSet.Contains(p));
        }

    }
}
