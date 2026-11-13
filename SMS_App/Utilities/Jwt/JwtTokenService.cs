using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMS_App.Utilities.Jwt;

public record TokenResult(string Token, DateTime ExpiresUtc);

public interface IJwtTokenService
{
    TokenResult CreateAccessToken(IEnumerable<Claim> claims);
    TokenResult CreateRefreshToken(string userId, string securityStamp);

    /// <summary>Validates a refresh token; returns the principal, or null if invalid/expired/not a refresh token.</summary>
    ClaimsPrincipal ValidateRefreshToken(string token);
}

/// <summary>
/// Issues and validates the mobile API's JWTs. Access tokens stay deliberately
/// slim (identity + roles + stamp only) — permission claims are rehydrated per
/// request by <see cref="SMS_App.Configurations.PermissionClaimsTransformation"/>,
/// so the token never grows to the ~250-claim size that would overflow proxy
/// header limits (the same reason those claims are kept out of the web cookie).
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    public const string TokenTypeClaim = "token_type";
    public const string AccessTokenType = "access";
    public const string RefreshTokenType = "refresh";
    public const string RefreshStampClaim = "stamp";

    private readonly JwtSettings _settings;
    private readonly SMS.BLL.MultiTenancy.ITenantContext _tenant;

    public JwtTokenService(IOptions<JwtSettings> options, SMS.BLL.MultiTenancy.ITenantContext tenant)
    {
        _settings = options.Value;
        _tenant = tenant;
    }

    private SigningCredentials Credentials() =>
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)), SecurityAlgorithms.HmacSha256);

    public TokenResult CreateAccessToken(IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        var minutes = _settings.AccessTokenMinutes <= 0 ? 60 : _settings.AccessTokenMinutes;
        var expires = now.AddMinutes(minutes);

        var all = new List<Claim>(claims)
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(TokenTypeClaim, AccessTokenType),
            SMS_App.MultiTenancy.TenantClaims.For(_tenant),
        };

        var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, all, now, expires, Credentials());
        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public TokenResult CreateRefreshToken(string userId, string securityStamp)
    {
        var now = DateTime.UtcNow;
        var days = _settings.RefreshTokenDays <= 0 ? 30 : _settings.RefreshTokenDays;
        var expires = now.AddDays(days);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(RefreshStampClaim, securityStamp ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(TokenTypeClaim, RefreshTokenType),
            SMS_App.MultiTenancy.TenantClaims.For(_tenant),
        };

        var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims, now, expires, Credentials());
        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public ClaimsPrincipal ValidateRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear(); // keep claim types exactly as written

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        try
        {
            var principal = handler.ValidateToken(token, parameters, out _);
            var type = principal.FindFirst(TokenTypeClaim)?.Value;
            // A refresh token issued by another school is worthless here.
            return type == RefreshTokenType && SMS_App.MultiTenancy.TenantClaims.Matches(principal, _tenant) ? principal : null;
        }
        catch
        {
            return null;
        }
    }
}
