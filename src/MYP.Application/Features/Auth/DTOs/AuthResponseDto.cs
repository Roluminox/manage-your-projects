namespace MYP.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string DisplayName,
    string? AvatarUrl
);
