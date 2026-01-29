using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tags.Commands.DeleteTag;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tags.Commands.DeleteTag;

public class DeleteTagCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly DeleteTagCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly TagFaker _tagFaker;

    public DeleteTagCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new DeleteTagCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteTag()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteTagCommand(tag.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedTag = await _dbContext.Tags.FindAsync(tag.Id);
        deletedTag.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new DeleteTagCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteTagCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Tag not found.");
    }

    [Fact]
    public async Task Handle_WhenTagBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var tag = _tagFaker.WithUserId(otherUser.Id).Generate();
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteTagCommand(tag.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Tag not found.");
    }

    [Fact]
    public async Task Handle_ShouldNotAffectOtherTags()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var tag1 = _tagFaker.WithUserId(user.Id).Generate();
        var tag2 = _tagFaker.WithUserId(user.Id).Generate();
        _dbContext.Tags.AddRange(tag1, tag2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteTagCommand(tag1.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingTags = await _dbContext.Tags.ToListAsync();
        remainingTags.Should().HaveCount(1);
        remainingTags.Single().Id.Should().Be(tag2.Id);
    }
}
