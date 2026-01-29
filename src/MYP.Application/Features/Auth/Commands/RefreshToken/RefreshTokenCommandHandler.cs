using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Auth.DTOs;

namespace MYP.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate refresh token and get user ID
        var userId = _jwtService.ValidateRefreshToken(request.RefreshToken);

        if (userId is null)
        {
            return Result.Failure<AuthResponseDto>("Invalid or expired refresh token.");
        }

        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthResponseDto>("User not found.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure<AuthResponseDto>("This account has been deactivated.");
        }

        // Revoke old refresh token
        _jwtService.RevokeRefreshToken(request.RefreshToken);

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = _jwtService.GetAccessTokenExpiration();

        // Store new refresh token
        _jwtService.StoreRefreshToken(refreshToken, user.Id);

        // Build response
        var userDto = new UserDto(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl
        );

        var response = new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: expiresAt,
            User: userDto
        );

        return Result.Success(response);
    }
}
