using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.Enums;

namespace SMS.DAL.Repositories
{
    public class StudentCertificateRepository : Repository<StudentCertificate>, IStudentCertificateRepository
    {
        public StudentCertificateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<StudentCertificate>> GetAllByTypeAsync(CertificateType type) =>
            await Entity.Include(c => c.Student)
                .Where(c => c.Type == type)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

        public async Task<List<StudentCertificate>> GetAllByStudentIdAsync(int studentId) =>
            await Entity.Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

        public async Task<StudentCertificate> GetByIdWithStudentAsync(int id) =>
            await Entity.Include(c => c.Student).FirstOrDefaultAsync(c => c.Id == id);
    }
}
