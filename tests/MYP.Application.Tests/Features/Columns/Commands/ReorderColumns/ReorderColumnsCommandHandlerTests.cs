using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Columns.Commands.ReorderColumns;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Columns.Commands.ReorderColumns;

public class ReorderColumnsCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ReorderColumnsCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public ReorderColumnsCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new ReorderColumnsCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReorderColumns()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", Order = 1, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        var column3 = new Column { Id = Guid.NewGuid(), Name = "Column 3", Order = 2, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.AddRange(column1, column2, column3);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Reorder: 3, 1, 2
        var command = new ReorderColumnsCommand(
            ProjectId: project.Id,
            ColumnIds: new List<Guid> { column3.Id, column1.Id, column2.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedColumns = await _dbContext.Columns
            .Where(c => c.ProjectId == project.Id)
            .OrderBy(c => c.Order)
            .ToListAsync();

        updatedColumns[0].Id.Should().Be(column3.Id);
        updatedColumns[0].Order.Should().Be(0);
        updatedColumns[1].Id.Should().Be(column1.Id);
        updatedColumns[1].Order.Should().Be(1);
        updatedColumns[2].Id.Should().Be(column2.Id);
        updatedColumns[2].Order.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new ReorderColumnsCommand(
            ProjectId: Guid.NewGuid(),
            ColumnIds: new List<Guid> { Guid.NewGuid() }
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

        var command = new ReorderColumnsCommand(
            ProjectId: Guid.NewGuid(),
            ColumnIds: new List<Guid> { Guid.NewGuid() }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Project not found.");
    }

    [Fact]
    public async Task Handle_WithColumnNotBelongingToProject_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column { Id = Guid.NewGuid(), Name = "Column", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.Add(column);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ReorderColumnsCommand(
            ProjectId: project.Id,
            ColumnIds: new List<Guid> { column.Id, Guid.NewGuid() }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Some column IDs do not belong to this project.");
    }

    [Fact]
    public async Task Handle_WithMissingColumns_ShouldReturnFailure()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", Order = 0, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", Order = 1, ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        _dbContext.Columns.AddRange(column1, column2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        // Only include one column
        var command = new ReorderColumnsCommand(
            ProjectId: project.Id,
            ColumnIds: new List<Guid> { column1.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("All project columns must be included in the reorder.");
    }
}
