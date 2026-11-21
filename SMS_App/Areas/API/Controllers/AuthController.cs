using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Areas.API.Models;
using SMS_App.Utilities.EmailServices;
using SMS_App.Utilities.Jwt;
using SMS_App.Utilities.ShortMessageService;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers;

/// <summary>
/// Token authentication for the SchoolBD mobile app. Validates credentials with
/// the same Identity store the web app uses, then issues JWTs. Login/refresh are
/// anonymous; me/logout require a valid bearer token.
/// </summary>
[Area("API")]
[Route("API/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwt;
    private readonly IClaimsTransformation _claimsTransformation;
    private readonly IStudentManager _studentManager;
    private readonly IEmployeeManager _employeeManager;
    private readonly IInstituteManager _instituteManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwt,
        IClaimsTransformation claimsTransformation,
        IStudentManager studentManager,
        IEmployeeManager employeeManager,
        IInstituteManager instituteManager,
        IMemoryCache cache,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _claimsTransformation = claimsTransformation;
        _studentManager = studentManager;
        _employeeManager = employeeManager;
        _instituteManager = instituteManager;
        _cache = cache;
        _logger = logger;
    }

    // In-memory lifetime of a password-reset code. Short, because the code is a
    // 6-digit number delivered out of band.
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private const int MaxOtpAttempts = 5;

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(MobileApiRateLimits.Login)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.UserName) || string.IsNullOrWhiteSpace(dto?.Password))
            return BadRequest(new { message = "Username and password are required." });

        var user = await ResolveUserAsync(dto.UserName.Trim());
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
        {
            var message = check.IsLockedOut
                ? "This account is locked. Please try again later."
                : "Invalid username or password.";
            return Unauthorized(new { message });
        }

        return Ok(await BuildAuthResultAsync(user));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        var principal = _jwt.ValidateRefreshToken(dto.RefreshToken);
        if (principal == null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tokenStamp = principal.FindFirst(JwtTokenService.RefreshStampClaim)?.Value;

        var user = string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized(new { message = "Session is no longer valid." });

        // Security-stamp check = revocation on password change / stamp reset,
        // without a server-side token table.
        var currentStamp = await _userManager.GetSecurityStampAsync(user);
        if (!string.Equals(tokenStamp, currentStamp, StringComparison.Ordinal))
            return Unauthorized(new { message = "Session is no longer valid. Please sign in again." });

        return Ok(await BuildAuthResultAsync(user));
    }

    /// <summary>
    /// Step 1 of a self-service password reset: send a one-time code to the
    /// account's phone (<c>sms</c>) or email. The code and a matching Identity
    /// reset token are held server-side (memory cache, 10 min); the client never
    /// sees the token. Returns a masked destination so the UI can say where the
    /// code went. Email confirmation is NOT required — the code goes to whatever
    /// is already on file.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting(MobileApiRateLimits.Login)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.UserName))
            return BadRequest(new { message = "Username is required." });

        var channel = (dto.Channel ?? "sms").Trim().ToLowerInvariant();
        if (channel != "sms" && channel != "email")
            return BadRequest(new { message = "Channel must be 'sms' or 'email'." });

        var user = await ResolveUserAsync(dto.UserName.Trim());
        if (user == null)
            return NotFound(new { message = "No account found for that username." });

        var destination = channel == "sms" ? user.PhoneNumber : user.Email;
        if (string.IsNullOrWhiteSpace(destination))
            return BadRequest(new
            {
                message = channel == "sms"
                    ? "No phone number is on file for this account. Try email instead."
                    : "No email address is on file for this account. Try SMS instead.",
            });

        var otp = Random.Shared.Next(100000, 1000000).ToString();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var expires = DateTimeOffset.UtcNow.Add(OtpLifetime);
        _cache.Set(
            OtpCacheKey(user.Id),
            new PasswordResetEntry(HashOtp(otp), token, 0, expires),
            new MemoryCacheEntryOptions { AbsoluteExpiration = expires });

        var instituteName = (await _instituteManager.GetAllAsync())?.FirstOrDefault()?.Name;
        var text = $"You are trying to reset your password. Your OTP is: {otp}"
                   + (string.IsNullOrWhiteSpace(instituteName) ? string.Empty : $" -{instituteName}");

        // Never log the code at Information level; keep it to a warning that the
        // operator can mute, matching the web ForgotPassword flow.
        _logger.LogWarning("Mobile password-reset OTP issued for {User} via {Channel}.", user.UserName, channel);

        bool sent = channel == "sms"
            ? await MobileSMS.SendSMS(user.PhoneNumber, text)
            : EmailService.SendOTP(user.Email, "One time password", DateTime.Now, otp);

        if (!sent)
        {
            _cache.Remove(OtpCacheKey(user.Id));
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Could not send the verification code. Please try again or use another method.",
            });
        }

        return Ok(new
        {
            message = channel == "sms"
                ? "We sent a verification code by SMS."
                : "We sent a verification code by email.",
            channel,
            destination = MaskDestination(channel, destination),
            expiresInSeconds = (int)OtpLifetime.TotalSeconds,
        });
    }

    /// <summary>
    /// Step 2: verify the code from <see cref="ForgotPassword"/> and set the new
    /// password. The code is single-use and limited to a few attempts.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting(MobileApiRateLimits.Login)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.UserName)
            || string.IsNullOrWhiteSpace(dto?.Otp)
            || string.IsNullOrWhiteSpace(dto?.NewPassword))
            return BadRequest(new { message = "Username, code and new password are required." });

        var user = await ResolveUserAsync(dto.UserName.Trim());
        // Same generic response whether the user or the code is unknown, so this
        // endpoint can't be used to probe which usernames exist.
        if (user == null)
            return BadRequest(new { message = "Invalid or expired code. Please request a new one." });

        var key = OtpCacheKey(user.Id);
        if (!_cache.TryGetValue(key, out PasswordResetEntry entry) || entry == null)
            return BadRequest(new { message = "Your code has expired. Please request a new one." });

        if (entry.Attempts >= MaxOtpAttempts)
        {
            _cache.Remove(key);
            return BadRequest(new { message = "Too many attempts. Please request a new code." });
        }

        if (!FixedTimeEquals(entry.OtpHash, HashOtp(dto.Otp.Trim())))
        {
            _cache.Set(
                key,
                entry with { Attempts = entry.Attempts + 1 },
                new MemoryCacheEntryOptions { AbsoluteExpiration = entry.Expires });
            return BadRequest(new { message = "The code is incorrect. Please try again." });
        }

        var result = await _userManager.ResetPasswordAsync(user, entry.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        _cache.Remove(key);
        _logger.LogWarning("Mobile password reset completed for {User}.", user.UserName);
        return Ok(new { message = "Your password has been updated. Please sign in with your new password." });
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Me()
    {
        var userId = _userManager.GetUserId(User);
        var user = string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();
        if (roles.Count == 0)
            roles = (await _userManager.GetRolesAsync(user)).ToList();

        return Ok(await BuildProfileAsync(user, roles, ExtractPermissions(User.Claims)));
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Logout()
    {
        // Stateless tokens: the client discards them. A hard server-side revoke
        // would reset the security stamp (which also signs the user out of the
        // web app), so it is intentionally not done here.
        return Ok(new { message = "Signed out." });
    }

    /// <summary>
    /// Resolves the login identifier to a user the same flexible way the web
    /// login does: username/email, then a 7-digit student class roll, then a
    /// numeric student UniqueId (with/without leading zeros).
    /// </summary>
    private async Task<ApplicationUser> ResolveUserAsync(string identifier)
    {
        var user = await _userManager.FindByNameAsync(identifier)
                   ?? await _userManager.FindByEmailAsync(identifier);
        if (user != null) return user;

        try
        {
            if (identifier.Length == 7 && int.TryParse(identifier, out var roll))
            {
                var student = await _studentManager.GetStudentByClassRollAsync(roll);
                if (student?.UniqueId != null)
                    user = await _userManager.FindByNameAsync(student.UniqueId);
            }

            if (user == null && identifier.All(char.IsDigit))
            {
                var studentById = await _studentManager.GetStudentByUniqueIdAsync(identifier);
                if (studentById?.UniqueId != null)
                    user = await _userManager.FindByNameAsync(studentById.UniqueId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Student-identifier login fallback failed for {Identifier}.", identifier);
        }

        return user;
    }

    private async Task<AuthResultDto> BuildAuthResultAsync(ApplicationUser user)
    {
        var stampType = _userManager.Options.ClaimsIdentity.SecurityStampClaimType;
        var stamp = await _userManager.GetSecurityStampAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var baseClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(stampType, stamp),
        };
        baseClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var access = _jwt.CreateAccessToken(baseClaims);
        var refresh = _jwt.CreateRefreshToken(user.Id, stamp);

        return new AuthResultDto
        {
            Tokens = new TokensDto
            {
                AccessToken = access.Token,
                RefreshToken = refresh.Token,
                ExpiresAtUtc = access.ExpiresUtc,
            },
            User = await BuildProfileAsync(
                user,
                roles.ToList(),
                await ResolvePermissionsAsync(baseClaims)),
        };
    }

    private async Task<UserProfileDto> BuildProfileAsync(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var displayName = user.UserName;
        string phoneNumber = null;
        string designation = null;
        string photoUrl = null;

        if (user.UserType == 'e')
        {
            var employee = await _employeeManager.GetByIdAsync(user.ReferenceId);
            if (employee != null)
            {
                if (!string.IsNullOrWhiteSpace(employee.EmployeeName))
                    displayName = employee.EmployeeName;
                phoneNumber = employee.Phone;
                designation = employee.Designation?.DesignationName;
                photoUrl = EmployeePhotoUrl(employee.Image);
            }
        }

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = displayName,
            Email = user.Email,
            PhoneNumber = phoneNumber,
            Designation = designation,
            PhotoUrl = photoUrl,
            Roles = roles.ToList(),
            Permissions = permissions.ToList(),
        };
    }

    private static string EmployeePhotoUrl(string photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : "/Images/Employee/photo/" + photo;

    /// <summary>
    /// Runs the shared <see cref="IClaimsTransformation"/> over a principal built
    /// from the slim claims, so the login response carries the same permission
    /// set a request would see — without baking them into the token.
    /// </summary>
    private async Task<List<string>> ResolvePermissionsAsync(IEnumerable<Claim> baseClaims)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(baseClaims, "password"));
        principal = await _claimsTransformation.TransformAsync(principal);
        return ExtractPermissions(principal.Claims);
    }

    private List<string> ExtractPermissions(IEnumerable<Claim> claims)
    {
        var stampType = _userManager.Options.ClaimsIdentity.SecurityStampClaimType;
        var standard = new HashSet<string>(StringComparer.Ordinal)
        {
            ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role, stampType,
            JwtTokenService.TokenTypeClaim, JwtRegisteredClaimNames.Jti,
            JwtRegisteredClaimNames.Iss, JwtRegisteredClaimNames.Aud,
            JwtRegisteredClaimNames.Exp, JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Nbf,
            JwtRegisteredClaimNames.Sub,
        };
        return claims.Where(c => !standard.Contains(c.Type))
                     .Select(c => c.Type)
                     .Distinct()
                     .ToList();
    }

    // --- Password-reset helpers --------------------------------------------

    private static string OtpCacheKey(string userId) => $"mobile-pwdreset:{userId}";

    private static string HashOtp(string otp) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(a ?? string.Empty),
            Encoding.ASCII.GetBytes(b ?? string.Empty));

    /// <summary>Hides most of the phone/email so the UI can show where the code went.</summary>
    private static string MaskDestination(string channel, string destination)
    {
        if (string.IsNullOrWhiteSpace(destination)) return string.Empty;

        if (channel == "sms")
        {
            var digits = new string(destination.Where(char.IsDigit).ToArray());
            if (digits.Length <= 3) return new string('*', digits.Length);
            return new string('*', digits.Length - 3) + digits[^3..];
        }

        var at = destination.IndexOf('@');
        if (at <= 0) return destination;
        var name = destination[..at];
        var domain = destination[at..];
        var head = name.Length <= 1 ? name : name[0].ToString();
        return head + new string('*', Math.Max(1, name.Length - 1)) + domain;
    }

    /// <summary>Cached one-time-code state for a pending password reset.</summary>
    private sealed record PasswordResetEntry(
        string OtpHash,
        string Token,
        int Attempts,
        DateTimeOffset Expires);
}
