using MYP.Domain.Common;
using MYP.Domain.Enums;

namespace MYP.Domain.Entities;

public class TaskItem : BaseEntity
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }
    public bool IsArchived { get; set; }

    public Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;

    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<ChecklistItem> Checklists { get; set; } = new List<ChecklistItem>();
}
