using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {

        public StudentRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<IReadOnlyCollection<Student>> GetAllAsync()
        {
            return await _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicSection)
                .Include(s => s.Gender)
                .OrderBy(s => s.AcademicClassId)
                .ThenBy(s => s.ClassRoll)
                .ToListAsync();
        }
        public override async Task<Student> GetByIdAsync(int id)
        {
            Student result = await _context.Student.Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Include(s => s.BloodGroup)
                .Include(s => s.Gender)
                .Include(s => s.Nationality)
                .Include(s => s.Religion)
                .Include(s => s.PresentUpazila)
                .Include(s => s.PresentDistrict)
                .Include(s => s.PresentDivision)
                .Include(s => s.PermanentUpazila)
                .Include(s => s.PermanentDistrict)
                .Include(s => s.PermanentDivision)
                .FirstOrDefaultAsync(s => s.Id == id);

            return result;
        }

        public async Task<List<StudentListVM>> GetCurrentStudentListAsync(int? AcademicClassId, int? AcademicSectionId)
        {
            var query = _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Include(s => s.Gender)
                .AsQueryable();

            if (AcademicClassId.HasValue)
                query = query.Where(s => s.AcademicClassId == AcademicClassId.Value);

            if (AcademicSectionId.HasValue)
                query = query.Where(s => s.AcademicSectionId == AcademicSectionId.Value);

            var studentListVMs = await query
                .Select(s => new StudentListVM
                {
                    Id = s.Id,
                    ClassRoll = s.ClassRoll,
                    Photo = s.Photo,
                    StudentName = s.Name,
                    NameBangla = s.NameBangla,
                    ClassName = s.AcademicClass.Name,
                    SectionName = s.AcademicSection.Name,
                    PhoneNo = s.PhoneNo,
                    SessionName = s.AcademicSession.Name,
                    Gender = s.Gender.Name,
                    Status = s.Status,
                    ClassSerial = s.AcademicClass.ClassSerial,
                    IsResidential = s.IsResidential,
                    UniqueId = s.UniqueId,
                    GuardianPhone = s.GuardianPhone,
                    AcademicSessionId = s.AcademicSessionId
                })
                .OrderBy(s => s.ClassSerial)
                .ThenBy(s => s.ClassRoll)
                .ToListAsync();

            return studentListVMs;
        }

        public async Task<Student> GetStudentByClassRollAsync(int classRoll)
        {
            var student = await _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicSection)
                .FirstOrDefaultAsync(s => s.ClassRoll == classRoll);
            return student;
        }
        public async Task<Student> GetStudentByUniqueIdAsync(string uniqueId)
        {
            // UniqueIds are inconsistently zero-padded across instances - and even
            // across rows ("027826618" vs "27826618") - while callers pass raw
            // machine CardNos, ReferenceIds and roll-derived strings. Exact match
            // first (preserves today's behavior for clean data), then the padded
            // variants, so a formatting difference can never silently drop a student
            // (empty payment history, missed SMS, blank dashboard) again.
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                return null;
            }
            var trimmed = uniqueId.Trim();

            var student = await _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicSection)
                .FirstOrDefaultAsync(s => s.UniqueId == trimmed);
            if (student != null)
            {
                return student;
            }

            var keys = AttendancePinMatcher.KeyVariants(trimmed);
            student = await _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicSection)
                .FirstOrDefaultAsync(s => s.UniqueId != null && keys.Contains(s.UniqueId.Trim()));
            return student;
        }

        public async Task<Student> GetStudentByClassRollAsync(int id, int classRoll)
        {
            return await _context.Student
                .FirstOrDefaultAsync(s => s.Id != id
                && s.ClassRoll == classRoll);
        }

        public async Task<List<Student>> GetStudentsByClassIdAndSessionIdAsync(int sessionId, int classId)
        {
            List<Student> students = await _context.Student
                .Include(s => s.AcademicSection)
                .Include(s => s.Gender)
                .Where(s => s.AcademicSessionId == sessionId
                && s.AcademicClassId == classId)
                .ToListAsync();
            return students;
        }

        public async Task<List<Student>> GetStudentsByClassSessionSectionAsync(int sessionId, int classId, int sectionId)
        {
            List<Student> students = new List<Student>();
            students = await _context
                .Student
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Include(s => s.AcademicClass)
                .Include(s => s.Gender)
                .Include(s => s.PresentDivision)
                .Include(s => s.PresentDistrict)
                .Include(s => s.PresentUpazila)
                .Include(s => s.PermanentDivision)
                .Include(s => s.PermanentDistrict)
                .Include(s => s.PermanentUpazila)
                .Include(s => s.Religion)
                .Include(s => s.BloodGroup)
                .ToListAsync();

            if (classId != 0)
            {
                students = students.Where(s => s.AcademicClassId == classId).ToList();
            }

            if (sectionId != 0)
            {
                students = (List<Student>)students.Where(s => s.AcademicSectionId == sectionId).ToList();
            }
            if (sessionId != 0)
            {
                students = (List<Student>)students.Where(s => s.AcademicSessionId == sessionId).ToList();
            }
            return students;
        }

        public async Task<List<Student>> GetStudentsWithSectionBySectionIdAsync(int academicSectionId, int? classId = null, int? sessionId = null, bool? isResidential = null)
        {
            var students = _context.Student
                .Include(s => s.AcademicSection)
                .Where(s => s.AcademicSectionId == academicSectionId);

            if (classId.HasValue)
                students = students.Where(s => s.AcademicClassId == classId.Value);

            if (sessionId.HasValue)
                students = students.Where(s => s.AcademicSessionId == sessionId.Value);

            if (isResidential.HasValue)
                students = students.Where(s => s.IsResidential == isResidential.Value);

            return await students.OrderBy(s => s.ClassRoll).ToListAsync();
        }

        public async Task<List<Student>> GetStudentsWithSectionByClassSessionResidentialAsync(int classId, int sessionId, bool isResidential)
        {
            return await _context.Student
                .Include(s => s.AcademicSection)
                .Where(s => s.AcademicClassId == classId
                    && s.AcademicSessionId == sessionId
                    && s.IsResidential == isResidential)
                .OrderBy(s => s.ClassRoll)
                .ToListAsync();
        }
    }
}
