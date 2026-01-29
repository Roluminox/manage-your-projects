using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("ChecklistItems");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Text)
            .IsRequired()
            .HasMaxLength(ChecklistItem.TextMaxLength);

        builder.Property(c => c.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.HasOne(c => c.TaskItem)
            .WithMany(t => t.Checklists)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.TaskItemId);
        builder.HasIndex(c => new { c.TaskItemId, c.Order });
    }
}
