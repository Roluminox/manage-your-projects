using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Columns.Commands.DeleteColumn;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Columns.Commands.DeleteColumn;

public class DeleteColumnCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly DeleteColumnCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public DeleteColumnCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new DeleteColumnCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteColumn()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Order = 0,
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Columns.Add(column);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteColumnCommand(column.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedColumn = await _dbContext.Columns.FindAsync(column.Id);
        deletedColumn.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new DeleteColumnCommand(Guid.NewGuid());

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

        var command = new DeleteColumnCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");
    }

    [Fact]
    public async Task Handle_WithOtherUsersColumn_ShouldReturnNotFound()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var otherProject = Project.Create("Other Project", null, null, otherUser.Id);
        _dbContext.Projects.Add(otherProject);

        var otherColumn = new Column
        {
            Id = Guid.NewGuid(),
            Name = "Other Column",
            Order = 0,
            ProjectId = otherProject.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Columns.Add(otherColumn);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteColumnCommand(otherColumn.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");

        // Verify column still exists
        var columnStillExists = await _dbContext.Columns.AnyAsync(c => c.Id == otherColumn.Id);
        columnStillExists.Should().BeTrue();
    }
}
