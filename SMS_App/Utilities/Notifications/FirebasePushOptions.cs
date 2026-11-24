namespace SMS_App.Utilities.Notifications
{
    public class FirebasePushOptions
    {
        public bool Enabled { get; set; }
        public string ProjectId { get; set; }
        public string ServiceAccountJson { get; set; }
        public string ServiceAccountJsonBase64 { get; set; }
        public string ServiceAccountPath { get; set; }
    }
}
