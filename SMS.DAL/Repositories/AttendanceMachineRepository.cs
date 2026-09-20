using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class AttendanceMachineRepository : Repository<Tran_MachineRawPunch>, IAttendanceMachineRepository
    {
        public AttendanceMachineRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<Tran_MachineRawPunch>> GetAllAttendanceByDateAsync(DateTime dateTime)
        {
            List<Tran_MachineRawPunch> allAttendance = await _context.Tran_MachineRawPunch
                .Where(t => t.PunchDatetime.Date == dateTime.Date)
                .ToListAsync();
            return allAttendance;
        }

        public async Task<IEnumerable<AttendanceVM>> GetAttendanceByDateAsync(string attendanceFor, string date, string attendanceType, int? aSessionId, int? aClassId)
        {
            // The date arrives from an <input type="date"> as yyyy-MM-dd; a
            // culture-default parse mis-reads that on Linux.
            if (!DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                parsedDate = DateTime.Today;

            var rawPunches = await _context.Tran_MachineRawPunch
                .Where(t => t.PunchDatetime.Date == parsedDate.Date)
                .ToListAsync();

            // One entry per enrolled PIN, holding that PIN's first punch of the day. Keyed by the
            // normalized PIN (see AttendancePinMatcher) so "0123" and "123" resolve to the same
            // entry - a raw string key here was why a student enrolled with a leading zero showed
            // up unpunched on this screen even though the monthly report matched them correctly.
            var firstPunchByPin = rawPunches
                .Select(p => new { Pin = AttendancePinMatcher.Normalize(p.CardNo), p.PunchDatetime })
                .Where(p => p.Pin != null)
                .GroupBy(p => p.Pin)
                .ToDictionary(g => g.Key, g => g.Min(p => p.PunchDatetime));

            // Candidates are tried in order, so the current enrolment scheme
            // (UniqueId / MachineUserId) wins and the legacy one (roll / id) only
            // answers for punches captured before it.
            string PunchTimeFor(params string[] candidatePins)
            {
                foreach (var pin in candidatePins)
                {
                    var normalized = AttendancePinMatcher.Normalize(pin);
                    if (normalized != null && firstPunchByPin.TryGetValue(normalized, out var first))
                        return first.ToString("hh:mm:ss tt");
                }
                return null;
            }

            var result = new List<AttendanceVM>();

            if (IsEmployeeAttendance(attendanceFor))
            {
                var employees = await _context.Employee
                    .Include(e => e.Designation)
                    .Where(e => e.Status)
                    .ToListAsync();

                result = employees.Select(e => new AttendanceVM
                {
                    CardNo = string.IsNullOrWhiteSpace(e.MachineUserId) ? e.Id.ToString() : e.MachineUserId,
                    Name = e.EmployeeName,
                    Class_Designation = e.Designation?.DesignationName,
                    Phone = e.Phone,
                    GuardianPhone = "",
                    PunchTime = PunchTimeFor(e.MachineUserId, e.Id.ToString()),
                    SectionId = null
                }).ToList();
            }
            else
            {
                var studentsQuery = _context.Student
                    .Include(s => s.AcademicClass)
                    .Include(s => s.AcademicSection)
                    .Where(s => s.Status)
                    .AsQueryable();

                if (aSessionId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.AcademicSessionId == aSessionId.Value);

                if (aClassId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.AcademicClassId == aClassId.Value);

                var students = await studentsQuery.ToListAsync();

                result = students.Select(s => new AttendanceVM
                {
                    CardNo = s.ClassRoll.ToString(),
                    Name = s.Name,
                    Class_Designation = s.AcademicClass?.Name,
                    Phone = s.PhoneNo,
                    GuardianPhone = s.GuardianPhone,
                    PunchTime = PunchTimeFor(s.UniqueId, s.ClassRoll.ToString()),
                    SectionId = s.AcademicSectionId
                }).ToList();
            }

            // "attended"/"absent" come from the search screen; anything else
            // (including the "all" default) means no filter.
            if (string.Equals(attendanceType, "attended", StringComparison.OrdinalIgnoreCase))
                result = result.Where(r => !string.IsNullOrEmpty(r.PunchTime)).ToList();
            else if (string.Equals(attendanceType, "absent", StringComparison.OrdinalIgnoreCase))
                result = result.Where(r => string.IsNullOrEmpty(r.PunchTime)).ToList();

            return result;
        }

        // The search form sends "employees"/"students"; accept every spelling
        // that has been in use rather than silently returning nothing when one drifts.
        private static bool IsEmployeeAttendance(string attendanceFor) =>
            !string.IsNullOrWhiteSpace(attendanceFor) &&
            (attendanceFor.Trim().Equals("e", StringComparison.OrdinalIgnoreCase) ||
             attendanceFor.Trim().StartsWith("employee", StringComparison.OrdinalIgnoreCase));

        public async Task<List<Tran_MachineRawPunch>> GetAttendanceByDateRangeAsync(string StartDate, string EndDate)
        {
            DateTime start = DateTime.Parse(StartDate);
            DateTime end = DateTime.Parse(EndDate);

            var attendanceList = await _context.Tran_MachineRawPunch
                .Where(t => t.PunchDatetime.Date >= start.Date && t.PunchDatetime.Date <= end.Date)
                .ToListAsync();
            return attendanceList;
        }

        public async Task<List<Tran_MachineRawPunch>> GetAttendanceByMonthSingleStudent(int studentId, string monthYear)
        {
            if (monthYear.Length >= 6 && int.TryParse(monthYear[..4], out int year) && int.TryParse(monthYear[^2..], out int month))
            {
                List<Tran_MachineRawPunch> allAttendance = await _context.Tran_MachineRawPunch
                    .Where(t => t.PunchDatetime.Year == year && t.PunchDatetime.Month == month
                        && t.CardNo == studentId.ToString())
                    .ToListAsync();
                return allAttendance;
            }

            return new List<Tran_MachineRawPunch>();
        }

        public async Task<List<Tran_MachineRawPunch>> GetCheckinDataByDateAsync(string date)
        {
            DateTime parsedDate = DateTime.Parse(date);
            try
            {
                var result = await _context.Tran_MachineRawPunch
                    .Where(t => t.PunchDatetime.Date == parsedDate.Date)
                    .ToListAsync();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Tran_MachineRawPunch>> GetEmpCheckinDataByDateAsync(string date)
        {
            DateTime parsedDate = DateTime.Parse(date);
            try
            {
                var result = await _context.Tran_MachineRawPunch
                    .Where(t => t.PunchDatetime.Date == parsedDate.Date)
                    .ToListAsync();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Tran_MachineRawPunch>> GetCheckOutDataByDateAsync(string date)
        {
            DateTime parsedDate = DateTime.Parse(date);
            try
            {
                var result = await _context.Tran_MachineRawPunch
                    .Where(t => t.PunchDatetime.Date == parsedDate.Date)
                    .ToListAsync();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Employee>> GetTodaysAbsentEmployeeAsync(string date)
        {
            var punchedPins = await GetPunchedPinsAsync(date);
            try
            {
                var activeEmployees = await _context.Employee
                    .Where(e => e.Status)
                    .ToListAsync();

                // A punch resolves through the enrolled PIN (MachineUserId);
                // legacy punches captured under the employee id still count.
                var absentEmployees = activeEmployees
                    .Where(e => !HasPunch(punchedPins, e.MachineUserId, e.Id.ToString()))
                    .ToList();

                return absentEmployees;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Student>> GetTodaysAbsentStudentAsync(string date)
        {
            var punchedPins = await GetPunchedPinsAsync(date);
            try
            {
                var activeStudents = await _context.Student
                    .Where(s => s.Status)
                    .ToListAsync();

                // A punch resolves through the enrolled PIN (UniqueId); legacy
                // punches captured under the class roll still count.
                var absentStudents = activeStudents
                    .Where(s => !HasPunch(punchedPins, s.UniqueId, s.ClassRoll.ToString()))
                    .ToList();

                return absentStudents;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Distinct, normalized CardNo values punched on the given day (see AttendancePinMatcher -
        // "0123" and "123" collapse to the same entry). The machine stores whatever PIN was
        // enrolled, so callers match it against the person's UniqueId / MachineUserId (with a
        // roll / id fallback).
        private async Task<HashSet<string>> GetPunchedPinsAsync(string date)
        {
            if (!DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                parsedDate = DateTime.Today;

            var cardNos = await _context.Tran_MachineRawPunch
                .Where(t => t.PunchDatetime.Date == parsedDate.Date)
                .Select(t => t.CardNo)
                .ToListAsync();

            return cardNos
                .Select(AttendancePinMatcher.Normalize)
                .Where(c => c != null)
                .ToHashSet();
        }

        private static bool HasPunch(HashSet<string> punchedPins, params string[] candidatePins)
        {
            foreach (var pin in candidatePins)
            {
                var normalized = AttendancePinMatcher.Normalize(pin);
                if (normalized != null && punchedPins.Contains(normalized))
                    return true;
            }
            return false;
        }

        public async Task<Tran_MachineRawPunch> GetTodaysAttendanceByUserIdAsync(int attendanceId)
        {
            // CardNo is matched against attendanceId (an int, so it never carries a leading zero
            // itself) with the same tolerant comparison used everywhere else - a query predicate
            // can't call AttendancePinMatcher, so today's punches are fetched once and matched in
            // memory (this also drops the previous, separate, unused fetch of the same rows).
            var todaysPunches = await _context.Tran_MachineRawPunch
                .Where(t => t.PunchDatetime.Date == DateTime.Now.Date)
                .ToListAsync();

            return todaysPunches.FirstOrDefault(m => AttendancePinMatcher.Matches(m.CardNo, attendanceId.ToString()));
        }

    }
}
