using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Tasks.Commands.ArchiveTask;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.Tasks.Commands.ArchiveTask;

public class ArchiveTaskCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ArchiveTaskCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public ArchiveTaskCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new ArchiveTaskCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ArchiveTask_ShouldSetIsArchivedToTrue()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "To Archive", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, IsArchived = false, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ArchiveTaskCommand(task.Id, Archive: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var archivedTask = await _dbContext.TaskItems.FindAsync(task.Id);
        archivedTask!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UnarchiveTask_ShouldSetIsArchivedToFalse()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "To Do", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);

        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Archived", Order = 0, ColumnId = column.Id, Priority = Priority.Medium, IsArchived = true, CreatedAt = DateTime.UtcNow };
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ArchiveTaskCommand(task.Id, Archive: false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var unarchivedTask = await _dbContext.TaskItems.FindAsync(task.Id);
        unarchivedTask!.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new ArchiveTaskCommand(Guid.NewGuid(), true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }
}
