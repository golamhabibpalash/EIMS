using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAttendanceMachineRepository : IRepository<Tran_MachineRawPunch>
    {
        Task<List<Tran_MachineRawPunch>> GetAllAttendanceByDateAsync(DateTime dateTime);
        Task<IEnumerable<AttendanceVM>> GetAttendanceByDateAsync(string attendanceFor, string date, string attendanceType, int? aSessionId, int? aClassId);

        Task<Tran_MachineRawPunch> GetTodaysAttendanceByUserIdAsync(int attendanceId);
        Task<List<Tran_MachineRawPunch>> GetCheckinDataByDateAsync(string date);
        Task<List<Tran_MachineRawPunch>> GetEmpCheckinDataByDateAsync(string date);
        Task<List<Tran_MachineRawPunch>> GetCheckOutDataByDateAsync(string date);
        Task<List<Student>> GetTodaysAbsentStudentAsync(string date);
        Task<List<Tran_MachineRawPunch>> GetAttendanceByDateRangeAsync(string StartDate, string EndDate);
        Task<List<Employee>> GetTodaysAbsentEmployeeAsync(string date);
        Task<List<Tran_MachineRawPunch>> GetAttendanceByMonthSingleStudent(int studentId, string monthYear);

    }
}
