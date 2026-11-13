using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IStudentPaymentDetailsManager : IManager<StudentPaymentDetails>
    {
        Task<List<StudentPaymentDetails>> GetAllByPaymentId(int studentPaymentId);
        Task<List<StudentPaymentDetails>> GetAllByStudentUniqueId(string uniqueId);
    }
}
