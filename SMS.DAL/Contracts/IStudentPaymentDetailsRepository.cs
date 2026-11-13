using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IStudentPaymentDetailsRepository : IRepository<StudentPaymentDetails>
    {
        Task<List<StudentPaymentDetails>> GetAllByPaymentId(int studentPaymentId);
        Task<List<StudentPaymentDetails>> GetAllByStudentAsync(string studentUniqueId);
    }

}
