using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SMS.DAL.Repositories
{
    public class StudentActivateHistRepository : Repository<StudentActivateHist>, IStudentActivateHistRepository
    {
        private readonly IStudentRepository _studentRepository;
        public StudentActivateHistRepository(ApplicationDbContext context, IStudentRepository studentRepository) : base(context)
        {
            _studentRepository = studentRepository;
        }

        public async Task<bool> IsStudentActive(int id, string date)
        {
            Student existingStudenet = await _studentRepository.GetByIdAsync(id);
            DateTime qDate = Convert.ToDateTime(date);
            if (existingStudenet.AdmissionDate.Date <= qDate.Date)
            {
                var listOfActivationHistory = await GetExistingHistory(id, date);
                StudentActivateHist objStudentActivateHist;
                if (listOfActivationHistory.Count>0)
                {
                    objStudentActivateHist = (from t in listOfActivationHistory
                                              where t.ActionDateTime.Date <= qDate.Date
                                              select t).OrderByDescending(t => t.ActionDateTime).FirstOrDefault();

                    if (objStudentActivateHist != null)
                        return objStudentActivateHist.IsActive;

                    return existingStudenet.Status;
                }
                else
                {
                    return existingStudenet.Status;
                }
            }
            else
            {
                return false;
            }
        }

        private async Task<List<StudentActivateHist>> GetExistingHistory(int studentId, string date)
        {
            DateTime qDate = Convert.ToDateTime(date);
            List<StudentActivateHist> studentActivateHists = await _context.StudentActivateHists
                .Where(h => h.StudentId == studentId && h.ActionDateTime.Date == qDate.Date)
                .OrderBy(m => m.ActionDateTime)
                .ToListAsync();

            return studentActivateHists;
        }
    }
}
