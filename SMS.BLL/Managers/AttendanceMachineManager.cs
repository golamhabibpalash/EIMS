using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AttendanceMachineManager : Manager<Tran_MachineRawPunch>, IAttendanceMachineManager
    {
        private readonly IAttendanceMachineRepository _attendanceMachineRepository;
        private readonly IAcademicSessionRepository _academicSessionRepository;

        public AttendanceMachineManager(IAttendanceMachineRepository attendanceMachineRepository, IAcademicSessionRepository academicSessionRepository) : base(attendanceMachineRepository)
        {
            _attendanceMachineRepository = attendanceMachineRepository;
            _academicSessionRepository = academicSessionRepository;
        }


        public async Task<List<Tran_MachineRawPunch>> GetAllAttendanceByDateAsync(DateTime dateTime)
        {
            return await _attendanceMachineRepository.GetAllAttendanceByDateAsync(dateTime);
        }

        public async Task<IEnumerable<AttendanceVM>> GetAttendanceByDateAsync(string attendanceFor, string date, string attendanceType, int? aSessionId, int? aClassId, int? aSectionId)
        {
            var result = await _attendanceMachineRepository.GetAttendanceByDateAsync(attendanceFor, date, attendanceType, aSessionId, aClassId);
            if(attendanceFor == "students")
            {
                if(aSectionId != null)
                {
                    result = result.Where(r => r.SectionId == aSectionId).ToList();
                }
            }
            return result.OrderByDescending(m => m.CardNo);
        }

        public async Task<List<Tran_MachineRawPunch>> GetAttendanceByDateRangeAsync(string StartDate, string EndDate)
        {
            var result = await _attendanceMachineRepository.GetAttendanceByDateRangeAsync(StartDate, EndDate);
            return result;
        }

        public async Task<List<Tran_MachineRawPunch>> GetAttendanceByMonthSingleStudent(int studentId, string monthYear)
        {
            var result = await _attendanceMachineRepository.GetAttendanceByMonthSingleStudent(studentId, monthYear);
            return result;
        }

        public async Task<List<Tran_MachineRawPunch>> GetCheckinDataByDateAsync(string date)
        {
            List<Tran_MachineRawPunch> allCheckInAttendnace = new();
            var result = await _attendanceMachineRepository.GetCheckinDataByDateAsync(date);
            if (result.Count > 0)
            {
                allCheckInAttendnace = result;
            }
            return allCheckInAttendnace;
        }
        public async Task<List<Tran_MachineRawPunch>> GetEmpCheckinDataByDateAsync(string date)
        {
            List<Tran_MachineRawPunch> allCheckInAttendnace = new List<Tran_MachineRawPunch>();
            var result = await _attendanceMachineRepository.GetEmpCheckinDataByDateAsync(date);
            if (result.Count > 0)
            {
                allCheckInAttendnace = result;
            }
            return allCheckInAttendnace;
        }

        public async Task<List<Tran_MachineRawPunch>> GetCheckOutDataByDateAsync(string date)
        {
            List<Tran_MachineRawPunch> allCheckOutAttendnace = new List<Tran_MachineRawPunch>();
            var result = await _attendanceMachineRepository.GetCheckOutDataByDateAsync(date);
            if (result.Count > 0)
            {
                allCheckOutAttendnace = result;
            }
            return allCheckOutAttendnace;
        }

        public async Task<List<Employee>> GetTodaysAbsentEmployeeAsync(string date)
        {
            List<Employee> employees = new List<Employee>();
            employees = await _attendanceMachineRepository.GetTodaysAbsentEmployeeAsync(date);
            return employees;
        }

        public async Task<List<Student>> GetTodaysAbsentStudentAsync(string date)
        {
            List<Student> students = new List<Student>();
            students = await _attendanceMachineRepository.GetTodaysAbsentStudentAsync(date);
            return students;
        }

        public async Task<Tran_MachineRawPunch> GetTodaysAttendanceByUserIdAsync(int attendanceId)
        {
            return await _attendanceMachineRepository.GetTodaysAttendanceByUserIdAsync(attendanceId);
        }

        public async Task<List<Tran_MachineRawPunch>> GetSessionWiseAttendanceByStudentUniqueIdAsync(int sessionId, string uniqueId)
        {
            var academicSession = await _academicSessionRepository.GetByIdAsync(sessionId);
            int sessionYear = Convert.ToInt32(academicSession.Name.Substring(academicSession.Name.Length - 4));

            // CardNo (raw punch, as the machine sent it) is matched against uniqueId with the same
            // leading-zero-tolerant comparison used everywhere else (AttendancePinMatcher) - that
            // can't be translated to SQL, so the year filter runs in the database and the PIN match
            // runs in memory over that one year's punches (one student's profile view, not a hot path).
            var yearPunches = await _attendanceMachineRepository.Table.AsNoTracking()
                .Where(s => s.PunchDatetime.Year == sessionYear)
                .ToListAsync();

            return yearPunches.Where(s => AttendancePinMatcher.Matches(s.CardNo, uniqueId)).ToList();
        }
    }
}
