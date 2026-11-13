using System;

namespace SMS_App.ViewModels.SetupVM
{
    public class AttendanceSetupVM
    {
        public int Id { get; set; }
        public bool SMSService { get; set; }

        #region Attendance SMS Service
        public bool AttendanceSMSService { get; set; }

        public bool CheckInSMSService { get; set; }
        public bool CheckInSMSServiceForMaleStudent { get; set; }
        public bool CheckInSMSServiceForGirlsStudent { get; set; }
        public bool CheckInSMSServiceForEmployees { get; set; }
        public bool CheckInSMSSummary { get; set; }
        public bool CheckOutSMSService { get; set; }
        public bool CheckOutSMSServiceForMaleStudent { get; set; }
        public bool CheckOutSMSServiceForGirlsStudent { get; set; }
        public bool CheckOutSMSServiceForEmployees { get; set; }
        #endregion

        #region Payment SMS Service
        public bool PaymentSMSService { get; set; }
        public bool DailyCollectionSMSService { get; set; }
        #endregion

        #region Administration SMS Service
        public bool AdministrativeSMSService { get; set; }
        #endregion

        #region Absent Notification SMS
        public bool AbsentNotification { get; set; }
        public bool AbsentNotificationStudent { get; set; }
        public bool AbsentNotificationEmployee { get; set; }
        #endregion
        public string EditedBy { get; set; }
        public DateTime EditedAt { get; set; }
    }
}
