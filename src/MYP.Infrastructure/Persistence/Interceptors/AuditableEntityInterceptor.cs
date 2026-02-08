using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MYP.Application.Common.Interfaces;
using MYP.Domain.Entities;
using MYP.Domain.Enums;

namespace MYP.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "RefreshToken",
        "RefreshTokenExpiryTime"
    };

    private static readonly HashSet<string> AuditedEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Snippet),
        nameof(Tag),
        nameof(Project),
        nameof(Column),
        nameof(TaskItem),
        nameof(Label),
        nameof(ChecklistItem)
    };

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AddAuditLogs(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AddAuditLogs(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void AddAuditLogs(DbContext context)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog)
            .Where(e => AuditedEntityTypes.Contains(e.Metadata.ClrType.Name))
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Metadata.ClrType.Name;
            var entityId = entry.Property("Id").CurrentValue as Guid? ?? Guid.Empty;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => throw new InvalidOperationException()
            };

            string? oldValues = null;
            string? newValues = null;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = false };

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                var old = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (ExcludedProperties.Contains(prop.Metadata.Name)) continue;
                    if (prop.Metadata.IsPrimaryKey()) continue;

                    if (entry.State == EntityState.Deleted || prop.IsModified)
                    {
                        old[prop.Metadata.Name] = prop.OriginalValue;
                    }
                }
                if (old.Count > 0)
                    oldValues = JsonSerializer.Serialize(old, jsonOptions);
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var current = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (ExcludedProperties.Contains(prop.Metadata.Name)) continue;
                    if (prop.Metadata.IsPrimaryKey()) continue;

                    if (entry.State == EntityState.Added || prop.IsModified)
                    {
                        current[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                if (current.Count > 0)
                    newValues = JsonSerializer.Serialize(current, jsonOptions);
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId.Value,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<AuditLog>().Add(auditLog);
        }
    }
}
