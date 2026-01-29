using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tags.Queries.GetTags;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tags.Queries.GetTags;

public class GetTagsQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetTagsQueryHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly TagFaker _tagFaker;

    public GetTagsQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetTagsQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnUserTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag1 = _tagFaker.WithUserId(user.Id).Generate();
        tag1.Name = "Tag 1";
        var tag2 = _tagFaker.WithUserId(user.Id).Generate();
        tag2.Name = "Tag 2";
        _dbContext.Tags.AddRange(tag1, tag2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnUserTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var myTag = _tagFaker.WithUserId(user.Id).Generate();
        var otherTag = _tagFaker.WithUserId(otherUser.Id).Generate();
        _dbContext.Tags.AddRange(myTag, otherTag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Id.Should().Be(myTag.Id);
    }

    [Fact]
    public async Task Handle_WithNoTags_ShouldReturnEmptyList()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnTagsOrderedByName()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tagZ = _tagFaker.WithUserId(user.Id).Generate();
        tagZ.Name = "Zeta";
        var tagA = _tagFaker.WithUserId(user.Id).Generate();
        tagA.Name = "Alpha";
        var tagM = _tagFaker.WithUserId(user.Id).Generate();
        tagM.Name = "Middle";
        _dbContext.Tags.AddRange(tagZ, tagA, tagM);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].Name.Should().Be("Alpha");
        result.Value[1].Name.Should().Be("Middle");
        result.Value[2].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTagProperties()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        tag.Name = "Test Tag";
        tag.Color = "#ff0000";
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnedTag = result.Value.Single();
        returnedTag.Id.Should().Be(tag.Id);
        returnedTag.Name.Should().Be("Test Tag");
        returnedTag.Color.Should().Be("#ff0000");
    }
}
