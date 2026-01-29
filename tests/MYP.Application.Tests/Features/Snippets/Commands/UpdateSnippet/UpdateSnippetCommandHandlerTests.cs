using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Commands.UpdateSnippet;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Commands.UpdateSnippet;

public class UpdateSnippetCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UpdateSnippetCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly SnippetFaker _snippetFaker;
    private readonly TagFaker _tagFaker;

    public UpdateSnippetCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new UpdateSnippetCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _snippetFaker = new SnippetFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSnippet()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Title = "Original Title";
        snippet.Code = "original code";
        snippet.Language = "javascript";
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Updated Title",
            Code: "updated code",
            Language: "typescript",
            Description: "Updated description",
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Updated Title");
        result.Value.Code.Should().Be("updated code");
        result.Value.Language.Should().Be("typescript");
        result.Value.Description.Should().Be("Updated description");
        result.Value.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new UpdateSnippetCommand(
            Id: Guid.NewGuid(),
            Title: "Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

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

        var command = new UpdateSnippetCommand(
            Id: Guid.NewGuid(),
            Title: "Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

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

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Snippet not found.");
    }

    [Fact]
    public async Task Handle_WithTags_ShouldUpdateSnippetTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.Add(snippet);

        var tag1 = _tagFaker.WithUserId(user.Id).Generate();
        var tag2 = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.AddRange(tag1, tag2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid> { tag1.Id, tag2.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(2);
        result.Value.Tags.Select(t => t.Id).Should().Contain(tag1.Id);
        result.Value.Tags.Select(t => t.Id).Should().Contain(tag2.Id);
    }

    [Fact]
    public async Task Handle_WithEmptyTagIds_ShouldRemoveAllTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.Add(tag);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Tags.Add(tag);
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullTagIds_ShouldNotUpdateTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.Add(tag);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Tags.Add(tag);
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Updated Title",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(1);
        result.Value.Tags.Single().Id.Should().Be(tag.Id);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeLanguageToLowercase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Title",
            Code: "code",
            Language: "TypeScript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Language.Should().Be("typescript");
    }

    [Fact]
    public async Task Handle_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Title = "Original";
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateSnippetCommand(
            Id: snippet.Id,
            Title: "Persisted Update",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedSnippet = await _dbContext.Snippets.FindAsync(snippet.Id);
        savedSnippet!.Title.Should().Be("Persisted Update");
    }
}
