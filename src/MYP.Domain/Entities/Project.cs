using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class Project : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int ColorMaxLength = 7;
    public const string DefaultColor = "#6366f1";

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = DefaultColor;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Column> Columns { get; set; } = new List<Column>();

    public static Project Create(string name, string? description, string? color, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));

        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Project name cannot exceed {NameMaxLength} characters.", nameof(name));

        if (description?.Length > DescriptionMaxLength)
            throw new ArgumentException($"Description cannot exceed {DescriptionMaxLength} characters.", nameof(description));

        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? DefaultColor : color,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));

        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Project name cannot exceed {NameMaxLength} characters.", nameof(name));

        if (description?.Length > DescriptionMaxLength)
            throw new ArgumentException($"Description cannot exceed {DescriptionMaxLength} characters.", nameof(description));

        Name = name.Trim();
        Description = description?.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? DefaultColor : color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddColumn(Column column)
    {
        Columns.Add(column);
        UpdatedAt = DateTime.UtcNow;
    }
}
