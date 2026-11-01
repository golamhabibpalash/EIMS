using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;

namespace SMS.DAL.Repositories
{
    public class TestimonialRepository : Repository<Testimonial>, ITestimonialRepository
    {
        public TestimonialRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Testimonial>> GetAllWithStudentAsync() =>
            await Entity.Include(t => t.Student)
                .OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.Id)
                .ToListAsync();

        public async Task<List<Testimonial>> GetAllByStudentIdAsync(int studentId) =>
            await Entity.Where(t => t.StudentId == studentId)
                .OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.Id)
                .ToListAsync();

        public async Task<List<Testimonial>> GetPublishedAsync() =>
            await Entity.Include(t => t.Student)
                .Where(t => t.Status && t.IsPublished)
                .OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.Id)
                .ToListAsync();
    }
}
