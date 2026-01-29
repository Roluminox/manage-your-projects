using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.Projects.Commands.DeleteProject;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using NSubstitute;

namespace MYP.Application.Tests.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly DeleteProjectCommandHandler _handler;
    private readonly UserFaker _userFaker;

    public DeleteProjectCommandHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new DeleteProjectCommandHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteProject()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project = Project.Create("To Delete", null, null, user.Id);
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteProjectCommand(project.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedProject = await _dbContext.Projects.FindAsync(project.Id);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var command = new DeleteProjectCommand(Guid.NewGuid());

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

        var command = new DeleteProjectCommand(Guid.NewGuid());

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

        var command = new DeleteProjectCommand(otherProject.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Project not found.");

        // Verify project still exists
        var projectStillExists = await _dbContext.Projects.AnyAsync(p => p.Id == otherProject.Id);
        projectStillExists.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldRemoveProjectFromDatabase()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var project1 = Project.Create("Project 1", null, null, user.Id);
        var project2 = Project.Create("Project 2", null, null, user.Id);
        _dbContext.Projects.AddRange(project1, project2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new DeleteProjectCommand(project1.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingProjects = await _dbContext.Projects.Where(p => p.UserId == user.Id).ToListAsync();
        remainingProjects.Should().HaveCount(1);
        remainingProjects.Single().Id.Should().Be(project2.Id);
    }
}
