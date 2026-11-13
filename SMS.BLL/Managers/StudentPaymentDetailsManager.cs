using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class StudentPaymentDetailsManager : Manager<StudentPaymentDetails>, IStudentPaymentDetailsManager
    {
        private readonly IStudentPaymentDetailsRepository _studentPaymentDetailRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        public StudentPaymentDetailsManager(IStudentPaymentDetailsRepository studentPaymentDetailsRepository, IStudentPaymentRepository studentPaymentRepository) : base(studentPaymentDetailsRepository)
        {
            _studentPaymentDetailRepository = studentPaymentDetailsRepository;
            _studentPaymentRepository = studentPaymentRepository;
        }

        public async Task<List<StudentPaymentDetails>> GetAllByPaymentId(int studentPaymentId)
        {
            return await _studentPaymentDetailRepository.GetAllByPaymentId(studentPaymentId);
        }
        public async Task<List<StudentPaymentDetails>> GetAllByStudentUniqueId(string uniqueId)
        {
            var allpaymentDetails = await _studentPaymentDetailRepository.GetAllAsync();
            var payments = await _studentPaymentRepository.GetAllByStudentUniqueIdAsync(uniqueId);
            List<StudentPaymentDetails> pd = new List<StudentPaymentDetails>();
            foreach (var payment in payments)
            {
                var d = await _studentPaymentDetailRepository.GetAllByPaymentId(payment.Id);
                pd.AddRange(d);
            }
            return pd;
        }

    }
}
