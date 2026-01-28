using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MYP.Application.Common.Interfaces;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Identity;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly Dictionary<string, (Guid UserId, DateTime Expiration)> _refreshTokens = new();

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("display_name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: GetAccessTokenExpiration(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        return refreshToken;
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
    }

    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
    }

    public Guid? ValidateRefreshToken(string refreshToken)
    {
        if (_refreshTokens.TryGetValue(refreshToken, out var tokenData))
        {
            if (tokenData.Expiration > DateTime.UtcNow)
            {
                return tokenData.UserId;
            }

            _refreshTokens.Remove(refreshToken);
        }

        return null;
    }

    public void StoreRefreshToken(string refreshToken, Guid userId)
    {
        CleanupExpiredTokens();

        _refreshTokens[refreshToken] = (userId, GetRefreshTokenExpiration());
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        _refreshTokens.Remove(refreshToken);
    }

    private void CleanupExpiredTokens()
    {
        var expiredTokens = _refreshTokens
            .Where(x => x.Value.Expiration <= DateTime.UtcNow)
            .Select(x => x.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            _refreshTokens.Remove(token);
        }
    }
}
