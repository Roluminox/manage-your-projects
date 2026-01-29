using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tags.Commands.CreateTag;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tags.Commands.CreateTag;

public class CreateTagCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateTagCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly TagFaker _tagFaker;

    public CreateTagCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateTagCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _tagFaker = new TagFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTag()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "Test Tag", Color: "#ff0000");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Tag");
        result.Value.Color.Should().Be("#ff0000");
    }

    [Fact]
    public async Task Handle_WithNullColor_ShouldUseDefaultColor()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "Test Tag", Color: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Color.Should().Be("#6366f1");
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new CreateTagCommand(Name: "Test Tag", Color: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var existingTag = _tagFaker.WithUserId(user.Id).Generate();
        existingTag.Name = "Existing Tag";
        _dbContext.Tags.Add(existingTag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "Existing Tag", Color: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A tag with this name already exists.");
    }

    [Fact]
    public async Task Handle_WithDuplicateNameDifferentCase_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var existingTag = _tagFaker.WithUserId(user.Id).Generate();
        existingTag.Name = "existing tag";
        _dbContext.Tags.Add(existingTag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "EXISTING TAG", Color: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A tag with this name already exists.");
    }

    [Fact]
    public async Task Handle_WithSameNameDifferentUser_ShouldSucceed()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var otherTag = _tagFaker.WithUserId(otherUser.Id).Generate();
        otherTag.Name = "Shared Name";
        _dbContext.Tags.Add(otherTag);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "Shared Name", Color: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Shared Name");
    }

    [Fact]
    public async Task Handle_ShouldPersistTagToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTagCommand(Name: "Persisted Tag", Color: "#00ff00");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedTag = await _dbContext.Tags.FindAsync(result.Value.Id);
        savedTag.Should().NotBeNull();
        savedTag!.Name.Should().Be("Persisted Tag");
        savedTag.Color.Should().Be("#00ff00");
        savedTag.UserId.Should().Be(user.Id);
    }
}
