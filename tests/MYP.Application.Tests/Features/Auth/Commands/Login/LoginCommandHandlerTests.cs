using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Auth.Commands.Login;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly LoginCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public LoginCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LoginCommandHandler(_dbContext, _passwordHasher, _jwtService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithTokens()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.PasswordHash = "hashed_password";
        user.IsActive = true;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.Verify("Password1!", user.PasswordHash).Returns(true);
        _jwtService.GenerateAccessToken(user).Returns("access_token");
        _jwtService.GenerateRefreshToken().Returns("refresh_token");
        _jwtService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddHours(1));

        var command = new LoginCommand("test@example.com", "Password1!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.User.Email.Should().Be(user.Email);
        result.Value.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password1!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.IsActive = false;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("test@example.com", "Password1!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("This account has been deactivated.");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.PasswordHash = "hashed_password";
        user.IsActive = true;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.Verify("wrong_password", user.PasswordHash).Returns(false);

        var command = new LoginCommand("test@example.com", "wrong_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.PasswordHash = "hashed_password";
        user.IsActive = true;
        user.LastLoginAt = null;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.Verify("Password1!", user.PasswordHash).Returns(true);
        _jwtService.GenerateAccessToken(user).Returns("access_token");
        _jwtService.GenerateRefreshToken().Returns("refresh_token");
        _jwtService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddHours(1));

        var command = new LoginCommand("test@example.com", "Password1!");
        var beforeLogin = DateTime.UtcNow;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.Com")]
    [InlineData("test@example.com")]
    public async Task Handle_WithDifferentEmailCasing_ShouldFindUser(string email)
    {
        // Arrange
        var user = _userFaker.Generate();
        user.Email = "test@example.com";
        user.PasswordHash = "hashed_password";
        user.IsActive = true;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.Verify("Password1!", user.PasswordHash).Returns(true);
        _jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwtService.GenerateRefreshToken().Returns("refresh_token");
        _jwtService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddHours(1));

        var command = new LoginCommand(email, "Password1!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
