using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Auth.Queries.GetCurrentUser;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetCurrentUserQueryHandler _handler;
    private readonly UserFaker _userFaker;

    public GetCurrentUserQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetCurrentUserQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_ShouldReturnUserDto()
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

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.Username.Should().Be("testuser");
        result.Value.DisplayName.Should().Be("Test User");
        result.Value.AvatarUrl.Should().Be("https://example.com/avatar.png");
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);
        _currentUserService.UserId.Returns((Guid?)null);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithNullUserId_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((Guid?)null);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(nonExistentUserId);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("This account has been deactivated.");
    }
}
