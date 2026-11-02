using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.Enums;

namespace SMS.DAL.Contracts
{
    public interface IStudentCertificateRepository : IRepository<StudentCertificate>
    {
        Task<List<StudentCertificate>> GetAllByTypeAsync(CertificateType type);
        Task<List<StudentCertificate>> GetAllByStudentIdAsync(int studentId);
        Task<StudentCertificate> GetByIdWithStudentAsync(int id);
    }
}
