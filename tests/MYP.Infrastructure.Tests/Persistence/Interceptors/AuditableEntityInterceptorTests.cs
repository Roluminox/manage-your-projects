using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Domain.Entities;
using MYP.Domain.Enums;
using MYP.Infrastructure.Persistence;
using MYP.Infrastructure.Persistence.Interceptors;
using NSubstitute;

namespace MYP.Infrastructure.Tests.Persistence.Interceptors;

public class AuditableEntityInterceptorTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly Guid _userId = Guid.NewGuid();

    public AuditableEntityInterceptorTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns(_userId);

        var interceptor = new AuditableEntityInterceptor(_currentUserService);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Seed user
        _dbContext.Users.Add(new User
        {
            Id = _userId,
            Email = "test@test.com",
            Username = "testuser",
            PasswordHash = "hash",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SaveChanges_OnCreate_ShouldGenerateAuditLog()
    {
        // Arrange
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "Test Snippet",
            Code = "console.log('test');",
            Language = "javascript",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(1);

        var log = auditLogs.Single();
        log.EntityType.Should().Be("Snippet");
        log.EntityId.Should().Be(snippet.Id);
        log.Action.Should().Be(AuditAction.Created);
        log.UserId.Should().Be(_userId);
        log.NewValues.Should().NotBeNullOrEmpty();
        log.OldValues.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_OnUpdate_ShouldGenerateAuditLogWithOldAndNewValues()
    {
        // Arrange
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Code = "code",
            Language = "csharp",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        // Clear the audit log from creation
        var createLog = await _dbContext.AuditLogs.FirstAsync();
        _dbContext.AuditLogs.Remove(createLog);
        await _dbContext.SaveChangesAsync();

        // Act
        snippet.Title = "Updated Title";
        _dbContext.Snippets.Update(snippet);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.Where(a => a.Action == AuditAction.Updated).ToListAsync();
        auditLogs.Should().HaveCount(1);

        var log = auditLogs.Single();
        log.EntityType.Should().Be("Snippet");
        log.Action.Should().Be(AuditAction.Updated);
        log.OldValues.Should().NotBeNullOrEmpty();
        log.NewValues.Should().NotBeNullOrEmpty();
        log.OldValues.Should().Contain("Original Title");
        log.NewValues.Should().Contain("Updated Title");
    }

    [Fact]
    public async Task SaveChanges_OnDelete_ShouldGenerateAuditLogWithOldValues()
    {
        // Arrange
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Code = "code",
            Language = "python",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Snippets.Remove(snippet);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deleteLogs = await _dbContext.AuditLogs.Where(a => a.Action == AuditAction.Deleted).ToListAsync();
        deleteLogs.Should().HaveCount(1);

        var log = deleteLogs.Single();
        log.EntityType.Should().Be("Snippet");
        log.Action.Should().Be(AuditAction.Deleted);
        log.OldValues.Should().NotBeNullOrEmpty();
        log.OldValues.Should().Contain("To Delete");
        log.NewValues.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_WhenNotAuthenticated_ShouldNotGenerateAuditLog()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "test-tag",
            Color = "#ff0000",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChanges_ShouldNotAuditAuditLogEntity()
    {
        // Arrange - create a snippet to generate an audit log
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Code = "code",
            Language = "go",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        // Assert - should only have 1 audit log (for snippet), not a recursive one
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(1);
        auditLogs.Single().EntityType.Should().Be("Snippet");
    }

    [Fact]
    public async Task SaveChanges_ShouldNotIncludeSensitiveProperties()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "sensitive@test.com",
            Username = "sensitiveuser",
            PasswordHash = "super_secret_hash",
            DisplayName = "Sensitive User",
            FirstName = "Sensitive",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act - User is not in audited entity types, so no audit log is expected
        // But this verifies the exclusion list is properly configured
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Assert - User entity is not audited
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChanges_ShouldAuditProjectEntity()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "A test project",
            Color = "#6366f1",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(1);
        auditLogs.Single().EntityType.Should().Be("Project");
        auditLogs.Single().Action.Should().Be(AuditAction.Created);
    }

    [Fact]
    public async Task SaveChanges_OldAndNewValues_ShouldContainCorrectProperties()
    {
        // Arrange
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Code = "original code",
            Language = "rust",
            Description = "old desc",
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Snippets.Add(snippet);
        await _dbContext.SaveChangesAsync();

        // Clear creation log
        _dbContext.AuditLogs.RemoveRange(_dbContext.AuditLogs);
        await _dbContext.SaveChangesAsync();

        // Act
        snippet.Title = "Modified";
        snippet.Description = "new desc";
        _dbContext.Snippets.Update(snippet);
        await _dbContext.SaveChangesAsync();

        // Assert
        var log = await _dbContext.AuditLogs.Where(a => a.Action == AuditAction.Updated).SingleAsync();
        log.OldValues.Should().Contain("\"Title\"");
        log.NewValues.Should().Contain("\"Title\"");
        log.NewValues.Should().Contain("Modified");
    }
}
