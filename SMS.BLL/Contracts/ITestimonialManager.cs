using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.BLL.Contracts.Base;
using SMS.Entities;

namespace SMS.BLL.Contracts
{
    public interface ITestimonialManager : IManager<Testimonial>
    {
        Task<List<Testimonial>> GetAllWithStudentAsync();
        Task<List<Testimonial>> GetAllByStudentIdAsync(int studentId);
        Task<List<Testimonial>> GetPublishedAsync();
    }
}
