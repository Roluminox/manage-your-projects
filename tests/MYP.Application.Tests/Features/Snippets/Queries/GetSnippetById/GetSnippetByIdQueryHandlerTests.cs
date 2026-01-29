using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Queries.GetSnippetById;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Queries.GetSnippetById;

public class GetSnippetByIdQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetSnippetByIdQueryHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly SnippetFaker _snippetFaker;
    private readonly TagFaker _tagFaker;

    public GetSnippetByIdQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetSnippetByIdQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _snippetFaker = new SnippetFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnSnippet()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Title = "Test Snippet";
        snippet.Code = "console.log('test');";
        snippet.Language = "javascript";
        snippet.Description = "Test description";
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetByIdQuery(snippet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(snippet.Id);
        result.Value.Title.Should().Be("Test Snippet");
        result.Value.Code.Should().Be("console.log('test');");
        result.Value.Language.Should().Be("javascript");
        result.Value.Description.Should().Be("Test description");
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var query = new GetSnippetByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetSnippetByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetSnippetByIdQuery(snippet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Snippet not found.");
    }

    [Fact]
    public async Task Handle_ShouldIncludeTagsInResponse()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag1 = _tagFaker.WithUserId(user.Id).Generate();
        tag1.Name = "Tag 1";
        var tag2 = _tagFaker.WithUserId(user.Id).Generate();
        tag2.Name = "Tag 2";
        _dbContext.Tags.AddRange(tag1, tag2);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Tags.Add(tag1);
        snippet.Tags.Add(tag2);
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetByIdQuery(snippet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(2);
        result.Value.Tags.Select(t => t.Name).Should().Contain("Tag 1");
        result.Value.Tags.Select(t => t.Name).Should().Contain("Tag 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectIsFavoriteStatus()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.IsFavorite = true;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetByIdQuery(snippet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTimestamps()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.CreatedAt = createdAt;
        snippet.UpdatedAt = updatedAt;
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetByIdQuery(snippet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.UpdatedAt.Should().Be(updatedAt);
    }
}
