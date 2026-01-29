using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Columns.Commands.UpdateColumn;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UpdateColumnCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public UpdateColumnCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new UpdateColumnCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateColumn()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("Test Project", null, null, user.Id);
        _dbContext.Projects.Add(project);

        var column = new Column
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Order = 0,
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Columns.Add(column);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new UpdateColumnCommand(
            Id: column.Id,
            Name: "Updated Name"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new UpdateColumnCommand(
            Id: Guid.NewGuid(),
            Name: "Test"
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

        var command = new UpdateColumnCommand(
            Id: Guid.NewGuid(),
            Name: "Test"
        );

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

        var command = new UpdateColumnCommand(
            Id: otherColumn.Id,
            Name: "Hacked"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Column not found.");
    }
}
