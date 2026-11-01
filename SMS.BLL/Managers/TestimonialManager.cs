using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;

namespace SMS.BLL.Managers
{
    public class TestimonialManager : Manager<Testimonial>, ITestimonialManager
    {
        private readonly ITestimonialRepository _testimonialRepository;

        public TestimonialManager(ITestimonialRepository testimonialRepository) : base(testimonialRepository)
        {
            _testimonialRepository = testimonialRepository;
        }

        public Task<List<Testimonial>> GetAllWithStudentAsync() => _testimonialRepository.GetAllWithStudentAsync();
        public Task<List<Testimonial>> GetAllByStudentIdAsync(int studentId) => _testimonialRepository.GetAllByStudentIdAsync(studentId);
        public Task<List<Testimonial>> GetPublishedAsync() => _testimonialRepository.GetPublishedAsync();
    }
}
