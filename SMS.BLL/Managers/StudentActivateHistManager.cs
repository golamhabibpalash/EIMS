using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class StudentActivateHistManager : Manager<StudentActivateHist>, IStudentActivateHistManager
    {
        private readonly IStudentActivateHistRepository _studentActivateHistRepository;
        private readonly IStudentManager _studentManager;

        public StudentActivateHistManager(IStudentActivateHistRepository repository, IStudentManager studentManager) : base(repository)
        {
            _studentActivateHistRepository = repository;
            _studentManager = studentManager;
        }

        public async Task<List<StudentActivateHist>> GetActivityListByUniqueId(string uniqueId)
        {
            var exisistingStudent = await _studentManager.GetStudentByUniqueIdAsync(uniqueId);

            var studentActivityHistory = await _studentActivateHistRepository.Table.Where(s => s.StudentId == exisistingStudent.Id).ToListAsync();
            return studentActivityHistory;

        }

        public async Task<bool> IsStudentActive(int id, string date)
        {
            return await _studentActivateHistRepository.IsStudentActive(id, date);
        }

    }
}
