using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.Enums;

namespace SMS.BLL.Managers
{
    public class StudentCertificateManager : Manager<StudentCertificate>, IStudentCertificateManager
    {
        private readonly IStudentCertificateRepository _repo;

        public StudentCertificateManager(IStudentCertificateRepository repo) : base(repo)
        {
            _repo = repo;
        }

        public Task<List<StudentCertificate>> GetAllByTypeAsync(CertificateType type) => _repo.GetAllByTypeAsync(type);
        public Task<List<StudentCertificate>> GetAllByStudentIdAsync(int studentId) => _repo.GetAllByStudentIdAsync(studentId);
        public Task<StudentCertificate> GetByIdWithStudentAsync(int id) => _repo.GetByIdWithStudentAsync(id);
    }
}
