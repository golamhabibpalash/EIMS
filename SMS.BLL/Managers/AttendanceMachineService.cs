using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AttendanceMachineService : IAttendanceMachineService
    {
        private readonly IAttendanceMachineDeviceManager _deviceManager;
        private readonly IAttendanceMachineManager _attendanceMachineManager;
        private readonly IStudentManager _studentManager;
        private readonly IEmployeeManager _employeeManager;
        private readonly IZKTecoService _zktecoService;

        public AttendanceMachineService(
            IAttendanceMachineDeviceManager deviceManager,
            IAttendanceMachineManager attendanceMachineManager,
            IStudentManager studentManager,
            IEmployeeManager employeeManager,
            IZKTecoService zktecoService)
        {
            _deviceManager = deviceManager;
            _attendanceMachineManager = attendanceMachineManager;
            _studentManager = studentManager;
            _employeeManager = employeeManager;
            _zktecoService = zktecoService;
        }

        public async Task<AttendanceMachine> GetByIdAsync(int id)
        {
            return await _deviceManager.GetByIdAsync(id);
        }

        public async Task<IEnumerable<AttendanceMachine>> GetAllAsync()
        {
            return await _deviceManager.GetAllAsync();
        }

        public async Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync()
        {
            return await _deviceManager.GetActiveMachinesAsync();
        }

        public async Task<bool> AddAsync(AttendanceMachine machine)
        {
            machine.CreatedAt = DateTime.Now;
            machine.EditedAt = DateTime.Now;
            return await _deviceManager.AddAsync(machine);
        }

        public async Task<bool> UpdateAsync(AttendanceMachine machine)
        {
            machine.EditedAt = DateTime.Now;
            return await _deviceManager.UpdateAsync(machine);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _deviceManager.RemoveAsync(await _deviceManager.GetByIdAsync(id));
        }

        public async Task<bool> TestConnectionAsync(int machineId)
        {
            var machine = await _deviceManager.GetByIdAsync(machineId);
            if (machine == null || !machine.IsActive)
                return false;

            // A push terminal is "reachable" when it has checked in with us
            // recently - we cannot dial it, so recent contact is the only
            // evidence available.
            if (machine.UsePushMode)
                return HasRecentContact(machine);

            var connected = await _zktecoService.ConnectAsync(machine.IPAddress, machine.Port, machine.Username, machine.Password);
            await _zktecoService.DisconnectAsync();
            return connected;
        }

        public async Task<MachineSyncResult> SyncUsersToMachineAsync(int machineId)
        {
            var result = new MachineSyncResult();
            var machine = await _deviceManager.GetByIdAsync(machineId);
            
            if (machine == null || !machine.IsActive)
            {
                result.Success = false;
                result.Message = "Machine not found or inactive";
                return result;
            }

            if (machine.UsePushMode)
            {
                // Nothing to dial: a push terminal has no listening port for us
                // to reach. Bail out immediately rather than burning the full
                // 30 second connect timeout on a call that cannot succeed.
                result.Success = false;
                result.Message = PushModeNotice("Users must be enrolled on the device itself");
                return result;
            }

            try
            {
                var connected = await _zktecoService.ConnectAsync(machine.IPAddress, machine.Port, machine.Username, machine.Password);
                if (!connected)
                {
                    result.Success = false;
                    result.Message = "Failed to connect to machine";
                    return result;
                }

                var users = new List<ZKTecoUser>();

                // Get students with MachineUserId
                var students = await _studentManager.GetAllAsync();
                foreach (var student in students.Where(s => s.Status && !string.IsNullOrEmpty(s.MachineUserId)))
                {
                    users.Add(new ZKTecoUser
                    {
                        UserId = student.MachineUserId,
                        Name = student.NameBangla ?? student.Name,
                        Role = 0, // Normal user
                        CardNumber = student.ClassRoll.ToString()
                    });
                }

                // Get employees with MachineUserId
                var employees = await _employeeManager.GetAllAsync();
                foreach (var employee in employees.Where(e => e.Status && !string.IsNullOrEmpty(e.MachineUserId)))
                {
                    users.Add(new ZKTecoUser
                    {
                        UserId = employee.MachineUserId,
                        Name = employee.EmployeeNameBangla ?? employee.EmployeeName,
                        Role = 0,
                        CardNumber = employee.Id.ToString()
                    });
                }

                result.TotalUsers = users.Count;

                if (users.Count > 0)
                {
                    var synced = await _zktecoService.SyncUsersAsync(users);
                    if (synced)
                    {
                        result.SyncedUsers = users.Count;
                        result.Success = true;
                        result.Message = $"Successfully synced {users.Count} users";
                    }
                    else
                    {
                        result.FailedUsers = users.Count;
                        result.Success = false;
                        result.Message = "Failed to sync users to machine";
                        result.Errors.Add("SyncUsersAsync returned false");
                    }
                }
                else
                {
                    result.Success = true;
                    result.Message = "No users to sync";
                }

                machine.LastSyncAt = DateTime.Now;
                machine.LastError = result.Success ? null : result.Message;
                await _deviceManager.UpdateAsync(machine);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Exception: {ex.Message}";
                result.Errors.Add(ex.ToString());
                
                var m = await _deviceManager.GetByIdAsync(machineId);
                if (m != null)
                {
                    m.LastError = result.Message;
                    await _deviceManager.UpdateAsync(m);
                }
            }
            finally
            {
                await _zktecoService.DisconnectAsync();
            }

            return result;
        }

        public async Task<MachinePullResult> PullAttendanceFromMachineAsync(int machineId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new MachinePullResult();
            var machine = await _deviceManager.GetByIdAsync(machineId);
            
            if (machine == null || !machine.IsActive)
            {
                result.Success = false;
                result.Message = "Machine not found or inactive";
                return result;
            }

            if (machine.UsePushMode)
            {
                result.Success = false;
                result.Message = PushModeNotice("Punches arrive on their own and there is nothing to pull");
                return result;
            }

            try
            {
                var connected = await _zktecoService.ConnectAsync(machine.IPAddress, machine.Port, machine.Username, machine.Password);
                if (!connected)
                {
                    result.Success = false;
                    result.Message = "Failed to connect to machine";
                    return result;
                }

                var logs = await _zktecoService.GetAttendanceLogsAsync(fromDate, toDate);
                result.RecordsPulled = logs.Count;

                if (logs.Count > 0)
                {
                    // one consistent machine tag for both storage and dedup comparison
                    var machineTag = string.IsNullOrEmpty(machine.SerialNumber)
                        ? machine.Id.ToString()
                        : machine.SerialNumber;

                    var existingPunches = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(
                        fromDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd"),
                        toDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd")
                    );

                    var existingKeys = existingPunches
                        .Select(p => $"{p.CardNo}_{p.PunchDatetime:yyyyMMddHHmmss}_{p.MachineNo}")
                        .ToHashSet();

                    var newPunches = new List<Tran_MachineRawPunch>();
                    var duplicateCount = 0;

                    foreach (var log in logs)
                    {
                        var key = $"{log.UserId}_{log.DateTime:yyyyMMddHHmmss}_{machineTag}";

                        if (existingKeys.Contains(key))
                        {
                            duplicateCount++;
                            continue;
                        }

                        // Determine if student or employee based on UserId format/length
                        // Students use ClassRoll (shorter), Employees use Id (longer)
                        // Or we can check MachineUserId mapping
                        var punch = new Tran_MachineRawPunch
                        {
                            CardNo = log.UserId,
                            PunchDatetime = log.DateTime,
                            P_Day = log.DateTime.DayOfWeek.ToString()[0],
                            ISManual = 'N',
                            MachineNo = machineTag,
                            VerifyMode = log.VerifyMode,
                            MachineSerialNo = machine.SerialNumber,
                            IsSynced = true
                        };

                        newPunches.Add(punch);
                        // register immediately so duplicates within the same batch are caught too
                        existingKeys.Add(key);
                    }

                    foreach (var punch in newPunches)
                    {
                        await _attendanceMachineManager.AddAsync(punch);
                    }

                    result.RecordsSaved = newPunches.Count;
                    result.DuplicateRecords = duplicateCount;
                    result.Success = true;
                    result.Message = $"Pulled {logs.Count} logs, saved {newPunches.Count}, {duplicateCount} duplicates";
                }
                else
                {
                    result.Success = true;
                    result.Message = "No new attendance logs found";
                }

                machine.LastSyncAt = DateTime.Now;
                machine.LastError = result.Success ? null : result.Message;
                await _deviceManager.UpdateAsync(machine);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Exception: {ex.Message}";
                result.Errors.Add(ex.ToString());
                
                var m = await _deviceManager.GetByIdAsync(machineId);
                if (m != null)
                {
                    m.LastError = result.Message;
                    await _deviceManager.UpdateAsync(m);
                }
            }
            finally
            {
                await _zktecoService.DisconnectAsync();
            }

            return result;
        }

        public async Task<MachineHealthStatus> GetMachineHealthAsync(int machineId)
        {
            var machine = await _deviceManager.GetByIdAsync(machineId);
            var status = new MachineHealthStatus();

            if (machine == null)
            {
                status.IsOnline = false;
                status.Error = "Machine not found";
                return status;
            }

            if (machine.UsePushMode)
            {
                // Report what the device last told us instead of opening a
                // socket. Without this the Details page blocks for the whole
                // connect timeout on every single load.
                status.IsOnline = HasRecentContact(machine);
                status.LastContact = machine.LastSyncAt;
                status.Error = machine.LastError;
                return status;
            }

            try
            {
                var connected = await _zktecoService.ConnectAsync(machine.IPAddress, machine.Port, machine.Username, machine.Password);
                
                if (connected)
                {
                    status.IsOnline = true;
                    status.LastContact = DateTime.Now;
                    
                    var info = await _zktecoService.GetDeviceInfoAsync();
                    status.FirmwareVersion = info.FirmwareVersion;
                    status.TotalUsers = info.UserCount;
                    status.TotalRecords = info.LogCount;

                    machine.LastSyncAt = DateTime.Now;
                    machine.LastError = null;
                }
                else
                {
                    status.IsOnline = false;
                    status.Error = "Connection failed";
                    machine.LastError = "Connection failed";
                }

                await _deviceManager.UpdateAsync(machine);
            }
            catch (Exception ex)
            {
                status.IsOnline = false;
                status.Error = ex.Message;
                machine.LastError = ex.Message;
                await _deviceManager.UpdateAsync(machine);
            }
            finally
            {
                await _zktecoService.DisconnectAsync();
            }

            return status;
        }

        /// <summary>
        /// A push terminal polls us on the Delay interval from its handshake
        /// (10s by default), so silence for this long means it has stopped
        /// talking to us - wrong server address, no Wi-Fi, or powered off.
        /// </summary>
        private const int PushContactGraceMinutes = 15;

        private static bool HasRecentContact(AttendanceMachine machine) =>
            machine.LastSyncAt.HasValue &&
            machine.LastSyncAt.Value > DateTime.Now.AddMinutes(-PushContactGraceMinutes);

        private static string PushModeNotice(string detail) =>
            $"This device is in push mode: it uploads to /iclock by itself. {detail}.";
    }
}