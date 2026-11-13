using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers;

public class AcademicExamDetailsManager : Manager<AcademicExamDetail>, IAcademicExamDetailsManager
{
    IAcademicExamDetailsRepository _academicExamDetailsRepository;
    public AcademicExamDetailsManager(IAcademicExamDetailsRepository academicExamDetailsRepository) :base(academicExamDetailsRepository)
    {
        _academicExamDetailsRepository = academicExamDetailsRepository;
    }

    public async Task<List<AcademicExamDetail>> GetAllByExamGroupAndStudentId(int examGroupId, int studentId)
    {
        var result = await _academicExamDetailsRepository.GetAllByExamGroupAndStudentId(examGroupId, studentId);
        return result;
    }

    public async Task<List<AcademicExamDetail>> GetByExamIdAsync(int examId)
    {
        return await _academicExamDetailsRepository.GetByExamIdAsync(examId);
    }
    public async Task<AcademicExamDetail> GetLastDataFromExamByStudentId(int studentId)
    {
        return await _academicExamDetailsRepository
            .Table      // AcademicExam → AcademicSubject
            .Include(s => s.AcademicExam)
                .ThenInclude(c => c.AcademicExamGroup)     // AcademicExam → AcademicExamGroup
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }
}
