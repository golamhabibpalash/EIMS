using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Utilities.Notifications
{
    public interface IAttendancePushNotifier
    {
        Task NotifyAsync(Tran_MachineRawPunch punch);
        Task NotifyBatchAsync(IReadOnlyCollection<Tran_MachineRawPunch> punches);
    }

    public class AttendancePushNotifier : IAttendancePushNotifier
    {
        private const int MaxBatchToNotify = 10;

        private readonly IStudentManager _studentManager;
        private readonly IAttendanceMachineManager _attendanceMachineManager;
        private readonly IInstituteManager _instituteManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly IMobileDeviceTokenManager _deviceTokenManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFirebasePushSender _pushSender;
        private readonly SMS.BLL.MultiTenancy.ITenantContext _tenant;
        private readonly ILogger<AttendancePushNotifier> _logger;

        public AttendancePushNotifier(
            IStudentManager studentManager,
            IAttendanceMachineManager attendanceMachineManager,
            IInstituteManager instituteManager,
            IUserNotificationManager userNotificationManager,
            IMobileDeviceTokenManager deviceTokenManager,
            UserManager<ApplicationUser> userManager,
            IFirebasePushSender pushSender,
            SMS.BLL.MultiTenancy.ITenantContext tenant,
            ILogger<AttendancePushNotifier> logger)
        {
            _studentManager = studentManager;
            _attendanceMachineManager = attendanceMachineManager;
            _instituteManager = instituteManager;
            _userNotificationManager = userNotificationManager;
            _deviceTokenManager = deviceTokenManager;
            _userManager = userManager;
            _pushSender = pushSender;
            _tenant = tenant;
            _logger = logger;
        }

        public async Task NotifyBatchAsync(IReadOnlyCollection<Tran_MachineRawPunch> punches)
        {
            if (punches == null || punches.Count == 0)
            {
                return;
            }

            if (punches.Count > MaxBatchToNotify)
            {
                _logger.LogInformation(
                    "Skipping attendance push for a batch of {Count} punches - treated as an offline replay, not live traffic",
                    punches.Count);
                return;
            }

            List<Tran_MachineRawPunch> todaysPunches = null;
            if (punches.Any(p => p.PunchDatetime.Date == DateTime.Today))
            {
                todaysPunches = await _attendanceMachineManager.GetAllAttendanceByDateAsync(DateTime.Today);
            }

            foreach (var punch in punches)
            {
                try
                {
                    await NotifyCoreAsync(punch, todaysPunches);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attendance push failed for PIN {Pin}", punch.CardNo);
                }
            }
        }

        public async Task NotifyAsync(Tran_MachineRawPunch punch)
        {
            if (punch == null)
            {
                return;
            }

            try
            {
                await NotifyCoreAsync(punch, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attendance push failed for PIN {Pin}", punch.CardNo);
            }
        }

        private async Task NotifyCoreAsync(Tran_MachineRawPunch punch, List<Tran_MachineRawPunch> todaysPunches)
        {
            if (punch.PunchDatetime.Date != DateTime.Today)
            {
                return;
            }

            var pin = punch.CardNo?.Trim();
            if (string.IsNullOrWhiteSpace(pin))
            {
                return;
            }

            var student = await _studentManager.GetStudentByAttendancePinAsync(pin);
            if (student == null)
            {
                return;
            }

            todaysPunches ??= await _attendanceMachineManager.GetAllAttendanceByDateAsync(punch.PunchDatetime.Date);
            var firstPunch = todaysPunches
                .Where(p => AttendancePinMatcher.MatchesStudent(
                    p.CardNo, student.MachineUserId, student.UniqueId, student.ClassRoll))
                .OrderBy(p => p.PunchDatetime)
                .ThenBy(p => p.Tran_MachineRawPunchId)
                .FirstOrDefault();

            if (firstPunch == null || firstPunch.Tran_MachineRawPunchId != punch.Tran_MachineRawPunchId)
            {
                return;
            }

            var institute = await _instituteManager.GetByIdAsync(1);

            if (institute != null
                && institute.StartingTime > DateTime.MinValue
                && institute.ClosingTime > DateTime.MinValue)
            {
                var windowOpens = TimeOnly.FromDateTime(institute.StartingTime).AddHours(-1);
                var windowCloses = TimeOnly.FromDateTime(institute.ClosingTime).AddHours(1);
                var punchTime = TimeOnly.FromDateTime(punch.PunchDatetime);

                if (punchTime < windowOpens || punchTime > windowCloses)
                {
                    return;
                }
            }

            var user = await ResolveStudentUserAsync(student);
            if (user == null)
            {
                return;
            }

            var tokens = await _deviceTokenManager.GetActiveByUserIdAsync(user.Id);
            if (tokens.Count == 0)
            {
                return;
            }

            if (!PushSendGate.TryClaim(student.Id, punch.PunchDatetime.Date, _tenant.Current?.Key))
            {
                return;
            }

            var schoolName = string.IsNullOrWhiteSpace(institute?.ShortName)
                ? (string.IsNullOrWhiteSpace(institute?.Name) ? "Your school" : institute.Name.Trim())
                : institute.ShortName.Trim();

            var timeText = punch.PunchDatetime.ToString("hh:mm tt");
            var title = "Entered school";
            var body = $"{schoolName} recorded your entry at {timeText}.";

            await _userNotificationManager.NotifyAsync(
                new[] { user.Id },
                title,
                body,
                null,
                NotificationCategory.General,
                "Attendance",
                "System Generate");

            var data = new Dictionary<string, string>
            {
                ["date"] = punch.PunchDatetime.ToString("yyyy-MM-dd"),
                ["time"] = timeText,
                ["studentId"] = student.Id.ToString()
            };

            foreach (var device in tokens)
            {
                var result = await _pushSender.SendAsync(device.Token, title, body, "attendance", data);

                if (result.Status == PushSendStatus.InvalidToken)
                {
                    await _deviceTokenManager.UnregisterAsync(device.Token, user.Id);
                }
                else if (result.Status == PushSendStatus.Failed)
                {
                    _logger.LogWarning(
                        "Attendance push failed for student {StudentId} and device {DeviceId}: {Error}",
                        student.Id,
                        device.Id,
                        result.Error);
                }
            }
        }

        private async Task<ApplicationUser> ResolveStudentUserAsync(Student student)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserType == 's' && u.ReferenceId == student.Id);

            if (user != null)
            {
                return user;
            }

            if (int.TryParse(student.UniqueId, out var numericUniqueId))
            {
                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.UserType == 's' && u.ReferenceId == numericUniqueId);

                if (user != null)
                {
                    return user;
                }
            }

            if (!string.IsNullOrWhiteSpace(student.UniqueId))
            {
                user = await _userManager.FindByNameAsync(student.UniqueId);
                if (user != null && user.UserType == 's')
                {
                    return user;
                }
            }

            return null;
        }
    }
}
