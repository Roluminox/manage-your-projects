using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.AuditLogs.Queries.GetAuditLogsByEntity;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.AuditLogs.Queries.GetAuditLogsByEntity;

public class GetAuditLogsByEntityQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetAuditLogsByEntityQueryHandler _handler;
    private readonly UserFaker _userFaker;

    public GetAuditLogsByEntityQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetAuditLogsByEntityQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnLogsForSpecificEntity()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var entityId = Guid.NewGuid();
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", entityId, AuditAction.Created));
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", entityId, AuditAction.Updated));
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", Guid.NewGuid(), AuditAction.Created));
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsByEntityQuery("Snippet", entityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(a => a.EntityId.Should().Be(entityId));
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetAuditLogsByEntityQuery("Snippet", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnUserLogs()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        var entityId = Guid.NewGuid();
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", entityId, AuditAction.Created));
        _dbContext.AuditLogs.Add(CreateAuditLog(otherUser.Id, "Snippet", entityId, AuditAction.Updated));
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsByEntityQuery("Snippet", entityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Single().UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldOrderByTimestampDescending()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var entityId = Guid.NewGuid();
        var older = CreateAuditLog(user.Id, "Snippet", entityId, AuditAction.Created);
        older.Timestamp = DateTime.UtcNow.AddHours(-2);
        var newer = CreateAuditLog(user.Id, "Snippet", entityId, AuditAction.Updated);
        newer.Timestamp = DateTime.UtcNow.AddHours(-1);
        _dbContext.AuditLogs.AddRange(older, newer);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsByEntityQuery("Snippet", entityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().Timestamp.Should().BeAfter(result.Value.Last().Timestamp);
    }

    [Fact]
    public async Task Handle_WhenNoLogsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsByEntityQuery("Snippet", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    private static AuditLog CreateAuditLog(Guid userId, string entityType, Guid entityId, AuditAction action)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
