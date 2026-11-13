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
    public class AcademicExamRepository : Repository<AcademicExam>, IAcademicExamRepository
    {
        private new readonly ApplicationDbContext _context;
        public AcademicExamRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override Task<bool> AddAsync(AcademicExam entity)
        {
            return base.AddAsync(entity);
        }

        public override async Task<IReadOnlyCollection<AcademicExam>> GetAllAsync()
        {
            var result = await _context.AcademicExams
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicExamGroup)
                .Include(s => s.AcademicSubject)
                .Include(s => s.AcademicSection)
                .Include(s => s.Employee)
                .Include(s => s.AcademicExamDetails)
                    .ThenInclude(d => d.Student)
                .ToListAsync();
            return result;
        }

        public override async Task<AcademicExam> GetByIdAsync(int id)
        {
            var result = await _context.AcademicExams
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicExamGroup)
                    .ThenInclude(s => s.AcademicSession)
                .Include(s => s.AcademicSubject)
                .Include(s => s.Employee)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicExamDetails.Where(s => s.Student.Status==true).OrderBy(s => s.Student.ClassRoll))
                    .ThenInclude(s => s.Student)
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            return result;
        }
        public async Task<List<AcademicExam>> GetByClassIdExamGroupId(int examGroupId, int academicClassId)
        {
            var allExams = await _context.AcademicExams
                .Where(s => s.AcademicClassId == academicClassId && s.AcademicExamGroupId == examGroupId)
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicExamGroup)
                .Include(s => s.AcademicSubject)
                .Include(s => s.AcademicExamDetails)
                    .ThenInclude(m => m.Student)
                .ToListAsync();

            var exams = allExams
                .GroupBy(e => new { e.AcademicSubjectId, e.ExamCategory })
                .Select(g => g.First())
                .ToList();

            return exams;
        }
        public async Task<List<AcademicExam>> GetAllByClassIdExamGroupId(int examGroupId, int academicClassId)
        {
            var exams = await _context.AcademicExams
                .Where(s => s.AcademicClassId == academicClassId && s.AcademicExamGroupId == examGroupId)
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicExamGroup)
                .Include(s => s.AcademicSubject)
                .Include(s => s.AcademicExamDetails)
                    .ThenInclude(m => m.Student)
                .ToListAsync();

            return exams;
        }
        public async Task<List<AcademicExam>> GetByClassIdExamGroupIdSectionId(int examGroupId, int academicClassId, int sectionId)
        {
            var allExams = await _context.AcademicExams
                .Where(s => s.AcademicClassId == academicClassId && s.AcademicExamGroupId == examGroupId && s.AcademicSectionId == sectionId)
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicExamGroup)
                .Include(s => s.AcademicSubject)
                .Include(s => s.AcademicExamDetails)
                    .ThenInclude(m => m.Student)
                .ToListAsync();

            var uniqueSubjectIds = allExams
                .Select(e => e.AcademicSubjectId)
                .Distinct()
                .ToHashSet();

            var exams = allExams
                .Where(e => uniqueSubjectIds.Contains(e.AcademicSubjectId))
                .GroupBy(e => e.AcademicSubjectId)
                .Select(g => g.First())
                .ToList();

            return exams;
        }

        public async Task<bool> IsDuplicateAsync(int examGroupId, int classId, int subjectId, int? sectionId, string examCategory)
        {
            var query = _context.AcademicExams
                .Where(e => e.AcademicExamGroupId == examGroupId 
                         && e.AcademicClassId == classId 
                         && e.AcademicSubjectId == subjectId 
                         && e.ExamCategory == examCategory);

            if (sectionId.HasValue)
            {
                query = query.Where(e => e.AcademicSectionId == sectionId.Value);
            }
            else
            {
                query = query.Where(e => e.AcademicSectionId == null);
            }

            return await query.AnyAsync();
        }

        public async Task<List<(int SectionId, bool IsDuplicate)>> CheckDuplicatesBulkAsync(int examGroupId, int classId, int subjectId, string examCategory, List<int> sectionIds)
        {
            var results = new List<(int SectionId, bool IsDuplicate)>();
            var existingExams = await _context.AcademicExams
                .Where(e => e.AcademicExamGroupId == examGroupId
                         && e.AcademicClassId == classId
                         && e.AcademicSubjectId == subjectId
                         && e.ExamCategory == examCategory)
                .Select(e => e.AcademicSectionId)
                .ToListAsync();

            foreach (var sectionId in sectionIds)
            {
                bool isDuplicate = existingExams.Contains(sectionId);
                results.Add((sectionId, isDuplicate));
            }

            return results;
        }

        public async Task<List<AcademicExam>> GetAllLiteAsync()
        {
            return await _context.AcademicExams
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicExamGroup)
                    .ThenInclude(g => g.AcademicSession)
                .Include(s => s.AcademicSubject)
                .Include(s => s.AcademicSection)
                .Include(s => s.Employee)
                .Include(s => s.AcademicExamDetails)
                .ToListAsync();
        }

        public async Task<bool> RemoveByIdAsync(int id)
        {
            return await _context.AcademicExams
                .Where(e => e.Id == id)
                .ExecuteDeleteAsync() > 0;
        }
    }
}
