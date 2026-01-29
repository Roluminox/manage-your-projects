using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Commands.CreateSnippet;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Commands.CreateSnippet;

public class CreateSnippetCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateSnippetCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly TagFaker _tagFaker;

    public CreateSnippetCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateSnippetCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSnippet()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: "A test snippet",
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Test Snippet");
        result.Value.Code.Should().Be("console.log('Hello');");
        result.Value.Language.Should().Be("javascript");
        result.Value.Description.Should().Be("A test snippet");
        result.Value.IsFavorite.Should().BeFalse();
        result.Value.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
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
    public async Task Handle_WithTags_ShouldAssociateTagsWithSnippet()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag1 = new TagFaker().WithUserId(user.Id).Generate();
        var tag2 = new TagFaker().WithUserId(user.Id).Generate();
        _dbContext.Tags.AddRange(tag1, tag2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
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
    public async Task Handle_WithNonOwnedTags_ShouldNotAssociateOtherUsersTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var myTag = new TagFaker().WithUserId(user.Id).Generate();
        var otherTag = new TagFaker().WithUserId(otherUser.Id).Generate();
        _dbContext.Tags.AddRange(myTag, otherTag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid> { myTag.Id, otherTag.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(1);
        result.Value.Tags.Single().Id.Should().Be(myTag.Id);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeLanguageToLowercase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
            Language: "JavaScript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Language.Should().Be("javascript");
    }

    [Fact]
    public async Task Handle_WithEmptyTagIds_ShouldCreateSnippetWithNoTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
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
    public async Task Handle_ShouldPersistSnippetToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateSnippetCommand(
            Title: "Persisted Snippet",
            Code: "const x = 1;",
            Language: "typescript",
            Description: "Check persistence",
            TagIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedSnippet = await _dbContext.Snippets.FindAsync(result.Value.Id);
        savedSnippet.Should().NotBeNull();
        savedSnippet!.Title.Should().Be("Persisted Snippet");
        savedSnippet.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentTagIds_ShouldIgnoreThem()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var nonExistentTagId = Guid.NewGuid();

        var command = new CreateSnippetCommand(
            Title: "Test Snippet",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid> { nonExistentTagId }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().BeEmpty();
    }
}
