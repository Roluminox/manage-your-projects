using FluentAssertions;
using MYP.Application.Common.Interfaces;
using MYP.Application.Features.AuditLogs.Queries.GetAuditLogs;
using MYP.Application.Tests.Common;
using MYP.Application.Tests.Common.Fakers;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using NSubstitute;

namespace MYP.Application.Tests.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandlerTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetAuditLogsQueryHandler _handler;
    private readonly UserFaker _userFaker;

    public GetAuditLogsQueryHandlerTests()
    {
        _dbContext = TestDbContext.Create();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new GetAuditLogsQueryHandler(_dbContext, _currentUserService);
        _userFaker = new UserFaker();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedAuditLogs()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        for (int i = 0; i < 25; i++)
        {
            _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Created, i));
        }
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetAuditLogsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnUserAuditLogs()
    {
        // Arrange
        var user = _userFaker.Generate();
        var otherUser = _userFaker.Generate();
        _dbContext.Users.AddRange(user, otherUser);

        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Created));
        _dbContext.AuditLogs.Add(CreateAuditLog(otherUser.Id, "Snippet", AuditAction.Created));
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithEntityTypeFilter_ShouldFilterByEntityType()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Created));
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "TaskItem", AuditAction.Created));
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery(EntityType: "Snippet");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().EntityType.Should().Be("Snippet");
    }

    [Fact]
    public async Task Handle_WithActionFilter_ShouldFilterByAction()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Created));
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Updated));
        _dbContext.AuditLogs.Add(CreateAuditLog(user.Id, "Snippet", AuditAction.Deleted));
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery(Action: AuditAction.Updated);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Action.Should().Be(AuditAction.Updated);
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ShouldFilterByDate()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var oldLog = CreateAuditLog(user.Id, "Snippet", AuditAction.Created);
        oldLog.Timestamp = DateTime.UtcNow.AddDays(-10);
        var recentLog = CreateAuditLog(user.Id, "Snippet", AuditAction.Updated);
        recentLog.Timestamp = DateTime.UtcNow.AddDays(-1);
        _dbContext.AuditLogs.AddRange(oldLog, recentLog);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery(FromDate: DateTime.UtcNow.AddDays(-3));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldOrderByTimestampDescending()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);

        var older = CreateAuditLog(user.Id, "Snippet", AuditAction.Created);
        older.Timestamp = DateTime.UtcNow.AddHours(-2);
        var newer = CreateAuditLog(user.Id, "Snippet", AuditAction.Updated);
        newer.Timestamp = DateTime.UtcNow.AddHours(-1);
        _dbContext.AuditLogs.AddRange(older, newer);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().Timestamp.Should().BeAfter(result.Value.Items.Last().Timestamp);
    }

    [Fact]
    public async Task Handle_WithNoLogs_ShouldReturnEmptyList()
    {
        // Arrange
        var user = _userFaker.Generate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);
        var query = new GetAuditLogsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    private static AuditLog CreateAuditLog(Guid userId, string entityType, AuditAction action, int offsetMinutes = 0)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = Guid.NewGuid(),
            Action = action,
            UserId = userId,
            Timestamp = DateTime.UtcNow.AddMinutes(-offsetMinutes),
            CreatedAt = DateTime.UtcNow
        };
    }
}
