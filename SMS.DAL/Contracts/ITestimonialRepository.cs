using System.Collections.Generic;
using System.Threading.Tasks;
using SMS.DAL.Contracts.Base;
using SMS.Entities;

namespace SMS.DAL.Contracts
{
    public interface ITestimonialRepository : IRepository<Testimonial>
    {
        Task<List<Testimonial>> GetAllWithStudentAsync();
        Task<List<Testimonial>> GetAllByStudentIdAsync(int studentId);

        /// <summary>Active + published testimonials, in display order, for public presentation.</summary>
        Task<List<Testimonial>> GetPublishedAsync();
    }
}
