using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("Labels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(Label.NameMaxLength);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(Label.ColorMaxLength);

        builder.HasOne(l => l.Project)
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.ProjectId);
        builder.HasIndex(l => new { l.ProjectId, l.Name }).IsUnique();
    }
}
