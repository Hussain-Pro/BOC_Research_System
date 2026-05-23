using System;
using System.Security.Claims;
using BOC.Domain.Entities;

namespace BOC.Application.Common.Interfaces;

public class JwtTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateTokens(AppUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
