using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Auth.Commands.RefreshToken;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly RefreshTokenCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public RefreshTokenCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new RefreshTokenCommandHandler(_dbContext, _jwtService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.IsActive = true;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _jwtService.ValidateRefreshToken("valid_refresh_token").Returns(user.Id);
        _jwtService.GenerateAccessToken(user).Returns("new_access_token");
        _jwtService.GenerateRefreshToken().Returns("new_refresh_token");
        _jwtService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddHours(1));

        var command = new RefreshTokenCommand("valid_refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_refresh_token");
        result.Value.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        _jwtService.ValidateRefreshToken("invalid_token").Returns((Guid?)null);

        var command = new RefreshTokenCommand("invalid_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _jwtService.ValidateRefreshToken("valid_token").Returns(nonExistentUserId);

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.IsActive = false;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _jwtService.ValidateRefreshToken("valid_token").Returns(user.Id);

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("This account has been deactivated.");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUserData()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.Username = "testuser";
        user.DisplayName = "Test User";
        user.AvatarUrl = "https://example.com/avatar.png";
        user.IsActive = true;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _jwtService.ValidateRefreshToken("valid_token").Returns(user.Id);
        _jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwtService.GenerateRefreshToken().Returns("refresh_token");
        _jwtService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddHours(1));

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Email.Should().Be("test@example.com");
        result.Value.User.Username.Should().Be("testuser");
        result.Value.User.DisplayName.Should().Be("Test User");
        result.Value.User.AvatarUrl.Should().Be("https://example.com/avatar.png");
    }
}
