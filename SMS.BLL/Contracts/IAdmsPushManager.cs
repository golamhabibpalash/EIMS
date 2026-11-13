using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    /// <summary>
    /// ADMS ("push SDK" / iclock) protocol support.
    ///
    /// Push devices such as the ZKTeco SenseFace T1 have no reachable listening
    /// port of their own - they dial OUT to the server and upload punches over
    /// plain HTTP. That is the only workable transport when the application is
    /// hosted off-site and the terminal sits behind the institute's router.
    /// The pull path (see IZKTecoService) requires a route to the device on
    /// port 4370 and cannot be used from a remote deployment.
    /// </summary>
    public interface IAdmsPushManager
    {
        /// <summary>Looks up a registered device by the serial number it reports in SN=.</summary>
        Task<AttendanceMachine> GetRegisteredDeviceAsync(string serialNumber);

        /// <summary>Builds the plain-text option block returned from the device handshake.</summary>
        string BuildDeviceOptions(AttendanceMachine machine);

        /// <summary>Parses a tab-delimited ATTLOG payload and stores the punches it contains.</summary>
        Task<AdmsAttLogResult> SaveAttLogAsync(AttendanceMachine machine, string payload);

        /// <summary>Stamps LastSyncAt/LastError so the device list reflects real contact.</summary>
        Task RecordContactAsync(AttendanceMachine machine, string error = null);
    }

    public class AdmsAttLogResult
    {
        public int Received { get; set; }
        public int Saved { get; set; }
        public int Duplicates { get; set; }
        public int Invalid { get; set; }
        public List<string> Errors { get; } = new();

        /// <summary>
        /// The punches actually written on this call - duplicates excluded, so
        /// a device re-sending its buffer cannot trigger a second notification
        /// for a punch somebody was already told about.
        /// </summary>
        public List<Tran_MachineRawPunch> SavedPunches { get; } = new();
    }
}
