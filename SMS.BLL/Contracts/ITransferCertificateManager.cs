using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.BLL.Contracts.Base;
using SMS.Entities;

namespace SMS.BLL.Contracts
{
    public interface ITransferCertificateManager : IManager<TransferCertificate>
    {
        Task<List<TransferCertificate>> GetAllWithStudentAsync();
        Task<List<TransferCertificate>> GetAllByStudentIdAsync(int studentId);
        Task<TransferCertificate> GetByIdWithStudentAsync(int id);

        /// <summary>True when the certificate number is already used by a different record.</summary>
        Task<bool> CertificateNoExistsAsync(string certificateNo, int excludeId = 0);
    }
}
