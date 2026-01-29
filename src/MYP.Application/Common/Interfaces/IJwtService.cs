using MYP.Domain.Entities;

namespace MYP.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiration();
    DateTime GetRefreshTokenExpiration();
    Guid? ValidateRefreshToken(string refreshToken);
    void StoreRefreshToken(string refreshToken, Guid userId);
    void RevokeRefreshToken(string refreshToken);
}
