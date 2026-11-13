using Microsoft.AspNetCore.Mvc;
using SMS_App.Utilities.ShortMessageService;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace SMS_App.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class API_AttendanceController : ControllerBase
    {
        private readonly IAttendanceMachineManager _attendanceMachineManager;
        private readonly IAttendanceMachineService _attendanceMachineService;
        private readonly IAttendanceSmsNotifier _attendanceSmsNotifier;

        public API_AttendanceController(
            IAttendanceMachineManager attendanceMachineManager,
            IAttendanceMachineService attendanceMachineService,
            IAttendanceSmsNotifier attendanceSmsNotifier)
        {
            _attendanceMachineManager = attendanceMachineManager;
            _attendanceMachineService = attendanceMachineService;
            _attendanceSmsNotifier = attendanceSmsNotifier;
        }


        // GET: api/<API_AttendanceController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        public async Task<IActionResult> SendSMS(int Tran_MachineRawPunchId)
        {
            // The decision rules - service switches, the school-hours window,
            // resolving a PIN to a person, per-audience toggles and the
            // once-per-day guard - all live in the notifier, which the /iclock
            // push path uses too. Keeping them in one place is what stops the
            // two routes drifting apart.
            var result = await _attendanceSmsNotifier.NotifyAsync(Tran_MachineRawPunchId);
            return Ok(result.Message);
        }


        // POST: api/attendance/push - Receive punch data from fingerprint machine (ZKTeco ADMS push)
        [HttpPost("push")]
        public async Task<IActionResult> ReceivePunch([FromBody] MachinePunchDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.UserId) || dto.PunchTime == default)
            {
                return BadRequest(new { success = false, message = "Invalid payload" });
            }

            try
            {
                // Find machine by serial number
                var machines = await _attendanceMachineService.GetAllAsync();
                var machine = machines.FirstOrDefault(m => m.SerialNumber == dto.MachineSerialNo);
                
                if (machine == null)
                {
                    // Try to find by IP if serial not matched
                    var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    machine = machines.FirstOrDefault(m => m.IPAddress == clientIp);
                }

                if (machine == null)
                {
                    return BadRequest(new { success = false, message = "Machine not registered" });
                }

                // Check if already synced (deduplication)
                var existingPunches = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(
                    dto.PunchTime.AddDays(-1).ToString("yyyy-MM-dd"),
                    dto.PunchTime.ToString("yyyy-MM-dd")
                );

                var key = $"{dto.UserId}_{dto.PunchTime:yyyyMMddHHmmss}_{machine.SerialNumber ?? machine.Id.ToString()}";
                var exists = existingPunches.Any(p => $"{p.CardNo}_{p.PunchDatetime:yyyyMMddHHmmss}_{p.MachineNo}" == key);

                if (exists)
                {
                    return Ok(new { success = true, message = "Duplicate punch ignored", duplicate = true });
                }

                var punch = new Tran_MachineRawPunch
                {
                    CardNo = dto.UserId,
                    PunchDatetime = dto.PunchTime,
                    P_Day = dto.PunchTime.DayOfWeek.ToString()[0],
                    ISManual = 'N',
                    MachineNo = machine.SerialNumber ?? machine.Id.ToString(),
                    VerifyMode = dto.VerifyMode,
                    MachineSerialNo = machine.SerialNumber,
                    IsSynced = true
                };

                await _attendanceMachineManager.AddAsync(punch);

                machine.LastSyncAt = DateTime.Now;
                await _attendanceMachineService.UpdateAsync(machine);

                return Ok(new { success = true, message = "Punch recorded", punchId = punch.Tran_MachineRawPunchId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/attendance/sync-users/{machineId} - Push users to machine
        [HttpPost("sync-users/{machineId}")]
        public async Task<IActionResult> SyncUsers(int machineId)
        {
            try
            {
                var result = await _attendanceMachineService.SyncUsersToMachineAsync(machineId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/attendance/machine-status/{machineId} - Health check
        [HttpGet("machine-status/{machineId}")]
        public async Task<IActionResult> GetMachineStatus(int machineId)
        {
            try
            {
                var status = await _attendanceMachineService.GetMachineHealthAsync(machineId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/attendance/pull/{machineId} - Pull attendance from machine
        [HttpPost("pull/{machineId}")]
        public async Task<IActionResult> PullAttendance(int machineId, [FromBody] DateRangeDto dto)
        {
            try
            {
                var result = await _attendanceMachineService.PullAttendanceFromMachineAsync(machineId, dto?.FromDate, dto?.ToDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET api/<API_AttendanceController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<API_AttendanceController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<API_AttendanceController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<API_AttendanceController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public class MachinePunchDto
        {
            public string MachineSerialNo { get; set; }
            public string UserId { get; set; }
            public DateTime PunchTime { get; set; }
            public int VerifyMode { get; set; }
            public int InOutMode { get; set; }
        }

        public class DateRangeDto
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
    }
}
