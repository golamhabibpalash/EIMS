using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student> GetStudentByClassRollAsync(int classRoll);
        Task<Student> GetStudentByUniqueIdAsync(string uniqueId);
        /// <summary>
        /// The student a machine punch belongs to: by MachineUserId (the PIN set on the Machine
        /// Enrollment screen) first, then by UniqueId. Leading-zero tolerant. Roll is not tried here.
        /// </summary>
        Task<Student> GetStudentByAttendancePinAsync(string pin);
        Task<Student> GetStudentByClassRollAsync(int id, int classRoll);
        Task<List<Student>> GetStudentsByClassIdAndSessionIdAsync(int sessionId, int classId);
        Task<List<Student>> GetStudentsByClassSessionSectionAsync(int sessionId, int classId, int sectionId);
        /// <summary>
        /// Students for the export screen: every related lookup is eager-loaded (addresses,
        /// religion, blood group, nationality, etc.) so any exportable column can be rendered.
        /// Pass 0 for sessionId/classId/sectionId to mean "no filter on that dimension".
        /// </summary>
        Task<List<Student>> GetStudentsForExportAsync(int sessionId, int classId, int sectionId);
        Task<List<StudentListVM>> GetCurrentStudentListAsync(int? AcademicClassId, int? AcademicSectionId);
        Task<List<Student>> GetStudentsWithSectionBySectionIdAsync(int academicSectionId, int? classId = null, int? sessionId = null, bool? isResidential = null);
        Task<List<Student>> GetStudentsWithSectionByClassSessionResidentialAsync(int classId, int sessionId, bool isResidential);
    }
}
