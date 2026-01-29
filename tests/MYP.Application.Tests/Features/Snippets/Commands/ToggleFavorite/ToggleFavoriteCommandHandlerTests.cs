using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Commands.ToggleFavorite;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Commands.ToggleFavorite;

public class ToggleFavoriteCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ToggleFavoriteCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly SnippetFaker _snippetFaker;

    public ToggleFavoriteCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new ToggleFavoriteCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _snippetFaker = new SnippetFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithNotFavoriteSnippet_ShouldSetToFavorite()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.IsFavorite = false;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(snippet.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        snippet.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithFavoriteSnippet_ShouldSetToNotFavorite()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.IsFavorite = true;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(snippet.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
        snippet.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new ToggleFavoriteCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenSnippetNotFound_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Snippet not found.");
    }

    [Fact]
    public async Task Handle_WhenSnippetBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var snippet = _snippetFaker.WithUserId(otherUser.Id).Generate();
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(snippet.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Snippet not found.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.IsFavorite = false;
        snippet.UpdatedAt = null;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(snippet.Id);
        var beforeToggle = DateTime.UtcNow;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        snippet.UpdatedAt.Should().NotBeNull();
        snippet.UpdatedAt.Should().BeOnOrAfter(beforeToggle);
    }

    [Fact]
    public async Task Handle_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.IsFavorite = false;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ToggleFavoriteCommand(snippet.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedSnippet = await _dbContext.Snippets.FindAsync(snippet.Id);
        savedSnippet!.IsFavorite.Should().BeTrue();
    }
}
