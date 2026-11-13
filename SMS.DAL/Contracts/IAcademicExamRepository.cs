using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SMS.DAL.Contracts
{
    public interface IAcademicExamRepository:IRepository<AcademicExam>
    {
        Task<List<AcademicExam>> GetByClassIdExamGroupId(int examGroupId, int academicClassId);
        Task<List<AcademicExam>> GetAllByClassIdExamGroupId(int examGroupId, int academicClassId);
        Task<List<AcademicExam>> GetByClassIdExamGroupIdSectionId(int examGroupId, int academicClassId, int sectionId);
        Task<bool> IsDuplicateAsync(int examGroupId, int classId, int subjectId, int? sectionId, string examCategory);
        Task<List<(int SectionId, bool IsDuplicate)>> CheckDuplicatesBulkAsync(int examGroupId, int classId, int subjectId, string examCategory, List<int> sectionIds);
        Task<List<AcademicExam>> GetAllLiteAsync();
        Task<bool> RemoveByIdAsync(int id);
    }
}
