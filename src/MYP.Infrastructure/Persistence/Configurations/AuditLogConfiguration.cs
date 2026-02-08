using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(AuditLog.EntityTypeMaxLength);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.Action)
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(AuditLog.IpAddressMaxLength);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(AuditLog.UserAgentMaxLength);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.Action);
    }
}
