using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IAcademicExamDetailsManager : IManager<AcademicExamDetail>
    {
        Task<List<AcademicExamDetail>> GetByExamIdAsync(int examId);
        Task<List<AcademicExamDetail>> GetAllByExamGroupAndStudentId(int examGroupId, int studentId);
        Task<AcademicExamDetail> GetLastDataFromExamByStudentId(int studentId);
    }
}
