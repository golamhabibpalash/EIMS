using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.DAL.Contracts.Base;
using SMS.Entities;

namespace SMS.DAL.Contracts
{
    public interface ITransferCertificateRepository : IRepository<TransferCertificate>
    {
        Task<List<TransferCertificate>> GetAllWithStudentAsync();
        Task<List<TransferCertificate>> GetAllByStudentIdAsync(int studentId);
        Task<TransferCertificate> GetByIdWithStudentAsync(int id);

        /// <summary>Finds a certificate by its number (used to enforce uniqueness). Null if none.</summary>
        Task<TransferCertificate> GetByCertificateNoAsync(string certificateNo);
    }
}
