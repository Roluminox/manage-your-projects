using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class Snippet : BaseEntity
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 1000;
    public const int LanguageMaxLength = 50;

    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFavorite { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string code, string language, string? description)
    {
        Title = title;
        Code = code;
        Language = language;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTags(IEnumerable<Tag> tags)
    {
        Tags.Clear();
        foreach (var tag in tags)
        {
            Tags.Add(tag);
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
