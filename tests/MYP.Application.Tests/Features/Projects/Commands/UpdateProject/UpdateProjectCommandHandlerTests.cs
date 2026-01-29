using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Projects.Commands.UpdateProject;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UpdateProjectCommandHandler _handler;
    private readonly UserFaker _userFaker;
    private readonly ProjectFaker _projectFaker;

    public UpdateProjectCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new UpdateProjectCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
        _projectFaker = new ProjectFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProject()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Original Name", "Original desc", "#000000", user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateProjectCommand(
            Id: project.Id,
            Name: "Updated Name",
            Description: "Updated description",
            Color: "#ffffff"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Description.Should().Be("Updated description");
        result.Value.Color.Should().Be("#ffffff");
        result.Value.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            Color: null
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

        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            Color: null
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

        var command = new UpdateProjectCommand(
            Id: otherProject.Id,
            Name: "Hacked Name",
            Description: null,
            Color: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Project not found.");
    }

    [Fact]
    public async Task Handle_WithNullColor_ShouldUseDefaultColor()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, "#123456", user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateProjectCommand(
            Id: project.Id,
            Name: "Updated",
            Description: null,
            Color: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Color.Should().Be(Project.DefaultColor);
    }

    [Fact]
    public async Task Handle_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Original", null, null, user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateProjectCommand(
            Id: project.Id,
            Name: "Persisted Update",
            Description: "New description",
            Color: "#abcdef"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedProject = await _dbContext.Projects.FindAsync(project.Id);
        savedProject.Should().NotBeNull();
        savedProject!.Name.Should().Be("Persisted Update");
        savedProject.Description.Should().Be("New description");
        savedProject.Color.Should().Be("#abcdef");
    }
}
