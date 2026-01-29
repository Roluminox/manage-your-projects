using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tasks.Commands.MoveTask;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly MoveTaskCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public MoveTaskCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new MoveTaskCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_MoveTaskToAnotherColumn_ShouldUpdateColumnAndOrder()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column1 = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "In Progress", Order = 1, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.AddRange(column1, column2);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task", Order = 0, ColumnId = column1.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new MoveTaskCommand(
            TaskId: task.Id,
            TargetColumnId: column2.Id,
            NewOrder: 0
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var movedTask = await _dbContext.TaskItems.FindAsync(task.Id);
        movedTask!.ColumnId.Should().Be(column2.Id);
        movedTask.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MoveTaskWithinSameColumn_ShouldReorderCorrectly()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Order = 1, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        var task3 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 3", Order = 2, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.AddRange(task1, task2, task3);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Move task3 to position 0
        var command = new MoveTaskCommand(
            TaskId: task3.Id,
            TargetColumnId: column.Id,
            NewOrder: 0
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var movedTask = await _dbContext.TaskItems.FindAsync(task3.Id);
        movedTask!.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new MoveTaskCommand(
            TaskId: Guid.NewGuid(),
            TargetColumnId: Guid.NewGuid(),
            NewOrder: 0
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithNonExistentTask_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new MoveTaskCommand(
            TaskId: Guid.NewGuid(),
            TargetColumnId: Guid.NewGuid(),
            NewOrder: 0
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Task not found.");
    }

    [Fact]
    public async Task Handle_MoveTaskToDifferentProject_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project1 = Project.Create("Project 1", null, null, user.Id);
        var project2 = Project.Create("Project 2", null, null, user.Id);
        _dbContext.Projects.AddRange(project1, project2);

        var column1 = new Column { Id = Guid.NewGuid(), Name = "Col 1", Order = 0, ProjectId = project1.Id, CreatedAt = DateTime.UtcNow };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Col 2", Order = 0, ProjectId = project2.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.AddRange(column1, column2);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task", Order = 0, ColumnId = column1.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new MoveTaskCommand(
            TaskId: task.Id,
            TargetColumnId: column2.Id,
            NewOrder: 0
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Cannot move task to a column in a different project.");
    }
}
