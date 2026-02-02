using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tasks.Commands.ReorderTasks;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tasks.Commands.ReorderTasks;

public class ReorderTasksCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ReorderTasksCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public ReorderTasksCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new ReorderTasksCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ReorderTasks_ShouldUpdateOrderCorrectly()
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

        // Reorder: 3, 1, 2
        var command = new ReorderTasksCommand(
            ColumnId: column.Id,
            TaskIds: new List<Guid> { task3.Id, task1.Id, task2.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tasks = await _dbContext.TaskItems.Where(t => t.ColumnId == column.Id).OrderBy(t => t.Order).ToListAsync();
        tasks[0].Id.Should().Be(task3.Id);
        tasks[0].Order.Should().Be(0);
        tasks[1].Id.Should().Be(task1.Id);
        tasks[1].Order.Should().Be(1);
        tasks[2].Id.Should().Be(task2.Id);
        tasks[2].Order.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new ReorderTasksCommand(
            ColumnId: Guid.NewGuid(),
            TaskIds: new List<Guid> { Guid.NewGuid() }
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

        var command = new ReorderTasksCommand(
            ColumnId: Guid.NewGuid(),
            TaskIds: new List<Guid> { Guid.NewGuid() }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");
    }

    [Fact]
    public async Task Handle_WithTaskFromDifferentColumn_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column1 = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Done", Order = 1, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.AddRange(column1, column2);

        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Order = 0, ColumnId = column1.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Order = 0, ColumnId = column2.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.AddRange(task1, task2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Try to include task from column2 in reorder for column1
        var command = new ReorderTasksCommand(
            ColumnId: column1.Id,
            TaskIds: new List<Guid> { task1.Id, task2.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Some task IDs do not belong to this column.");
    }

    [Fact]
    public async Task Handle_WithMissingTask_ShouldReturnFailure()
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
        _dbContext.TaskItems.AddRange(task1, task2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Only include task1, missing task2
        var command = new ReorderTasksCommand(
            ColumnId: column.Id,
            TaskIds: new List<Guid> { task1.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("All column tasks must be included in the reorder.");
    }

    [Fact(Skip = "EF Core InMemory provider doesn't support filtered Include. This should be tested with integration tests using a real database.")]
    public async Task Handle_WithArchivedTasksExcluded_ShouldSucceed()
    {
        // Note: This test verifies that archived tasks are excluded from reorder validation.
        // The handler uses .Include(c => c.Tasks.Where(t => !t.IsArchived)) which works
        // with real databases but not with InMemory provider.

        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Order = 1, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow, IsArchived = true };
        var task3 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 3", Order = 2, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.AddRange(task1, task2, task3);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Reorder only non-archived tasks (task1, task3), excluding archived task2
        var command = new ReorderTasksCommand(
            ColumnId: column.Id,
            TaskIds: new List<Guid> { task3.Id, task1.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue($"Expected success but got errors: {string.Join(", ", result.Errors)}");
        var tasks = await _dbContext.TaskItems.Where(t => t.ColumnId == column.Id && !t.IsArchived).OrderBy(t => t.Order).ToListAsync();
        tasks[0].Id.Should().Be(task3.Id);
        tasks[1].Id.Should().Be(task1.Id);
    }

    [Fact]
    public async Task Handle_WithDifferentProjectOwner_ShouldReturnFailure()
    {
        // Arrange
        var user1 = _userFaker.Generate();
        var user2 = _userFaker.Generate();
        _dbContext.Users.AddRange(user1, user2);

        var project = Project.Create("Test", null, null, user1.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        // User2 trying to reorder tasks in user1's project
        _currentUserService.UserId.Returns(user2.Id);

        var command = new ReorderTasksCommand(
            ColumnId: column.Id,
            TaskIds: new List<Guid> { task.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var initialTime = DateTime.UtcNow.AddHours(-1);
        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = initialTime, UpdatedAt = initialTime };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Order = 1, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = initialTime, UpdatedAt = initialTime };
        _dbContext.TaskItems.AddRange(task1, task2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ReorderTasksCommand(
            ColumnId: column.Id,
            TaskIds: new List<Guid> { task2.Id, task1.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedTask1 = await _dbContext.TaskItems.FindAsync(task1.Id);
        var updatedTask2 = await _dbContext.TaskItems.FindAsync(task2.Id);
        updatedTask1!.UpdatedAt.Should().BeAfter(initialTime);
        updatedTask2!.UpdatedAt.Should().BeAfter(initialTime);
    }
}
