using Microsoft.EntityFrameworkCore;
using MYP.Domain.Entities;

namespace MYP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Snippet> Snippets { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Project> Projects { get; }
    DbSet<Column> Columns { get; }
    DbSet<TaskItem> TaskItems { get; }
    DbSet<Label> Labels { get; }
    DbSet<ChecklistItem> ChecklistItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
