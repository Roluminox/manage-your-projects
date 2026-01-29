using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence.Configurations;

public class SnippetConfiguration : IEntityTypeConfiguration<Snippet>
{
    public void Configure(EntityTypeBuilder<Snippet> builder)
    {
        builder.ToTable("Snippets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(Snippet.TitleMaxLength);

        builder.Property(s => s.Code)
            .IsRequired();

        builder.Property(s => s.Language)
            .IsRequired()
            .HasMaxLength(Snippet.LanguageMaxLength);

        builder.Property(s => s.Description)
            .HasMaxLength(Snippet.DescriptionMaxLength);

        builder.Property(s => s.IsFavorite)
            .HasDefaultValue(false);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Tags)
            .WithMany(t => t.Snippets)
            .UsingEntity<Dictionary<string, object>>(
                "SnippetTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Snippet>().WithMany().HasForeignKey("SnippetId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("SnippetTags");
                    j.HasKey("SnippetId", "TagId");
                });

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.Language);
        builder.HasIndex(s => s.IsFavorite);
        builder.HasIndex(s => s.CreatedAt);
    }
}
