using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class ClassFeeListRepository : Repository<ClassFeeList>, IClassFeeListRepository
    {
        public ClassFeeListRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<IReadOnlyCollection<ClassFeeList>> GetAllAsync()
        {
            var feeLists = await _context
                .ClassFeeList
                .Include(cf => cf.AcademicClass)
                .Include(cf => cf.AcademicSession)
                .Include(cf => cf.StudentFeeHead)
                .ToListAsync();
            return feeLists;
        }
        public override async Task<ClassFeeList> GetByIdAsync(int id)
        {
            return await _context.ClassFeeList.Include(s => s.AcademicClass).Include(c => c.StudentFeeHead).FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<List<ClassFeeList>> GetAllByClassIdAsync(int classId)
        {
            var result = await _context.ClassFeeList.Include(c => c.StudentFeeHead).Where(c => c.AcademicClassId == classId).ToListAsync();
            return result;
        }
        public async Task<ClassFeeList> GetByClassIdAndFeeHeadIdAsync(int classId, int feeHeadId, int sessionId)
        {
            var feeListExist = await _context.ClassFeeList
                .FirstOrDefaultAsync(s => s.AcademicClassId == classId && s.StudentFeeHeadId == feeHeadId && s.AcademicSessionId == sessionId);

            return feeListExist;
        }
        public async Task<List<ClassFeeList>> GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(int classId, int feeHeadId, int sessionId)
        {
            return await _context.ClassFeeList
            .Where(s => s.AcademicClassId == classId &&
            s.AcademicSessionId == sessionId &&
            s.StudentFeeHeadId == feeHeadId)
            .ToListAsync();
        }
        public async Task<double> GetFeeAmountByFeeListSL(string uniquId, int sl)
        {
            var classFeeList = await _context.ClassFeeList
                .Include(c => c.StudentFeeHead)
                .Where(c => c.SL == sl)
                .FirstOrDefaultAsync();

            if (classFeeList != null)
            {
                return classFeeList.Amount;
            }

            return 0;
        }

        public async Task<List<ClassFeeList>> GetByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId)
        {
            List<ClassFeeList> results = new();
            try
            {
                var student = await _context.Student.FindAsync(studentId);
                if (student == null) return results;

                results = await (from t in _context.ClassFeeList
                              where t.AcademicSessionId == sessionId && t.AcademicClassId == classId
                              join h in _context.StudentFeeHead on t.StudentFeeHeadId equals h.Id
                              where h.IsResidential == student.IsResidential
                              select t)
                              .Include(s => s.StudentFeeHead)
                              .Include(s => s.AcademicSession)
                              .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return results;
        }
        public async Task<List<ClassFeeList>> GetAllBySessionIdClassIdAsync(int sessionId, int classId)
        {
            List<ClassFeeList> results = new();
            try
            {
                results = await _context.ClassFeeList.Where(s => s.AcademicSessionId == sessionId && s.AcademicClassId == classId).Include(c => c.StudentFeeHead).Include(c => c.AcademicSession).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return results;
        }
        public async Task<List<ClassFeeList>> GetAllBySessionIdClassIdAsync(int sessionId, int classId, bool isResidential)
        {
            List<ClassFeeList> results = new();
            try
            {
                results = await _context.ClassFeeList
                    .Where(s => s.AcademicSessionId == sessionId && s.AcademicClassId == classId && s.StudentFeeHead.IsResidential == isResidential)
                    .Include(c => c.StudentFeeHead)
                    .Include(c => c.AcademicSession)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return results;
        }
        public async Task<List<ClassFeeList>> GetCurrentAllByUniqueId(string uniqueId)
        {
            var student = await _context.Student.FirstOrDefaultAsync(s => s.UniqueId == uniqueId);
            var currentSession = await _context.AcademicSession.FirstOrDefaultAsync(s => s.CurrentSession == true);
            return await _context
                .ClassFeeList.Include(m => m.StudentFeeHead).
                Where(f => f.AcademicClassId == student.AcademicClassId && f.StudentFeeHead.IsResidential == student.IsResidential && f.AcademicSessionId == currentSession.Id)
                .ToListAsync();
        }
    }
}
