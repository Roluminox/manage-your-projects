using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Columns.Commands.CreateColumn;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateColumnCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public CreateColumnCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateColumnCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateColumn()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateColumnCommand(
            ProjectId: project.Id,
            Name: "To Do"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("To Do");
        result.Value.Order.Should().Be(0);
        result.Value.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new CreateColumnCommand(
            ProjectId: Guid.NewGuid(),
            Name: "Test"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithNonExistentProject_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateColumnCommand(
            ProjectId: Guid.NewGuid(),
            Name: "Test"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Project not found.");
    }

    [Fact]
    public async Task Handle_WithOtherUsersProject_ShouldReturnNotFound()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var otherProject = Project.Create("Other Project", null, null, otherUser.Id);
        _dbContext.Projects.Add(otherProject);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateColumnCommand(
            ProjectId: otherProject.Id,
            Name: "Hacked Column"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Project not found.");
    }

    [Fact]
    public async Task Handle_ShouldAssignCorrectOrderToNewColumn()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var existingColumn = new Column
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Order = 0,
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Columns.Add(existingColumn);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateColumnCommand(
            ProjectId: project.Id,
            Name: "New Column"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Order.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldTrimColumnName()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateColumnCommand(
            ProjectId: project.Id,
            Name: "  To Do  "
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("To Do");
    }
}
