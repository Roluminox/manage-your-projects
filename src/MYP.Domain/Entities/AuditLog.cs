using MYP.Domain.Common;
using MYP.Domain.Enums;

namespace MYP.Domain.Entities;

public class AuditLog : BaseEntity
{
    public const int EntityTypeMaxLength = 100;
    public const int IpAddressMaxLength = 45;
    public const int UserAgentMaxLength = 500;

    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public AuditAction Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
