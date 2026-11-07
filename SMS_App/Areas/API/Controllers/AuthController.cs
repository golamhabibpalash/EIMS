using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Areas.API.Models;
using SMS_App.Utilities.Jwt;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwt,
        IClaimsTransformation claimsTransformation,
        IStudentManager studentManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _claimsTransformation = claimsTransformation;
        _studentManager = studentManager;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
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

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.UserName,
            Email = user.Email,
            Roles = roles,
            // Permissions were rehydrated onto User by PermissionClaimsTransformation
            // (invoked from the bearer OnTokenValidated event).
            Permissions = ExtractPermissions(User.Claims),
        });
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
            User = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                Permissions = await ResolvePermissionsAsync(baseClaims),
            },
        };
    }

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
}
