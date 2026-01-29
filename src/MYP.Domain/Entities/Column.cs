using MYP.Domain.Common;

namespace MYP.Domain.Entities;

public class Column : BaseEntity
{
    public const int NameMaxLength = 50;

    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
