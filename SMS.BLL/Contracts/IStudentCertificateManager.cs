using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.Enums;

namespace SMS.BLL.Contracts
{
    public interface IStudentCertificateManager : IManager<StudentCertificate>
    {
        Task<List<StudentCertificate>> GetAllByTypeAsync(CertificateType type);
        Task<List<StudentCertificate>> GetAllByStudentIdAsync(int studentId);
        Task<StudentCertificate> GetByIdWithStudentAsync(int id);
    }
}
