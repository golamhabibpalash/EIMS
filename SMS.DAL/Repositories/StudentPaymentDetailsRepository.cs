using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class StudentPaymentDetailsRepository : Repository<StudentPaymentDetails>, IStudentPaymentDetailsRepository
    {
        private new readonly ApplicationDbContext _context;
        public StudentPaymentDetailsRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }
        public override async Task<IReadOnlyCollection<StudentPaymentDetails>> GetAllAsync()
        {
            return await _context.StudentPaymentDetails
                .Include(s => s.StudentFeeHead)
                .Include(s => s.StudentPayment)
                .ToListAsync();
        }

        public async Task<List<StudentPaymentDetails>> GetAllByPaymentId(int studentPaymentId)
        {
            return await _context.StudentPaymentDetails
                .Where(s => s.StudentPaymentId == studentPaymentId)
                .ToListAsync();
        }

        public async Task<List<StudentPaymentDetails>> GetAllByStudentAsync(string studentUniqueId)
        {
            // Same zero-padding history as StudentPayment.UniqueId - match variants.
            var keys = AttendancePinMatcher.KeyVariants(studentUniqueId);
            return await _context.StudentPaymentDetails
                .Where(d => d.StudentPayment.UniqueId != null && keys.Contains(d.StudentPayment.UniqueId.Trim()))
                .Include(d => d.StudentPayment)
                .Include(d => d.StudentFeeHead)
                .ToListAsync();
        }

        public override async Task<StudentPaymentDetails> GetByIdAsync(int id)
        {
            return await _context.StudentPaymentDetails
                .Include(s => s.StudentFeeHead)
                .Include(s => s.StudentPayment)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
