using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;

namespace SMS_App.Utilities.ShortMessageService
{
    /// <summary>
    /// Turns a raw machine punch into a guardian/employee SMS.
    ///
    /// This lives in the presentation layer because <see cref="MobileSMS"/> does,
    /// and SMS.BLL must not reference SMS_App. Both callers - the ADMS push
    /// endpoint that fires automatically, and the manual
    /// /api/API_Attendance/SendSMS endpoint - go through here so the decision
    /// rules exist in exactly one place.
    /// </summary>
    public interface IAttendanceSmsNotifier
    {
        Task<AttendanceSmsResult> NotifyAsync(Tran_MachineRawPunch punch);
        Task<AttendanceSmsResult> NotifyAsync(int punchId);
        Task NotifyBatchAsync(IReadOnlyCollection<Tran_MachineRawPunch> punches);
    }

    public class AttendanceSmsResult
    {
        public bool Sent { get; init; }
        public string Message { get; init; }

        public static AttendanceSmsResult Skip(string reason) => new() { Sent = false, Message = reason };
        public static AttendanceSmsResult Ok(string message) => new() { Sent = true, Message = message };
    }

    public class AttendanceSmsNotifier : IAttendanceSmsNotifier
    {
        // A terminal that has been offline replays its whole buffer at once.
        // Notifying on a replay would text every guardian at midnight for
        // punches from days ago, so a batch above this size is recorded but
        // stays silent. Realtime=1 means normal batches are a single punch.
        private const int MaxBatchToNotify = 10;

        private readonly IStudentManager _studentManager;
        private readonly IEmployeeManager _employeeManager;
        private readonly IInstituteManager _instituteManager;
        private readonly ISetupMobileSMSManager _setupMobileSMSManager;
        private readonly IPhoneSMSManager _phoneSMSManager;
        private readonly IAttendanceMachineManager _attendanceMachineManager;
        private readonly ILogger<AttendanceSmsNotifier> _logger;

        public AttendanceSmsNotifier(
            IStudentManager studentManager,
            IEmployeeManager employeeManager,
            IInstituteManager instituteManager,
            ISetupMobileSMSManager setupMobileSMSManager,
            IPhoneSMSManager phoneSMSManager,
            IAttendanceMachineManager attendanceMachineManager,
            ILogger<AttendanceSmsNotifier> logger)
        {
            _studentManager = studentManager;
            _employeeManager = employeeManager;
            _instituteManager = instituteManager;
            _setupMobileSMSManager = setupMobileSMSManager;
            _phoneSMSManager = phoneSMSManager;
            _attendanceMachineManager = attendanceMachineManager;
            _logger = logger;
        }

        public async Task<AttendanceSmsResult> NotifyAsync(int punchId)
        {
            var punch = await _attendanceMachineManager.GetByIdAsync(punchId);
            return punch == null
                ? AttendanceSmsResult.Skip("Attendance Not Found")
                : await NotifyAsync(punch);
        }

        public async Task NotifyBatchAsync(IReadOnlyCollection<Tran_MachineRawPunch> punches)
        {
            if (punches == null || punches.Count == 0)
                return;

            if (punches.Count > MaxBatchToNotify)
            {
                _logger.LogInformation(
                    "Skipping attendance SMS for a batch of {Count} punches - treated as an offline replay, not live traffic",
                    punches.Count);
                return;
            }

            foreach (var punch in punches)
            {
                try
                {
                    var result = await NotifyAsync(punch);
                    _logger.LogInformation(
                        "Attendance SMS for PIN {Pin} at {PunchTime}: {Outcome}",
                        punch.CardNo, punch.PunchDatetime, result.Message);
                }
                catch (Exception ex)
                {
                    // One bad punch must never cost the device its OK response.
                    _logger.LogError(ex, "Attendance SMS failed for PIN {Pin}", punch.CardNo);
                }
            }
        }

        public async Task<AttendanceSmsResult> NotifyAsync(Tran_MachineRawPunch punch)
        {
            if (punch == null)
                return AttendanceSmsResult.Skip("Attendance Not Found");

            var setup = await _setupMobileSMSManager.GetByIdAsync(1);
            if (setup == null || !setup.SMSService)
                return AttendanceSmsResult.Skip("SMS Service is turned off");

            if (!setup.AttendanceSMSService)
                return AttendanceSmsResult.Skip("Attendance SMS Service is turned off");

            var institute = await _instituteManager.GetByIdAsync(1);
            if (institute == null)
                return AttendanceSmsResult.Skip("Institute Information not found");

            // Messages only go out around the school day, with an hour of slack
            // either side, so an out-of-hours test punch cannot wake parents up.
            var duration = institute.ClosingTime - institute.StartingTime;
            var middleTime = TimeOnly.FromDateTime(institute.StartingTime).Add(duration / 2);
            var windowOpens = TimeOnly.FromDateTime(institute.StartingTime).AddHours(-1);
            var windowCloses = TimeOnly.FromDateTime(institute.ClosingTime).AddHours(1);

            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (now < windowOpens || now > windowCloses)
                return AttendanceSmsResult.Skip($"SMS sending time from {windowOpens} to {windowCloses}");

            if (punch.PunchDatetime.Date != DateTime.Today)
                return AttendanceSmsResult.Skip(
                    $"Punch is dated {punch.PunchDatetime:dd MMM yyyy}, today is {DateTime.Today:dd MMM yyyy}");

            var punchTime = TimeOnly.FromDateTime(punch.PunchDatetime);
            if (punchTime < windowOpens || punchTime > windowCloses)
                return AttendanceSmsResult.Skip("Information time is not valid");

            var attendanceType = punchTime > middleTime ? "CheckOut" : "CheckIn";
            var isCheckIn = attendanceType == "CheckIn";

            var (name, mobileNumber, allowed, reason) = await ResolveRecipientAsync(punch, setup, isCheckIn);
            if (!allowed)
                return AttendanceSmsResult.Skip(reason);

            if (string.IsNullOrWhiteSpace(mobileNumber))
                return AttendanceSmsResult.Skip($"{name} has no contact number on file");

            var alreadySent = await _phoneSMSManager.IsSMSSendForAttendance(
                mobileNumber, attendanceType, DateTime.Now.ToString("yyyyMMdd"));

            if (alreadySent)
                return AttendanceSmsResult.Skip($"{attendanceType} SMS already sent for this user");

            var instituteName = string.IsNullOrWhiteSpace(institute.ShortName)
                ? institute.Name?.Trim() ?? string.Empty
                : institute.ShortName.Trim();

            var text = isCheckIn
                ? AttendanceSmsText.CheckIn(name, punchTime.ToString(), instituteName)
                : AttendanceSmsText.CheckOut(name, punchTime.ToString(), instituteName);

            if (string.IsNullOrEmpty(text))
                return AttendanceSmsResult.Skip("Message could not be composed");

            var sent = await MobileSMS.SendSMS(mobileNumber, text);
            if (!sent)
                return AttendanceSmsResult.Skip("Service balance finished or SMS Service provider problem.");

            await _phoneSMSManager.AddAsync(new PhoneSMS
            {
                Text = text,
                MobileNumber = mobileNumber,
                SMSType = attendanceType,
                MACAddress = "System Generate",
                CreatedAt = DateTime.Now,
                CreatedBy = "Punch Machine"
            });

            return AttendanceSmsResult.Ok($"{attendanceType} SMS sent to {mobileNumber}");
        }

        /// <summary>
        /// Resolves the PIN the terminal reported to a person, then applies that
        /// audience's on/off switch.
        ///
        /// Lookup order matters: the current enrolment scheme is tried first and
        /// the legacy ones only answer for punches captured under the old
        /// numbering. Identity is never inferred from the length of the PIN -
        /// a student UniqueId and an employee's last-9 phone digits are both
        /// nine characters, so length cannot tell them apart.
        /// </summary>
        private async Task<(string Name, string Mobile, bool Allowed, string Reason)> ResolveRecipientAsync(
            Tran_MachineRawPunch punch, SetupMobileSMS setup, bool isCheckIn)
        {
            var pin = punch.CardNo?.Trim();
            if (string.IsNullOrEmpty(pin))
                return (null, null, false, "Punch has no PIN");

            // 1. Student by UniqueId - the scheme the terminal is enrolled with.
            var student = await _studentManager.GetStudentByUniqueIdAsync(pin);

            // 2. Employee by MachineUserId.
            Employee employee = null;
            if (student == null)
                employee = await _employeeManager.GetByMachineUserIdAsync(pin);

            // 3. Legacy: student by ClassRoll. TryParse, not Parse - a
            //    non-numeric PIN used to throw and surface as an HTTP 500.
            if (student == null && employee == null && int.TryParse(pin, out var classRoll))
                student = await _studentManager.GetStudentByClassRollAsync(classRoll);

            // 4. Legacy: employee by the last 9 digits of their phone number.
            if (student == null && employee == null)
                employee = await _employeeManager.GetByPhoneAttendance(pin);

            if (student != null)
            {
                var name = string.IsNullOrEmpty(student.NameBangla) ? student.Name : student.NameBangla;
                var isMale = student.GenderId == 1;

                var allowed = isMale
                    ? (isCheckIn ? setup.CheckInSMSServiceForMaleStudent : setup.CheckOutSMSServiceForMaleStudent)
                    : (isCheckIn ? setup.CheckInSMSServiceForGirlsStudent : setup.CheckOutSMSServiceForGirlsStudent);

                var audience = isMale ? "Boys" : "Girls";
                var stage = isCheckIn ? "checkin" : "checkout";

                return (name, student.GuardianPhone, allowed,
                    allowed ? null : $"{audience} student {stage} SMS Service is turned off");
            }

            if (employee != null)
            {
                var name = string.IsNullOrEmpty(employee.EmployeeNameBangla)
                    ? employee.EmployeeName
                    : employee.EmployeeNameBangla;

                var allowed = isCheckIn
                    ? setup.CheckInSMSServiceForEmployees
                    : setup.CheckOutSMSServiceForEmployees;

                var stage = isCheckIn ? "checkin" : "checkout";

                return (name, employee.Phone, allowed,
                    allowed ? null : $"Employees {stage} SMS Service is turned off");
            }

            return (null, null, false, $"No student or employee is enrolled with PIN '{pin}'");
        }

        // SMS wording lives in the shared AttendanceSmsText composer.
    }
}
