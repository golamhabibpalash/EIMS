using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class StudentFeeAllocationRepository : Repository<StudentFeeAllocation>, IStudentFeeAllocationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public StudentFeeAllocationRepository(ApplicationDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public override async Task<IReadOnlyCollection<StudentFeeAllocation>> GetAllAsync()
        {
            var result = await _context.StudentFeeAllocations
                .Include(s => s.Student)
                .ThenInclude(s => s.AcademicClass)
                .Include(s => s.Student.AcademicSession)
                .Include(s => s.StudentFeeHead)
                .ToListAsync();
            return result;
        }

        public async Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdClassFeeId(string uniqueId, int classfeeId)
        {
            return await _context.StudentFeeAllocations.FirstOrDefaultAsync(f => f.UniqueId == uniqueId && f.ClassFeeListId == classfeeId);
        }

        public async Task<StudentFeeAllocation> GetStudentFeeAllocationByUniqueIdFeeHeadId(string uniqueId, int feeHeadId)
        {
            var result = await _context.StudentFeeAllocations.FirstOrDefaultAsync(s => s.UniqueId == uniqueId && s.StudentFeeHeadId == feeHeadId);
            return result;
        }

        public async Task<List<StudentFeeAllocation>> GetStudentFeeAllocationByUniqueIdSessionId(string uniqueId, int sessionId)
        {
            var results = await _context.StudentFeeAllocations.Include(s => s.StudentFeeHead).Where(s => s.UniqueId == uniqueId && s.ClassFeeList.AcademicSessionId == sessionId).ToListAsync();
            return results;
        }

        public async Task<(int totalRecord, int filteredRecord, List<StudentFeeAllocation> data)> GetDataTableDataAsync(
            string searchValue, string orderColumn, string orderDirection, int start, int length)
        {
            var query = _context.StudentFeeAllocations
                .Include(s => s.Student)
                .ThenInclude(s => s.AcademicClass)
                .Include(s => s.Student.AcademicSession)
                .Include(s => s.StudentFeeHead)
                .Include(s => s.ClassFeeList)
                .ThenInclude(c => c.AcademicSession)
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(s =>
                    (s.Student != null && s.Student.ClassRoll.ToString().ToLower().Contains(searchValue.ToLower())) ||
                    (s.Student != null && s.Student.Name.ToLower().Contains(searchValue.ToLower())) ||
                    (s.StudentFeeHead != null && s.StudentFeeHead.Name.ToLower().Contains(searchValue.ToLower())));
            }

            var filteredRecord = await query.CountAsync();

            var orderColumnLower = orderColumn?.ToLower() ?? "classroll";
            if (orderColumnLower.Contains("."))
            {
                orderColumnLower = orderColumnLower.Split('.').Last().ToLower();
            }
            var orderDirectionLower = orderDirection?.ToLower() ?? "asc";

            query = orderColumnLower switch
            {
                "classroll" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.Student.ClassRoll)
                    : query.OrderByDescending(s => s.Student.ClassRoll),
                "name" when orderColumnLower.Contains("student") => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.Student.Name)
                    : query.OrderByDescending(s => s.Student.Name),
                "studentname" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.Student.Name)
                    : query.OrderByDescending(s => s.Student.Name),
                "feetypename" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.StudentFeeHead.Name)
                    : query.OrderByDescending(s => s.StudentFeeHead.Name),
                "allocatedamount" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.AllocatedAmount)
                    : query.OrderByDescending(s => s.AllocatedAmount),
                "isactive" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.IsActive)
                    : query.OrderByDescending(s => s.IsActive),
                "editedat" => orderDirectionLower == "asc"
                    ? query.OrderBy(s => s.EditedAt)
                    : query.OrderByDescending(s => s.EditedAt),
                _ => query.OrderBy(s => s.Student.ClassRoll)
            };

            var data = await query.Skip(start).Take(length).ToListAsync();

            return (totalRecord, filteredRecord, data);
        }
    }
}
