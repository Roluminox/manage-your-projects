using FluentAssertions;
using MYP.Domain.Entities;
using MYP.Domain.Enums;

namespace MYP.Domain.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var auditLog = new AuditLog();

        // Assert
        auditLog.Id.Should().Be(Guid.Empty);
        auditLog.EntityType.Should().BeEmpty();
        auditLog.EntityId.Should().Be(Guid.Empty);
        auditLog.Action.Should().Be(AuditAction.Created);
        auditLog.OldValues.Should().BeNull();
        auditLog.NewValues.Should().BeNull();
        auditLog.UserId.Should().Be(Guid.Empty);
        auditLog.IpAddress.Should().BeNull();
        auditLog.UserAgent.Should().BeNull();
    }

    [Fact]
    public void AuditLog_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        // Act
        var auditLog = new AuditLog
        {
            Id = id,
            EntityType = "Snippet",
            EntityId = entityId,
            Action = AuditAction.Updated,
            OldValues = "{\"Title\":\"Old\"}",
            NewValues = "{\"Title\":\"New\"}",
            UserId = userId,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            Timestamp = timestamp
        };

        // Assert
        auditLog.Id.Should().Be(id);
        auditLog.EntityType.Should().Be("Snippet");
        auditLog.EntityId.Should().Be(entityId);
        auditLog.Action.Should().Be(AuditAction.Updated);
        auditLog.OldValues.Should().Be("{\"Title\":\"Old\"}");
        auditLog.NewValues.Should().Be("{\"Title\":\"New\"}");
        auditLog.UserId.Should().Be(userId);
        auditLog.IpAddress.Should().Be("192.168.1.1");
        auditLog.UserAgent.Should().Be("Mozilla/5.0");
        auditLog.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void AuditLog_ShouldSupportAllActions()
    {
        // Arrange & Act & Assert
        var created = new AuditLog { Action = AuditAction.Created };
        var updated = new AuditLog { Action = AuditAction.Updated };
        var deleted = new AuditLog { Action = AuditAction.Deleted };

        created.Action.Should().Be(AuditAction.Created);
        updated.Action.Should().Be(AuditAction.Updated);
        deleted.Action.Should().Be(AuditAction.Deleted);
    }

    [Fact]
    public void AuditAction_ShouldHaveCorrectIntValues()
    {
        // Assert
        ((int)AuditAction.Created).Should().Be(0);
        ((int)AuditAction.Updated).Should().Be(1);
        ((int)AuditAction.Deleted).Should().Be(2);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        AuditLog.EntityTypeMaxLength.Should().Be(100);
        AuditLog.IpAddressMaxLength.Should().Be(45);
        AuditLog.UserAgentMaxLength.Should().Be(500);
    }
}
