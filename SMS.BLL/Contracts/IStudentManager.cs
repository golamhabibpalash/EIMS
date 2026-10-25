using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts;

public interface IStudentManager : IManager<Student>
{
    Task<Student> GetStudentByClassRollAsync(int classRoll);
    Task<Student> GetStudentByUniqueIdAsync(string uniqueId);
    /// <summary>The student a machine punch belongs to (MachineUserId, then UniqueId; leading-zero tolerant).</summary>
    Task<Student> GetStudentByAttendancePinAsync(string pin);
    Task<Student> GetStudentByClassRollAsync(int id, int classRoll);
    Task<List<Student>> GetStudentsByClassIdAndSessionIdAsync(int sessionId, int classId);
    Task<List<Student>> GetStudentsByClassSessionSectionAsync(int sessionId, int classId,int sectionId);
    Task<List<StudentListVM>> GetCurrentStudentListAsync(int? AcademicClassId, int? AcademicSectionId);
    Task<string> GetUniqueIdByStudentId(int stuId);
    Task<List<StudentListVM>> GetStudentsBySearch(string search);
    Task<ProfileAttendance> GetProfileAttendanceAsync(int id);
    Task<ProfileDocument> GetStudentProfileDocuments(int id);
    Task<List<Student>> GetStudentsWithSectionBySectionIdAsync(int academicSectionId, int? classId = null, int? sessionId = null, bool? isResidential = null);
    Task<List<Student>> GetStudentsWithSectionByClassSessionResidentialAsync(int classId, int sessionId, bool isResidential);
}
