namespace SMS_App.Areas.API.Models;

/// <summary>
/// Institute branding for the mobile app: the signed-in instance's display name
/// and logo, shown in the app header after login. Both fields may be null; the
/// client falls back to the bundled SchoolBD name/logo per field.
/// </summary>
public class InstituteProfileDto
{
    public string Name { get; set; }
    public string ShortName { get; set; }

    /// <summary>
    /// Relative path (e.g. "/Images/Institute/x.png"); the client prefixes the
    /// instance base URL. Null when the institute has no logo.
    /// </summary>
    public string LogoUrl { get; set; }
}
