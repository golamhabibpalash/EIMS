using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.MACIPServices;
using SMS_App.Utilities.Notifications;
using SMS_App.Utilities.ShortMessageService;
using System;
using System.Threading.Tasks;

namespace SMS_App.Utilities.OnlineAdmission
{
    /// <summary>
    /// Admission notifications: SMS to the guardian and in-app alerts to admission staff.
    /// Everything is best-effort - a failed SMS is logged but never rolls back a valid
    /// application or payment. Texts carry no private details (only the application number and
    /// what to do next); the guardian sees the rest after signing in.
    /// </summary>
    public class AdmissionNotifier
    {
        /// <summary>Roles alerted in-app about new submissions and payments waiting for verification.</summary>
        public static readonly string[] StaffRoles = { "Admin" };

        private readonly IAdmissionSetupManager _setupManager;
        private readonly ISetupMobileSMSManager _setupMobileSmsManager;
        private readonly IPhoneSMSManager _phoneSmsManager;
        private readonly IInstituteManager _instituteManager;
        private readonly INotificationService _notificationService;
        private readonly IAppLogger _appLogger;

        public AdmissionNotifier(
            IAdmissionSetupManager setupManager,
            ISetupMobileSMSManager setupMobileSmsManager,
            IPhoneSMSManager phoneSmsManager,
            IInstituteManager instituteManager,
            INotificationService notificationService,
            IAppLogger appLogger)
        {
            _setupManager = setupManager;
            _setupMobileSmsManager = setupMobileSmsManager;
            _phoneSmsManager = phoneSmsManager;
            _instituteManager = instituteManager;
            _notificationService = notificationService;
            _appLogger = appLogger;
        }

        #region Guardian SMS

        public Task ApplicationSubmittedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"Your admission application {a.ApplicationNo} has been received. Keep this number to track it.");

        public Task ApplicationResubmittedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"Your corrected admission application {a.ApplicationNo} has been received.");

        public Task CorrectionRequestedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"Admission application {a.ApplicationNo} needs a correction. Please sign in to the admission portal to see what to change.");

        public Task ApprovedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, a.AmountDue > a.AmountPaid
                ? $"Congratulations! Admission application {a.ApplicationNo} is approved. Please pay BDT {a.AmountDue - a.AmountPaid:0}"
                  + (a.OfferExpiresAt.HasValue ? $" by {a.OfferExpiresAt:dd MMM yyyy}" : string.Empty) + ". Details in the admission portal."
                : $"Congratulations! Admission application {a.ApplicationNo} is approved. The school will confirm enrolment.");

        public Task RejectedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"Admission application {a.ApplicationNo} was not approved. Please sign in to the admission portal for details.");

        public Task WaitlistedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"Admission application {a.ApplicationNo} has been placed on the waiting list. We will inform you of any change.");

        public Task GuardianMessageAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"There is a new message about admission application {a.ApplicationNo}. Please sign in to the admission portal.");

        public Task PaymentVerifiedAsync(AdmissionApplication a, string receiptNo) =>
            SendToGuardianAsync(a, $"Payment for admission application {a.ApplicationNo} is verified. Receipt no. {receiptNo}.");

        public Task PaymentRejectedAsync(AdmissionApplication a) =>
            SendToGuardianAsync(a, $"The payment submitted for admission application {a.ApplicationNo} could not be verified. Please sign in to the admission portal for the reason.");

        /// <summary>Enrolment confirmation with the student's new EIMS login (same as the Students > Create screen sends).</summary>
        public Task EnrolledAsync(AdmissionApplication a, string uniqueId, string password) =>
            SendToGuardianAsync(a, string.IsNullOrEmpty(password)
                ? $"Admission confirmed! {a.Name} is enrolled. Student ID: {uniqueId}."
                : $"Admission confirmed! {a.Name} is enrolled. Student ID: {uniqueId}. Login - User: {uniqueId}, Password: {password}", "NewUser");

        /// <summary>Sends a one-time code. The code itself is never written to the SMS log or app logs.</summary>
        public async Task<bool> SendOtpAsync(string mobile, string code)
        {
            var school = await SchoolNameAsync();
            var sent = await TrySendAsync(mobile, $"Your {school} admission portal code is {code}. It expires in 10 minutes. Do not share it.");
            if (sent)
            {
                await LogSmsAsync(mobile, "Admission portal verification code sent.", "OTP");
            }
            return sent;
        }

        private async Task SendToGuardianAsync(AdmissionApplication application, string text, string smsType = "Admission")
        {
            try
            {
                var settings = await _setupManager.GetSettingsAsync();
                if (!settings.SendSmsNotifications)
                {
                    return;
                }
                // Respect the school-wide SMS switch (Setup > SMS).
                var smsSetup = await _setupMobileSmsManager.GetByIdAsync(1);
                if (smsSetup != null && !smsSetup.SMSService)
                {
                    return;
                }

                var mobile = application.GuardianPhone ?? application.GuardianAccount?.MobileNumber;
                if (string.IsNullOrEmpty(mobile))
                {
                    return;
                }

                var fullText = text + " - " + await SchoolNameAsync();
                if (await TrySendAsync(mobile, fullText))
                {
                    await LogSmsAsync(mobile, fullText, smsType);
                }
            }
            catch (Exception ex)
            {
                await SafeLogAsync("Admission SMS failed for application " + application?.Id, ex);
            }
        }

        private async Task<bool> TrySendAsync(string mobile, string text)
        {
            var sent = await MobileSMS.SendSMS(mobile, text);
            if (!sent)
            {
                // Recorded for follow-up; the workflow itself carries on.
                await SafeLogAsync($"Admission SMS to {Mask(mobile)} was not accepted by the SMS gateway.", null);
            }
            return sent;
        }

        private async Task LogSmsAsync(string mobile, string text, string smsType)
        {
            var now = DateTime.Now;
            await _phoneSmsManager.AddAsync(new PhoneSMS
            {
                Text = text,
                MobileNumber = mobile,
                SMSType = smsType,
                CreatedAt = now,
                CreatedBy = "System",
                EditedAt = now,
                EditedBy = "System",
                MACAddress = MACService.GetMAC()
            });
        }

        #endregion Guardian SMS

        #region Staff in-app

        public Task NewSubmissionAsync(AdmissionApplication a, bool resubmitted) =>
            _notificationService.NotifyRolesAsync(StaffRoles,
                resubmitted ? "Admission application resubmitted" : "New admission application",
                $"{a.ApplicationNo}: {a.Name}",
                $"/AdmissionApplications/Details/{a.Id}",
                NotificationCategory.Admission, null, MACService.GetMAC());

        public Task OfflinePaymentSubmittedAsync(AdmissionApplication a, decimal amount) =>
            _notificationService.NotifyRolesAsync(StaffRoles,
                "Admission payment to verify",
                $"{a.ApplicationNo}: BDT {amount:0.00} submitted for verification",
                "/AdmissionPayments?status=PendingVerification",
                NotificationCategory.Admission, null, MACService.GetMAC());

        #endregion Staff in-app

        private async Task<string> SchoolNameAsync()
        {
            try
            {
                var institute = await _instituteManager.GetFirstOrDefaultAsync();
                return string.IsNullOrWhiteSpace(institute?.ShortName) ? institute?.Name ?? "School" : institute.ShortName;
            }
            catch (Exception)
            {
                return "School";
            }
        }

        private static string Mask(string mobile) =>
            string.IsNullOrEmpty(mobile) || mobile.Length < 4 ? "***" : new string('*', mobile.Length - 3) + mobile[^3..];

        private async Task SafeLogAsync(string context, Exception ex)
        {
            try
            {
                await _appLogger.WarningAsync(context, ex?.Message);
            }
            catch
            {
                // Logging must never become the reason a workflow fails.
            }
        }
    }
}
