using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Projects.Commands.CreateProject;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateProjectCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public CreateProjectCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateProjectCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProject()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateProjectCommand(
            Name: "My Kanban Board",
            Description: "A test project",
            Color: "#ff5733"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("My Kanban Board");
        result.Value.Description.Should().Be("A test project");
        result.Value.Color.Should().Be("#ff5733");
        result.Value.Columns.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new CreateProjectCommand(
            Name: "Test Project",
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
    public async Task Handle_WithNullColor_ShouldUseDefaultColor()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateProjectCommand(
            Name: "Test Project",
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
    public async Task Handle_ShouldPersistProjectToDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateProjectCommand(
            Name: "Persisted Project",
            Description: "Check persistence",
            Color: "#123456"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedProject = await _dbContext.Projects.FindAsync(result.Value.Id);
        savedProject.Should().NotBeNull();
        savedProject!.Name.Should().Be("Persisted Project");
        savedProject.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateProjectCommand(
            Name: "Test Project",
            Description: null,
            Color: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldTrimNameAndDescription()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateProjectCommand(
            Name: "  Test Project  ",
            Description: "  Description with spaces  ",
            Color: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Project");
        result.Value.Description.Should().Be("Description with spaces");
    }
}
