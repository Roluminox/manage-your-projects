using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;
using MYP.Domain.Enums;

namespace MYP.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskItem.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasDefaultValue(Priority.Medium);

        builder.Property(t => t.Order)
            .IsRequired();

        builder.Property(t => t.IsArchived)
            .HasDefaultValue(false);

        builder.HasOne(t => t.Column)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Labels)
            .WithMany(l => l.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskItemLabel",
                j => j.HasOne<Label>().WithMany().HasForeignKey("LabelId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<TaskItem>().WithMany().HasForeignKey("TaskItemId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("TaskItemLabels");
                    j.HasKey("TaskItemId", "LabelId");
                });

        builder.HasMany(t => t.Checklists)
            .WithOne(c => c.TaskItem)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.ColumnId);
        builder.HasIndex(t => new { t.ColumnId, t.Order });
        builder.HasIndex(t => t.IsArchived);
        builder.HasIndex(t => t.DueDate);
    }
}
