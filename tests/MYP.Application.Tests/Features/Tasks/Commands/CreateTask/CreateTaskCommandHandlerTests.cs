using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tasks.Commands.CreateTask;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateTaskCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public CreateTaskCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateTaskCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTask()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTaskCommand(
            ColumnId: column.Id,
            Title: "New Task",
            Description: "Task description",
            Priority: Priority.High,
            DueDate: DateTime.UtcNow.AddDays(7),
            LabelIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Task");
        result.Value.Description.Should().Be("Task description");
        result.Value.Priority.Should().Be(Priority.High);
        result.Value.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new CreateTaskCommand(
            ColumnId: Guid.NewGuid(),
            Title: "Test",
            Description: null,
            Priority: Priority.Medium,
            DueDate: null,
            LabelIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithNonExistentColumn_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTaskCommand(
            ColumnId: Guid.NewGuid(),
            Title: "Test",
            Description: null,
            Priority: Priority.Medium,
            DueDate: null,
            LabelIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");
    }

    [Fact]
    public async Task Handle_ShouldAssignCorrectOrderToNewTask()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var existingTask = new TaskItem { Id = Guid.NewGuid(), Title = "Existing", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(existingTask);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new CreateTaskCommand(
            ColumnId: column.Id,
            Title: "New Task",
            Description: null,
            Priority: Priority.Medium,
            DueDate: null,
            LabelIds: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Order.Should().Be(1);
    }
}
