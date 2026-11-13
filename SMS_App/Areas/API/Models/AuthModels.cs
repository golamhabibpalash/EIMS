using System;
using System.Collections.Generic;

namespace SMS_App.Areas.API.Models;

/// <summary>Credentials posted by the mobile app to <c>POST /API/Auth/login</c>.</summary>
public class LoginRequestDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

/// <summary>Body of <c>POST /API/Auth/refresh</c>.</summary>
public class RefreshRequestDto
{
    public string RefreshToken { get; set; }
}

public class TokensDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

/// <summary>
/// Lean user projection for the mobile client — NOT a Razor ViewModel.
/// <see cref="Permissions"/> carries the same claim strings EIMS uses
/// server-side (e.g. "View Academic Class") so the app can gate UI, while the
/// server remains the real authority.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class AuthResultDto
{
    public TokensDto Tokens { get; set; }
    public UserProfileDto User { get; set; }
}
