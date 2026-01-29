using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class Tag : BaseEntity
{
    public const int NameMaxLength = 50;
    public const int ColorMaxLength = 7; // #RRGGBB

    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1"; // Default indigo

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
}
