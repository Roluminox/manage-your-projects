using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Snippets.Queries.GetSnippets;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Snippets.Queries.GetSnippets;

public class GetSnippetsQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetSnippetsQueryHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly SnippetFaker _snippetFaker;
    private readonly TagFaker _tagFaker;

    public GetSnippetsQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetSnippetsQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _snippetFaker = new SnippetFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedSnippets()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        for (int i = 0; i < 15; i++)
        {
            var snippet = _snippetFaker.WithUserId(user.Id).Generate();
            _dbContext.Snippets.Add(snippet);
        }
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(15);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var query = new GetSnippetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnUserSnippets()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var mySnippet = _snippetFaker.WithUserId(user.Id).Generate();
        var otherSnippet = _snippetFaker.WithUserId(otherUser.Id).Generate();
        _dbContext.Snippets.AddRange(mySnippet, otherSnippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Id.Should().Be(mySnippet.Id);
    }

    [Fact]
    public async Task Handle_WithLanguageFilter_ShouldFilterByLanguage()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var jsSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        jsSnippet.Language = "javascript";
        var tsSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        tsSnippet.Language = "typescript";
        _dbContext.Snippets.AddRange(jsSnippet, tsSnippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(Language: "javascript");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Language.Should().Be("javascript");
    }

    [Fact]
    public async Task Handle_WithTagFilter_ShouldFilterByTag()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.Add(tag);

        var taggedSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        taggedSnippet.Tags.Add(tag);
        var untaggedSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.AddRange(taggedSnippet, untaggedSnippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(TagId: tag.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Id.Should().Be(taggedSnippet.Id);
    }

    [Fact]
    public async Task Handle_WithFavoriteFilter_ShouldFilterByFavorite()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var favoriteSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        favoriteSnippet.IsFavorite = true;
        var normalSnippet = _snippetFaker.WithUserId(user.Id).Generate();
        normalSnippet.IsFavorite = false;
        _dbContext.Snippets.AddRange(favoriteSnippet, normalSnippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(IsFavorite: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithSortByTitle_ShouldSortCorrectly()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippetA = _snippetFaker.WithUserId(user.Id).Generate();
        snippetA.Title = "Alpha";
        var snippetZ = _snippetFaker.WithUserId(user.Id).Generate();
        snippetZ.Title = "Zeta";
        _dbContext.Snippets.AddRange(snippetZ, snippetA);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(SortBy: "title", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().Title.Should().Be("Alpha");
        result.Value.Items.Last().Title.Should().Be("Zeta");
    }

    [Fact]
    public async Task Handle_WithNoSnippets_ShouldReturnEmptyList()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldIncludeTagsInResponse()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        tag.Name = "Test Tag";
        _dbContext.Tags.Add(tag);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        snippet.Tags.Add(tag);
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Single().Tags.Should().HaveCount(1);
        result.Value.Items.Single().Tags.Single().Name.Should().Be("Test Tag");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    public async Task Handle_WithInvalidPage_ShouldDefaultToFirstPage(int invalidPage, int expectedPage)
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var snippet = _snippetFaker.WithUserId(user.Id).Generate();
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetSnippetsQuery(Page: invalidPage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(expectedPage);
    }
}
