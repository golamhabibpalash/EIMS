using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS_App.Utilities.ShortMessageService;

namespace SMS_App.Controllers
{
    /// <summary>
    /// ADMS / "push SDK" endpoints for ZKTeco attendance terminals.
    ///
    /// The device firmware has these paths burned in - /iclock/cdata,
    /// /iclock/getrequest, /iclock/devicecmd - so the routes below cannot be
    /// renamed or moved under an area. The terminal cannot authenticate, which
    /// is why the controller is [AllowAnonymous]; the only identity it presents
    /// is the SN query parameter, so every action refuses to do anything until
    /// that serial matches a device row somebody deliberately registered.
    ///
    /// All responses are plain text. The firmware parses them literally and
    /// will drop its buffer or retry forever if the wording is wrong.
    /// </summary>
    [AllowAnonymous]
    [Route("iclock")]
    public class IClockController : Controller
    {
        private readonly IAdmsPushManager _admsPushManager;
        private readonly IAttendanceSmsNotifier _smsNotifier;
        private readonly ILogger<IClockController> _logger;

        public IClockController(
            IAdmsPushManager admsPushManager,
            IAttendanceSmsNotifier smsNotifier,
            ILogger<IClockController> logger)
        {
            _admsPushManager = admsPushManager;
            _smsNotifier = smsNotifier;
            _logger = logger;
        }

        /// <summary>
        /// Handshake. The terminal calls this on boot and after any error, and
        /// configures itself from the option block it gets back.
        /// GET /iclock/cdata?SN=...&amp;options=all&amp;pushver=...
        /// </summary>
        [HttpGet("cdata")]
        public async Task<IActionResult> Handshake([FromQuery(Name = "SN")] string sn)
        {
            var machine = await _admsPushManager.GetRegisteredDeviceAsync(sn);
            if (machine == null)
                return UnregisteredDevice(sn);

            await _admsPushManager.RecordContactAsync(machine);

            _logger.LogInformation("ADMS handshake from {Serial} ({Name})", sn, machine.Name);
            return Text(_admsPushManager.BuildDeviceOptions(machine));
        }

        /// <summary>
        /// Data upload. Punches arrive as table=ATTLOG with a tab-delimited
        /// body; other tables (OPERLOG, options, ...) are acknowledged and
        /// discarded so the device does not retry them in a loop.
        /// POST /iclock/cdata?SN=...&amp;table=ATTLOG
        /// </summary>
        [HttpPost("cdata")]
        public async Task<IActionResult> Upload(
            [FromQuery(Name = "SN")] string sn,
            [FromQuery(Name = "table")] string table)
        {
            var machine = await _admsPushManager.GetRegisteredDeviceAsync(sn);
            if (machine == null)
                return UnregisteredDevice(sn);

            var payload = await ReadBodyAsync();

            if (!string.Equals(table, "ATTLOG", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("ADMS {Table} upload from {Serial} acknowledged and ignored", table, sn);
                return Text("OK");
            }

            try
            {
                var result = await _admsPushManager.SaveAttLogAsync(machine, payload);
                await _admsPushManager.RecordContactAsync(machine);

                _logger.LogInformation(
                    "ADMS ATTLOG from {Serial}: {Received} received, {Saved} saved, {Duplicates} duplicate, {Invalid} invalid",
                    sn, result.Received, result.Saved, result.Duplicates, result.Invalid);

                foreach (var error in result.Errors)
                    _logger.LogWarning("ADMS ATTLOG from {Serial}: {Error}", sn, error);

                // Notify on the punches this call actually stored. Wrapped
                // because the punches are already safely saved by now - an SMS
                // provider outage must not turn into a 500 that makes the
                // device replay a batch we have.
                try
                {
                    await _smsNotifier.NotifyBatchAsync(result.SavedPunches);
                }
                catch (Exception smsEx)
                {
                    _logger.LogError(smsEx, "Attendance SMS failed for ATTLOG from {Serial}", sn);
                }

                // The firmware clears its buffer on OK. Report the number we
                // accepted so a mismatch is visible in the device log.
                return Text($"OK: {result.Saved}");
            }
            catch (Exception ex)
            {
                // Deliberately NOT returning OK - the device keeps the batch and
                // resends it, so a database outage costs nothing but a delay.
                _logger.LogError(ex, "ADMS ATTLOG from {Serial} failed; asking device to retry", sn);
                await _admsPushManager.RecordContactAsync(machine, ex.Message);
                return StatusCode(500, "ERROR");
            }
        }

        /// <summary>
        /// Command poll. The terminal asks for work on the Delay interval from
        /// the handshake. We push nothing today, so it always gets an empty OK.
        /// Deliberately does not touch LastSyncAt: this fires every few seconds
        /// and would otherwise write to AttendanceMachines continuously.
        /// GET /iclock/getrequest?SN=...
        /// </summary>
        [HttpGet("getrequest")]
        public async Task<IActionResult> GetRequest([FromQuery(Name = "SN")] string sn)
        {
            var machine = await _admsPushManager.GetRegisteredDeviceAsync(sn);
            if (machine == null)
                return UnregisteredDevice(sn);

            return Text("OK");
        }

        /// <summary>
        /// Command acknowledgement, sent after the device executes something we
        /// handed it via getrequest. Nothing is queued yet, so this only needs
        /// to answer politely.
        /// POST /iclock/devicecmd?SN=...
        /// </summary>
        [HttpPost("devicecmd")]
        public async Task<IActionResult> DeviceCmd([FromQuery(Name = "SN")] string sn)
        {
            var machine = await _admsPushManager.GetRegisteredDeviceAsync(sn);
            if (machine == null)
                return UnregisteredDevice(sn);

            var payload = await ReadBodyAsync();
            _logger.LogInformation("ADMS devicecmd result from {Serial}: {Payload}", sn, payload);

            return Text("OK");
        }

        /// <summary>Connectivity probe used by some firmware revisions.</summary>
        [HttpGet("ping")]
        public IActionResult Ping() => Text("OK");

        private async Task<string> ReadBodyAsync()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        private IActionResult UnregisteredDevice(string sn)
        {
            // Logged at warning because on a public endpoint this is either a
            // device nobody registered yet or somebody probing the route.
            _logger.LogWarning(
                "ADMS request from unregistered serial '{Serial}' (remote {RemoteIp}) rejected",
                sn, HttpContext.Connection.RemoteIpAddress);

            return Unauthorized("Device not registered");
        }

        private IActionResult Text(string body) => Content(body, "text/plain", Encoding.UTF8);
    }
}
