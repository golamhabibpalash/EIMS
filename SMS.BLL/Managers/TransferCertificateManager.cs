using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;

namespace SMS.BLL.Managers
{
    public class TransferCertificateManager : Manager<TransferCertificate>, ITransferCertificateManager
    {
        private readonly ITransferCertificateRepository _tcRepository;

        public TransferCertificateManager(ITransferCertificateRepository tcRepository) : base(tcRepository)
        {
            _tcRepository = tcRepository;
        }

        public Task<List<TransferCertificate>> GetAllWithStudentAsync() => _tcRepository.GetAllWithStudentAsync();
        public Task<List<TransferCertificate>> GetAllByStudentIdAsync(int studentId) => _tcRepository.GetAllByStudentIdAsync(studentId);
        public Task<TransferCertificate> GetByIdWithStudentAsync(int id) => _tcRepository.GetByIdWithStudentAsync(id);

        public async Task<bool> CertificateNoExistsAsync(string certificateNo, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(certificateNo)) return false;
            var existing = await _tcRepository.GetByCertificateNoAsync(certificateNo.Trim());
            return existing != null && existing.Id != excludeId;
        }
    }
}
