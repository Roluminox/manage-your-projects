using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.ToTable("Columns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(Column.NameMaxLength);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.HasOne(c => c.Project)
            .WithMany(p => p.Columns)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Column)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => new { c.ProjectId, c.Order });
    }
}
