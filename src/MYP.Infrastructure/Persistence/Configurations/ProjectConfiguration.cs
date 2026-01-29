using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Project.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(Project.DescriptionMaxLength);

        builder.Property(p => p.Color)
            .IsRequired()
            .HasMaxLength(Project.ColorMaxLength)
            .HasDefaultValue(Project.DefaultColor);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Columns)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CreatedAt);
    }
}
