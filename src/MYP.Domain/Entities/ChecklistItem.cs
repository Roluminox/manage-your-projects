using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class ChecklistItem : BaseEntity
{
    public const int TextMaxLength = 200;

    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Order { get; set; }

    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public void Toggle()
    {
        IsCompleted = !IsCompleted;
        UpdatedAt = DateTime.UtcNow;
    }
}
