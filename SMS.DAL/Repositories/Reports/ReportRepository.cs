using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts.Reports;
using SMS.DB;
using SMS.Entities.AdditionalModels;
using SMS.Entities.RptModels;
using SMS.Entities.RptModels.AttendanceVM;
using SMS.Entities.RptModels.Results;
using SMS.Entities.RptModels.StudentPayment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RptAdmitCardVM>> GetAdmitCard(int monthId, int academicClassId, int academicSectionId, int examTypeId, int examGroupId = 0)
        {
            var query = _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Include(s => s.Gender)
                .Include(s => s.Religion)
                .AsQueryable();

            var examsQuery = _context.AcademicExams
                .Include(e => e.AcademicExamGroup)
                .Include(e => e.AcademicSubject)
                .Where(e => e.AcademicExamGroup.ExamMonthId == monthId
                    && e.AcademicExamGroup.AcademicExamTypeId == examTypeId);

            if (examGroupId > 0)
            {
                examsQuery = examsQuery.Where(e => e.AcademicExamGroupId == examGroupId);
            }

            var exams = await examsQuery.ToListAsync();

            if (academicClassId > 0)
            {
                var examClassIds = exams.Where(e => e.AcademicClassId == academicClassId).Select(e => e.AcademicClassId).Distinct();
                query = query.Where(s => s.AcademicClassId == academicClassId);
                exams = exams.Where(e => e.AcademicClassId == academicClassId).ToList();
            }

            if (academicSectionId > 0)
            {
                query = query.Where(s => s.AcademicSectionId == academicSectionId);
            }

            var institute = await _context.Institute.FirstOrDefaultAsync();

            var students = await query.ToListAsync();

            var admitCards = students
                .SelectMany(s => exams.Where(e =>
                    e.AcademicClassId == s.AcademicClassId
                    && (!e.AcademicSectionId.HasValue || e.AcademicSectionId == s.AcademicSectionId))
                    .Select(e => new RptAdmitCardVM
                    {
                        StudentId = s.Id,
                        ClassRoll = s.ClassRoll,
                        StudentName = s.Name,
                        FatherName = s.FatherName,
                        MotherName = s.MotherName,
                        SessionName = s.AcademicSession?.Name,
                        ClassName = s.AcademicClass?.Name,
                        SectionName = s.AcademicSection?.Name,
                        AcademicSectionId = s.AcademicSectionId,
                        MonthId = monthId,
                        SubjectCode = e.AcademicSubject?.SubjectCode,
                        SubjectName = e.AcademicSubject?.SubjectName,
                        AcademicClassId = s.AcademicClassId,
                        ExamTypeName = e.AcademicExamGroup?.AcademicExamType?.ExamTypeName,
                        ExamGroupName = e.AcademicExamGroup?.ExamGroupName,
                        InstituteName = institute?.Name,
                        EIIN = institute?.EIIN,
                        GenderId = s.GenderId,
                        Gender = s.Gender?.Name,
                        Religion = s.Religion?.Name,
                        StudentStauts = s.Status
                    }))
                .OrderBy(a => a.ClassRoll)
                .ToList();

            return admitCards;
        }

        public async Task<List<rptStudentPaymentsVM>> GetStudentPaymentsByRoll(int classRoll, string fromDate, string toDate)
        {
            List<rptStudentPaymentsVM> rptStudentPaymentsVMs = new List<rptStudentPaymentsVM>();

            var student = await _context.Student.FirstOrDefaultAsync(s => s.ClassRoll == classRoll);
            if (student == null) return rptStudentPaymentsVMs;

            DateTime from = DateTime.Parse(fromDate);
            DateTime to = DateTime.Parse(toDate);

            rptStudentPaymentsVMs = await _context.StudentPayment
                .Where(sp => sp.StudentId == student.Id && sp.PaidDate >= from && sp.PaidDate <= to)
                .Select(sp => new rptStudentPaymentsVM
                {
                    ReceiptNo = sp.ReceiptNo,
                    PaidDate = sp.PaidDate.ToString("yyyy-MM-dd"),
                    PaymentTypeName = string.Join(", ", sp.StudentPaymentDetails.Select(d => d.StudentFeeHead.Name)),
                    TotalPayment = sp.TotalPayment,
                    Remarks = sp.Remarks
                })
                .ToListAsync();

            return rptStudentPaymentsVMs;
        }

        public async Task<List<RptStudentVM>> getStudentsInfo(int AcademicSessionId, int? AcademicClassId, int? AcademicSectionId)
        {
            var query = _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Include(s => s.Gender)
                .Include(s => s.Religion)
                .Include(s => s.BloodGroup)
                .Include(s => s.Nationality)
                .Where(s => s.AcademicSessionId == AcademicSessionId)
                .AsQueryable();

            if (AcademicClassId.HasValue)
                query = query.Where(s => s.AcademicClassId == AcademicClassId.Value);

            if (AcademicSectionId.HasValue)
                query = query.Where(s => s.AcademicSectionId == AcademicSectionId.Value);

            var rptStudentVMs = await query
                .Select(s => new RptStudentVM
                {
                    ClassRoll = s.ClassRoll.ToString(),
                    StudentName = s.Name,
                    ClassName = s.AcademicClass.Name,
                    SessionName = s.AcademicSession.Name,
                    FatherName = s.FatherName,
                    MotherName = s.MotherName,
                    GuardianPhone = s.GuardianPhone,
                    PhoneNo = s.PhoneNo,
                    Gender = s.Gender.Name,
                    Religion = s.Religion.Name,
                    Status = s.Status ? "Active" : "Inactive",
                    BloodGroup = s.BloodGroup.Name,
                    SectionName = s.AcademicSection.Name,
                    AcademicClassId = s.AcademicClassId.ToString(),
                    AcademicSectionId = s.AcademicSectionId.ToString(),
                    UniqueId = s.UniqueId
                })
                .ToListAsync();

            return rptStudentVMs;
        }
        public async Task<List<RptStudentsPaymentVM>> GetStudentPayment(string fromDate, string ToDate, string AcademicClassId, string AcademicSectionId)
        {
            DateTime from = DateTime.Parse(fromDate);
            DateTime to = DateTime.Parse(ToDate);

            var query = _context.StudentPayment
                .Include(sp => sp.Student).ThenInclude(s => s.AcademicClass)
                .Include(sp => sp.Student).ThenInclude(s => s.AcademicSection)
                .Include(sp => sp.StudentPaymentDetails)
                .Where(sp => sp.PaidDate >= from && sp.PaidDate <= to)
                .AsQueryable();

            if (!string.IsNullOrEmpty(AcademicClassId) && AcademicClassId != "null" && int.TryParse(AcademicClassId, out int classId))
            {
                query = query.Where(sp => sp.Student.AcademicClassId == classId);
            }

            if (!string.IsNullOrEmpty(AcademicSectionId) && AcademicSectionId != "null" && int.TryParse(AcademicSectionId, out int sectionId))
            {
                query = query.Where(sp => sp.Student.AcademicSectionId == sectionId);
            }

            var rptStudentsPayments = await query
                .Select(sp => new RptStudentsPaymentVM
                {
                    ClassRoll = sp.Student.ClassRoll.ToString(),
                    StudentName = sp.Student.Name,
                    AcademicSection = sp.Student.AcademicSection.Name,
                    PaymentType = "",
                    ReceiptNo = sp.ReceiptNo,
                    Remarks = sp.Remarks,
                    PaidDate = sp.PaidDate.ToString("yyyy-MM-dd"),
                    TotalPayment = sp.TotalPayment,
                    AcademicClassId = sp.Student.AcademicClassId.ToString(),
                    AcademicSectionId = sp.Student.AcademicSectionId.ToString(),
                    AcademicClassName = sp.Student.AcademicClass.Name,
                    IsResidential = sp.Student.IsResidential
                })
                .ToListAsync();

            return rptStudentsPayments;
        }

        public async Task<List<RptDailyAttendaceVM>> GetDailyAttendanceReport(string fromDate, string AcademicClassId, string AcademicSectionId, string attendanceType, string aSessionId, string attendanceFor)
        {
            return await BuildDailyAttendanceAsync(fromDate, AcademicClassId, AcademicSectionId, attendanceType, aSessionId, attendanceFor, checkOut: false);
        }

        public async Task<List<RptDailyAttendaceVM>> GetDailyAttendanceReportCheckOut(string fromDate, string AcademicClassId, string AcademicSectionId, string attendanceType, string aSessionId, string attendanceFor)
        {
            return await BuildDailyAttendanceAsync(fromDate, AcademicClassId, AcademicSectionId, attendanceType, aSessionId, attendanceFor, checkOut: true);
        }

        /// <summary>
        /// Check-in and check-out differ only in which punch of the day they
        /// report, so both share this. A person's first punch of the day is the
        /// check-in and their last is the check-out; somebody who punched only
        /// once has arrived but not left, so their check-out stays blank rather
        /// than repeating the arrival time back at the reader.
        /// </summary>
        private async Task<List<RptDailyAttendaceVM>> BuildDailyAttendanceAsync(
            string fromDate, string academicClassId, string academicSectionId,
            string attendanceType, string aSessionId, string attendanceFor, bool checkOut)
        {
            // Explicit invariant parse: the date arrives from an <input type="date">
            // as yyyy-MM-dd, which a culture-default parse mis-reads on Linux.
            if (!DateTime.TryParse(fromDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                date = DateTime.Today;

            var punches = await _context.Tran_MachineRawPunch
                .AsNoTracking()
                .Where(t => t.PunchDatetime.Date == date.Date)
                .ToListAsync();

            // One entry per PIN, holding that PIN's first and last punch of the day.
            var punchesByPin = punches
                .Where(p => !string.IsNullOrWhiteSpace(p.CardNo))
                .GroupBy(p => p.CardNo.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        First = g.Min(p => p.PunchDatetime),
                        Last = g.Max(p => p.PunchDatetime),
                        Count = g.Count()
                    });

            // Which numbers already received an attendance SMS today. Without
            // this every row reported "", so the SMS Sent / Not Sent filter on
            // the report screen could never match anything.
            var smsType = checkOut ? "CheckOut" : "CheckIn";
            var notifiedNumbers = (await _context.PhoneSMS
                    .AsNoTracking()
                    .Where(s => s.SMSType == smsType && s.CreatedAt.Date == date.Date)
                    .Select(s => s.MobileNumber)
                    .ToListAsync())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .ToHashSet();

            // Candidates are tried in order, so the current enrolment scheme
            // wins and the legacy one only answers for older punches.
            string PunchTimeFor(params string[] candidatePins)
            {
                foreach (var pin in candidatePins)
                {
                    if (string.IsNullOrWhiteSpace(pin) || !punchesByPin.TryGetValue(pin.Trim(), out var day))
                        continue;

                    if (checkOut)
                        return day.Count > 1 ? day.Last.ToString("hh:mm:ss tt") : null;

                    return day.First.ToString("hh:mm:ss tt");
                }

                return null;
            }

            bool WasNotified(string number) =>
                !string.IsNullOrWhiteSpace(number) && notifiedNumbers.Contains(number.Trim());

            var result = new List<RptDailyAttendaceVM>();

            if (IsEmployeeReport(attendanceFor))
            {
                var employees = await _context.Employee
                    .AsNoTracking()
                    .Include(e => e.Designation)
                    .Where(e => e.Status)
                    .ToListAsync();

                result = employees
                    .OrderBy(e => e.Designation != null ? e.Designation.DesignationName : string.Empty)
                    .ThenBy(e => e.EmployeeName)
                    .Select(e => new RptDailyAttendaceVM
                    {
                        // MachineUserId is what gets enrolled on the terminal;
                        // e.Id is the fallback for punches recorded before that
                        // field was in use.
                        CardNo = string.IsNullOrWhiteSpace(e.MachineUserId) ? e.Id.ToString() : e.MachineUserId,
                        ClassRoll = "",
                        Name = e.EmployeeName,
                        Class_Designation = e.Designation?.DesignationName,
                        Phone = e.Phone,
                        GuardianPhone = "",
                        PunchTime = PunchTimeFor(e.MachineUserId, e.Id.ToString()),
                        SortingOrder = e.Designation?.DesignationName,
                        SMSSent = WasNotified(e.Phone) ? "Sent" : "Not Sent",
                        ClassSL = ""
                    })
                    .ToList();
            }
            else
            {
                var studentsQuery = _context.Student
                    .AsNoTracking()
                    .Include(s => s.AcademicClass)
                    .Include(s => s.AcademicSection)
                    .Where(s => s.Status)
                    .AsQueryable();

                // Without the session filter the report listed every student who
                // has ever been enrolled, previous years included.
                if (!string.IsNullOrEmpty(aSessionId) && int.TryParse(aSessionId, out int sessionId))
                    studentsQuery = studentsQuery.Where(s => s.AcademicSessionId == sessionId);

                if (!string.IsNullOrEmpty(academicClassId) && int.TryParse(academicClassId, out int cId))
                    studentsQuery = studentsQuery.Where(s => s.AcademicClassId == cId);

                if (!string.IsNullOrEmpty(academicSectionId) && int.TryParse(academicSectionId, out int sId))
                    studentsQuery = studentsQuery.Where(s => s.AcademicSectionId == sId);

                var students = await studentsQuery.ToListAsync();

                result = students
                    // ClassSerial sorted as a number: as a string "10" ordered
                    // ahead of "2".
                    .OrderBy(s => s.AcademicClass != null ? s.AcademicClass.ClassSerial : int.MaxValue)
                    .ThenBy(s => s.ClassRoll)
                    .Select(s => new RptDailyAttendaceVM
                    {
                        // The terminal is enrolled with UniqueId; ClassRoll is
                        // kept as a fallback so punches captured under the old
                        // scheme still resolve.
                        CardNo = string.IsNullOrWhiteSpace(s.UniqueId) ? s.ClassRoll.ToString() : s.UniqueId,
                        ClassRoll = s.ClassRoll.ToString(),
                        Name = s.Name,
                        Class_Designation = s.AcademicClass?.Name,
                        Phone = s.PhoneNo,
                        GuardianPhone = s.GuardianPhone,
                        PunchTime = PunchTimeFor(s.UniqueId, s.ClassRoll.ToString()),
                        SortingOrder = s.AcademicClass?.ClassSerial.ToString(),
                        SMSSent = WasNotified(s.GuardianPhone) ? "Sent" : "Not Sent",
                        ClassSL = s.AcademicClass?.ClassSerial.ToString()
                    })
                    .ToList();
            }

            // "attended"/"absent" come from the report screen; anything else,
            // including the unselected placeholder, means no filter.
            if (string.Equals(attendanceType, "attended", StringComparison.OrdinalIgnoreCase))
                result = result.Where(r => !string.IsNullOrEmpty(r.PunchTime)).ToList();
            else if (string.Equals(attendanceType, "absent", StringComparison.OrdinalIgnoreCase))
                result = result.Where(r => string.IsNullOrEmpty(r.PunchTime)).ToList();

            return result;
        }

        /// <summary>
        /// The report screen sends "e"/"s" and the controller expands those to
        /// words. Accept every spelling that has been in use rather than
        /// silently returning an empty report when one of them drifts.
        /// </summary>
        private static bool IsEmployeeReport(string attendanceFor) =>
            !string.IsNullOrWhiteSpace(attendanceFor) &&
            (attendanceFor.Trim().Equals("e", StringComparison.OrdinalIgnoreCase) ||
             attendanceFor.Trim().StartsWith("employee", StringComparison.OrdinalIgnoreCase));

        public async Task<List<RptPaymentReceiptVM>> GetPaymentReceiptReport(int paymentId)
        {
            List<RptPaymentReceiptVM> rptPaymentReceiptVMs;
            try
            {
                rptPaymentReceiptVMs = await _context.StudentPayment
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicClass)
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicSection)
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicSession)
                    .Include(sp => sp.StudentPaymentDetails).ThenInclude(pd => pd.StudentFeeHead)
                    .Where(sp => sp.Id == paymentId)
                    .SelectMany(sp => sp.StudentPaymentDetails.Select(pd => new RptPaymentReceiptVM
                    {
                        ReceiptNo = sp.ReceiptNo,
                        Student_Name = sp.Student.Name,
                        Class_Name = sp.Student.AcademicClass.Name,
                        PaidDate = sp.PaidDate,
                        ClassRoll = sp.Student.ClassRoll,
                        Section_Name = sp.Student.AcademicSection.Name,
                        Fee_Head = pd.StudentFeeHead.Name,
                        PaidAmount = pd.PaidAmount,
                        TotalPayment = sp.TotalPayment
                    }))
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return rptPaymentReceiptVMs;
        }
        public async Task<List<SubjectWiseMarkSheetVM>> GetSubjectWiseMarkSheet(int examId)
        {
            var result = await _context.AcademicExamDetails
                .Include(aed => aed.AcademicExam).ThenInclude(e => e.AcademicSubject)
                .Include(aed => aed.Student)
                .Where(aed => aed.AcademicExamId == examId)
                .Select(aed => new SubjectWiseMarkSheetVM
                {
                    ObtainMark = aed.ObtainMark,
                    Name = aed.Student.Name,
                    StudentId = aed.StudentId,
                    ClassRoll = aed.Student.ClassRoll,
                    Status = aed.Status,
                    Remarks = aed.Remarks,
                    AcademicExamId = aed.AcademicExamId,
                    LetterGrade = "",
                    GradePoint = 0
                })
                .OrderBy(r => r.ClassRoll)
                .ToListAsync();

            return result;
        }
        public async Task<List<StudentWiseMarkSheetVM>> GetStudentWiseMarkSheet(int examGroupId, int classId)
        {
            var result = await _context.ExamResults
                .Where(r => r.AcademicExamGroupId == examGroupId && r.AcademicClassId == classId)
                .SelectMany(r => r.ExamResultDetails.Select(erd => new StudentWiseMarkSheetVM
                {
                    ExamGroupName = r.AcademicExamGroup.ExamGroupName,
                    ClassName = r.AcademicClass.Name,
                    StudentName = r.Student.Name,
                    FatherName = r.Student.FatherName,
                    MotherName = r.Student.MotherName,
                    ClassRoll = r.Student.ClassRoll,
                    SectionName = r.Student.AcademicSection.Name,
                    AcademicSectionId = r.Student.AcademicSectionId,
                    GenderName = r.Student.Gender.Name,
                    SubjectName = erd.AcademicSubject.SubjectName,
                    TotalMark = erd.TotalMark,
                    ObtainMark = erd.ObtainMark,
                    GPA = erd.GPA,
                    Grade = erd.Grade,
                    MaxNumber = erd.TotalMark,
                    FinalGPA = r.CGPA,
                    FinalGrade = r.FinalGrade,
                    AttendancePercentage = r.AttendancePercentage,
                    TotalObtainMarks = r.TotalObtainMarks,
                    TotalFails = r.TotalFails,
                    MeritPosition = r.Rank,
                    GradeComments = r.GradeComments,
                    ExamGroupId = examGroupId,
                    AcademicClassId = classId,
                    StudentId = r.StudentId,
                    DOB = r.Student.DOB,
                    ReligionName = r.Student.Religion.Name,
                    CreatedAt = r.CreatedAt
                }))
                .OrderBy(r => r.ClassRoll)
                .ThenBy(r => r.SubjectName)
                .ToListAsync();

            return result;
        }
    }
}
