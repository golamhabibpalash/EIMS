using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Implements the server half of the ZKTeco ADMS push protocol.
    /// The wire format is line-oriented plain text, not JSON - every response
    /// body here is consumed by device firmware, so the exact wording and the
    /// LF line endings matter.
    /// </summary>
    public class AdmsPushManager : IAdmsPushManager
    {
        // The terminal timestamps punches in its own local time and sends them
        // as naked "yyyy-MM-dd HH:mm:ss" strings with no offset. Bangladesh is
        // UTC+6; this is echoed back in the handshake so the device and the
        // server agree on what those strings mean.
        private const int DeviceTimeZone = 6;

        // How long the device waits before retrying after an error, and how
        // long it sleeps between command polls. Seconds.
        private const int ErrorDelaySeconds = 30;
        private const int PollDelaySeconds = 10;

        private readonly IAttendanceMachineDeviceManager _deviceManager;
        private readonly IAttendanceMachineManager _attendanceMachineManager;

        public AdmsPushManager(
            IAttendanceMachineDeviceManager deviceManager,
            IAttendanceMachineManager attendanceMachineManager)
        {
            _deviceManager = deviceManager;
            _attendanceMachineManager = attendanceMachineManager;
        }

        public async Task<AttendanceMachine> GetRegisteredDeviceAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return null;

            return await _deviceManager.GetBySerialNumberAsync(serialNumber.Trim());
        }

        public string BuildDeviceOptions(AttendanceMachine machine)
        {
            // Realtime=1 tells the terminal to upload each punch as it happens
            // instead of batching until TransTimes. TransFlag enables every
            // upload table the firmware supports; we ignore the ones we do not
            // consume. Encrypt=0 keeps the payload readable.
            var lines = new[]
            {
                $"GET OPTION FROM: {machine.SerialNumber}",
                "Stamp=9999",
                "OpStamp=9999",
                $"ErrorDelay={ErrorDelaySeconds}",
                $"Delay={PollDelaySeconds}",
                "TransTimes=00:00;14:05",
                "TransInterval=1",
                "TransFlag=1111111111",
                $"TimeZone={DeviceTimeZone}",
                "Realtime=1",
                "Encrypt=0"
            };

            return string.Join("\n", lines) + "\n";
        }

        public async Task<AdmsAttLogResult> SaveAttLogAsync(AttendanceMachine machine, string payload)
        {
            var result = new AdmsAttLogResult();

            if (string.IsNullOrWhiteSpace(payload))
                return result;

            var machineTag = string.IsNullOrEmpty(machine.SerialNumber)
                ? machine.Id.ToString()
                : machine.SerialNumber;

            var parsed = new List<Tran_MachineRawPunch>();

            foreach (var line in payload.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                result.Received++;

                var punch = ParseAttLogLine(line, machineTag, machine.SerialNumber);
                if (punch == null)
                {
                    result.Invalid++;
                    result.Errors.Add($"Unparseable ATTLOG line: {line}");
                    continue;
                }

                parsed.Add(punch);
            }

            if (parsed.Count == 0)
                return result;

            // Only the dates actually present in this batch need checking, which
            // keeps the duplicate lookup small even on a bulk re-upload.
            var from = parsed.Min(p => p.PunchDatetime).Date;
            var to = parsed.Max(p => p.PunchDatetime).Date;

            var existing = await _attendanceMachineManager.GetAttendanceByDateRangeAsync(
                from.ToString("yyyy-MM-dd"),
                to.ToString("yyyy-MM-dd"));

            var seen = existing
                .Select(BuildKey)
                .ToHashSet();

            var toSave = new List<Tran_MachineRawPunch>();

            foreach (var punch in parsed)
            {
                // A terminal re-sends its whole buffer whenever it is unsure the
                // server got it, so repeats are normal traffic rather than an
                // error. Adding to the set as we go also catches repeats inside
                // this single payload.
                if (!seen.Add(BuildKey(punch)))
                {
                    result.Duplicates++;
                    continue;
                }

                toSave.Add(punch);
            }

            if (toSave.Count > 0)
            {
                await _attendanceMachineManager.AddRangeAsync(toSave);
                result.Saved = toSave.Count;
                result.SavedPunches.AddRange(toSave);
            }

            return result;
        }

        public async Task RecordContactAsync(AttendanceMachine machine, string error = null)
        {
            machine.LastSyncAt = DateTime.Now;
            machine.LastError = error;
            machine.EditedAt = DateTime.Now;
            await _deviceManager.UpdateAsync(machine);
        }

        /// <summary>
        /// ATTLOG lines are tab-delimited:
        ///   PIN \t yyyy-MM-dd HH:mm:ss \t Status \t VerifyMode \t WorkCode \t Reserved...
        /// Only the first two fields are required; firmware revisions differ in
        /// how many trailing fields they send, so everything after VerifyMode is
        /// read defensively.
        /// </summary>
        private static Tran_MachineRawPunch ParseAttLogLine(string line, string machineTag, string serialNumber)
        {
            var fields = line.Split('\t');
            if (fields.Length < 2)
                return null;

            var pin = fields[0].Trim();
            if (string.IsNullOrEmpty(pin))
                return null;

            if (!DateTime.TryParseExact(
                    fields[1].Trim(),
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var punchedAt))
            {
                return null;
            }

            int? verifyMode = null;
            if (fields.Length > 3 && int.TryParse(fields[3].Trim(), out var verify))
                verifyMode = verify;

            return new Tran_MachineRawPunch
            {
                CardNo = pin,
                PunchDatetime = punchedAt,
                P_Day = punchedAt.DayOfWeek.ToString()[0],
                ISManual = 'N',
                MachineNo = machineTag,
                VerifyMode = verifyMode,
                MachineSerialNo = serialNumber,
                IsSynced = true
            };
        }

        private static string BuildKey(Tran_MachineRawPunch punch) =>
            $"{punch.CardNo}_{punch.PunchDatetime:yyyyMMddHHmmss}_{punch.MachineNo}";
    }
}
