using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tasks.Commands.DeleteTask;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly DeleteTaskCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public DeleteTaskCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new DeleteTaskCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteTask()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "To Delete", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteTaskCommand(task.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedTask = await _dbContext.TaskItems.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new DeleteTaskCommand(Guid.NewGuid());

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

        var command = new DeleteTaskCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Task not found.");
    }
}
