using SMS.BLL.Contracts;
using SMS.BLL.ZKTeco;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Real implementation of IZKTecoService using a pure TCP ZK protocol client.
    /// Cross-platform (no zkemkeeper.dll COM dependency) — works on Linux deployments.
    /// Usage pattern: ConnectAsync → operations → DisconnectAsync (one device session per scope).
    /// </summary>
    public class ZKTecoService : IZKTecoService
    {
        private const int DefaultTimeoutSeconds = 30;

        private ZkClient _client;
        private string _ipAddress;
        private int _port;

        public async Task<bool> ConnectAsync(string ipAddress, int port, string username = "admin", string password = "admin")
        {
            try
            {
                await DisconnectQuietlyAsync();

                _ipAddress = ipAddress;
                _port = port;
                _client = new ZkClient(ipAddress, port, DefaultTimeoutSeconds);
                await _client.ConnectAsync(ParseCommPassword(password));
                return true;
            }
            catch
            {
                _client?.Dispose();
                _client = null;
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_client != null)
                    await _client.DisconnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _client?.Dispose();
                _client = null;
            }
        }

        public Task<bool> IsConnectedAsync()
        {
            return Task.FromResult(_client?.IsConnected ?? false);
        }

        public async Task<List<ZKTecoUser>> GetAllUsersAsync()
        {
            var users = new List<ZKTecoUser>();
            RequireConnection();

            var deviceUsers = await _client.GetUsersAsync();
            foreach (var u in deviceUsers)
            {
                users.Add(new ZKTecoUser
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Password = u.Password,
                    Role = u.Privilege,
                    CardNumber = u.Card > 0 ? u.Card.ToString() : null
                });
            }
            return users;
        }

        public async Task<bool> SyncUsersAsync(IEnumerable<ZKTecoUser> users)
        {
            RequireConnection();

            try
            {
                var existing = await _client.GetUsersAsync();
                ushort nextUid = existing.Count > 0 ? (ushort)(existing.Max(u => u.Uid) + 1) : (ushort)1;

                foreach (var user in users)
                {
                    var match = existing.FirstOrDefault(u => u.UserId == user.UserId);
                    var zkUser = new ZkClient.ZkUser
                    {
                        Uid = match?.Uid ?? nextUid,
                        UserId = user.UserId,
                        Name = string.IsNullOrEmpty(user.Name) ? ("NN-" + user.UserId) : user.Name,
                        Privilege = user.Role == 14 ? 14 : 0,
                        Password = user.Password ?? "",
                        Card = uint.TryParse(user.CardNumber, out var card) ? card : 0
                    };

                    if (!await _client.SetUserAsync(zkUser))
                        return false;

                    if (match == null) nextUid++;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            RequireConnection();

            try
            {
                return await _client.DeleteUserAsync(userId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ZKTecoAttendanceLog>> GetAttendanceLogsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var logs = new List<ZKTecoAttendanceLog>();
            RequireConnection();

            var deviceLogs = await _client.GetAttendanceAsync();
            foreach (var l in deviceLogs)
            {
                if (fromDate.HasValue && l.Timestamp < fromDate.Value) continue;
                if (toDate.HasValue && l.Timestamp > toDate.Value) continue;

                logs.Add(new ZKTecoAttendanceLog
                {
                    UserId = l.UserId,
                    DateTime = l.Timestamp,
                    // device semantics: Punch = verification type, Status = in/out state
                    VerifyMode = l.Punch,
                    InOutMode = l.Status,
                    WorkCode = 0
                });
            }
            return logs;
        }

        public async Task<ZKTecoDeviceInfo> GetDeviceInfoAsync()
        {
            var info = new ZKTecoDeviceInfo();
            RequireConnection();

            info.SerialNumber = await _client.GetSerialNumberAsync();
            info.FirmwareVersion = await _client.GetFirmwareVersionAsync();
            info.Platform = await _client.GetPlatformAsync();

            try
            {
                var sizes = await _client.ReadSizesAsync();
                info.UserCount = sizes.Users;
                info.FingerprintCount = sizes.Fingers;
                info.LogCount = sizes.Records;
                info.FaceCount = sizes.Faces;
            }
            catch
            {
                // counts are informational — don't fail device info when they are unavailable
            }

            return info;
        }

        public async Task<bool> ClearAttendanceLogsAsync()
        {
            RequireConnection();

            try
            {
                return await _client.ClearAttendanceLogsAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetDeviceTimeAsync(DateTime dateTime)
        {
            RequireConnection();

            try
            {
                return await _client.SetTimeAsync(dateTime);
            }
            catch
            {
                return false;
            }
        }

        private void RequireConnection()
        {
            if (_client == null || !_client.IsConnected)
                throw new InvalidOperationException("Not connected to the attendance machine. Call ConnectAsync first.");
        }

        private async Task DisconnectQuietlyAsync()
        {
            if (_client != null)
            {
                try { await _client.DisconnectAsync(); } catch { /* ignore */ }
                _client.Dispose();
                _client = null;
            }
        }

        /// <summary>
        /// ZK comm passwords are numeric; non-numeric values stored on the machine
        /// record (e.g. "admin") mean no comm password is configured.
        /// </summary>
        private static int ParseCommPassword(string password) =>
            int.TryParse(password, out var value) ? value : 0;
    }
}
