namespace SMS_App.Utilities.OnlineAdmission
{
    /// <summary>Rate-limiter policy names (configured in Program.cs).</summary>
    public static class AdmissionRateLimits
    {
        /// <summary>Sign-in, registration, one-time codes and public tracking: 10 per minute per IP.</summary>
        public const string Auth = "admission-auth";

        /// <summary>File uploads: 30 per minute per IP.</summary>
        public const string Upload = "admission-upload";
    }
}
