using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IAttendanceMachineService
    {
        Task<AttendanceMachine> GetByIdAsync(int id);
        Task<IEnumerable<AttendanceMachine>> GetAllAsync();
        Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync();
        Task<bool> AddAsync(AttendanceMachine machine);
        Task<bool> UpdateAsync(AttendanceMachine machine);
        Task<bool> DeleteAsync(int id);
        Task<bool> TestConnectionAsync(int machineId);
        Task<MachineSyncResult> SyncUsersToMachineAsync(int machineId);
        Task<MachinePullResult> PullAttendanceFromMachineAsync(int machineId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<MachineHealthStatus> GetMachineHealthAsync(int machineId);
    }

    public class MachineSyncResult
    {
        public bool Success { get; set; }
        public int TotalUsers { get; set; }
        public int SyncedUsers { get; set; }
        public int FailedUsers { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; }
    }

    public class MachinePullResult
    {
        public bool Success { get; set; }
        public int RecordsPulled { get; set; }
        public int RecordsSaved { get; set; }
        public int DuplicateRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; }
    }

    public class MachineHealthStatus
    {
        public bool IsOnline { get; set; }
        public DateTime? LastContact { get; set; }
        public string FirmwareVersion { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRecords { get; set; }
        public string Error { get; set; }
    }
}