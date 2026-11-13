using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts;

public interface IAcademicExamManager:IManager<AcademicExam>
{
    Task<List<AcademicExam>> GetByClassIdExamGroupIdAsync(int examGroupId, int academicClassId);
    Task<List<AcademicExam>> GetAllByClassIdExamGroupIdAsync(int examGroupId, int academicClassId);
    Task<List<AcademicExam>> GetByClassIdExamGroupIdSectionIdAsync(int examGroupId, int academicClassId, int sectionId);
    Task<List<ExamSessionDto>> GetExaminationListAsync();
    Task<List<ExamSessionDto>> GetExaminationListLiteAsync();
    Task<LiveResultVM> GetLiveResultByGroupIdClassIdSectionId(int academicGroupId, int academiClassId, int? academicSectionId);
    Task<AcademicExam> GetAcademicExam(int examGroupId, int classId, int subjectId, string examType, int? sectionId);
    Task<int> GetTotalExamAsync(int examGroupId, int classId);
    Task<List<AcademicClass>> GetAcademicClassListByGroupIdAsync(int groupId);
    Task<ProfileResult> GetSingleStudentResultDetailForProfile(int studentId);
    Task<bool> IsDuplicateAsync(int examGroupId, int classId, int subjectId, int? sectionId, string examCategory);
    Task<List<(int SectionId, bool IsDuplicate)>> CheckDuplicatesBulkAsync(int examGroupId, int classId, int subjectId, string examCategory, List<int> sectionIds);
    Task<List<AcademicExam>> GetMergedExamsAsync(int examId);
    Task<List<Student>> GetStudentsByExamIdAsync(int examId);
    Task<bool> RemoveByIdAsync(int id);
}
