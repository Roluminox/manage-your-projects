using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class Label : BaseEntity
{
    public const int NameMaxLength = 30;
    public const int ColorMaxLength = 7;

    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
