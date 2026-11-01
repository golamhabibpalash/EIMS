using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;

namespace SMS.DAL.Repositories
{
    public class TransferCertificateRepository : Repository<TransferCertificate>, ITransferCertificateRepository
    {
        public TransferCertificateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<TransferCertificate>> GetAllWithStudentAsync() =>
            await Entity.Include(t => t.Student)
                .OrderByDescending(t => t.IssueDate).ThenByDescending(t => t.Id)
                .ToListAsync();

        public async Task<List<TransferCertificate>> GetAllByStudentIdAsync(int studentId) =>
            await Entity.Where(t => t.StudentId == studentId)
                .OrderByDescending(t => t.IssueDate).ThenByDescending(t => t.Id)
                .ToListAsync();

        public async Task<TransferCertificate> GetByIdWithStudentAsync(int id) =>
            await Entity.Include(t => t.Student).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<TransferCertificate> GetByCertificateNoAsync(string certificateNo) =>
            await Entity.FirstOrDefaultAsync(t => t.CertificateNo == certificateNo);
    }
}
