using MediatR;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Common.Models;
using MYP.Application.Features.Auth.DTOs;

namespace MYP.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthResponseDto>("Invalid email or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure<AuthResponseDto>("This account has been deactivated.");
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>("Invalid email or password.");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = _jwtService.GetAccessTokenExpiration();

        // Store refresh token
        _jwtService.StoreRefreshToken(refreshToken, user.Id);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

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
