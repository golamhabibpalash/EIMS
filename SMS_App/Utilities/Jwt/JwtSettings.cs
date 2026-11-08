namespace SMS_App.Utilities.Jwt;

/// <summary>
/// JWT options bound from the "Jwt" section of appsettings.json. Each deployed
/// instance sets its own <see cref="Key"/> (kept out of source control like the
/// connection string). The key must be at least 32 characters for HMAC-SHA256.
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "EIMS";
    public string Audience { get; set; } = "EIMS.Mobile";

    /// <summary>Access-token lifetime in minutes (default 60).</summary>
    public int AccessTokenMinutes { get; set; } = 60;

    /// <summary>Refresh-token lifetime in days (default 30).</summary>
    public int RefreshTokenDays { get; set; } = 30;
}
