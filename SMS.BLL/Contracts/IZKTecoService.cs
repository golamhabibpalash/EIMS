using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IZKTecoService
    {
        Task<bool> ConnectAsync(string ipAddress, int port, string username = "admin", string password = "admin");
        Task<bool> DisconnectAsync();
        Task<bool> IsConnectedAsync();
        Task<List<ZKTecoUser>> GetAllUsersAsync();
        Task<bool> SyncUsersAsync(IEnumerable<ZKTecoUser> users);
        Task<bool> DeleteUserAsync(string userId);
        Task<List<ZKTecoAttendanceLog>> GetAttendanceLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ZKTecoDeviceInfo> GetDeviceInfoAsync();
        Task<bool> ClearAttendanceLogsAsync();
        Task<bool> SetDeviceTimeAsync(DateTime dateTime);
    }

    public class ZKTecoUser
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Role { get; set; } = 0;
        public List<ZKTecoFingerprint> Fingerprints { get; set; } = new();
        public string CardNumber { get; set; }
    }

    public class ZKTecoFingerprint
    {
        public int FingerIndex { get; set; }
        public byte[] Template { get; set; }
    }

    public class ZKTecoAttendanceLog
    {
        public string UserId { get; set; }
        public DateTime DateTime { get; set; }
        public int VerifyMode { get; set; }
        public int InOutMode { get; set; }
        public int WorkCode { get; set; }
    }

    public class ZKTecoDeviceInfo
    {
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string Platform { get; set; }
        public int UserCount { get; set; }
        public int FingerprintCount { get; set; }
        public int LogCount { get; set; }
        public int FaceCount { get; set; }
    }
}