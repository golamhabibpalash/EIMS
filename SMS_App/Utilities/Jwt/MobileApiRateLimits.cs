namespace SMS_App.Utilities.Jwt;

/// <summary>Rate-limiter policy names for the mobile API (configured in Program.cs).</summary>
public static class MobileApiRateLimits
{
    /// <summary>
    /// <c>POST /API/Auth/login</c>: 20 per minute per IP. Caps password guessing and
    /// stops one client from cycling through student rolls to lock accounts out.
    /// </summary>
    public const string Login = "mobile-login";
}
