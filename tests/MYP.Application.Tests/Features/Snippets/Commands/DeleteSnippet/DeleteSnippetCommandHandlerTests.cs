using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Commands.DeleteSnippet;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Commands.DeleteSnippet;

public class DeleteSnippetCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly DeleteSnippetCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly SnippetFaker _snippetFaker;

    public DeleteSnippetCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new DeleteSnippetCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _snippetFaker = new SnippetFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteSnippet()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteSnippetCommand(snippet.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedSnippet = await _dbContext.Snippets.FindAsync(snippet.Id);
        deletedSnippet.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new DeleteSnippetCommand(Guid.NewGuid());

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

        var command = new DeleteSnippetCommand(Guid.NewGuid());

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

        var command = new DeleteSnippetCommand(snippet.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Snippet not found.");
    }

    [Fact]
    public async Task Handle_ShouldNotAffectOtherSnippets()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet1 = _snippetFaker.WithUserId(user.Id).Generate();
        var snippet2 = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.AddRange(snippet1, snippet2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteSnippetCommand(snippet1.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingSnippets = await _dbContext.Snippets.ToListAsync();
        remainingSnippets.Should().HaveCount(1);
        remainingSnippets.Single().Id.Should().Be(snippet2.Id);
    }
}
