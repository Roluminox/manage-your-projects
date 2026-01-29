using Microsoft.EntityFrameworkCore;
using MYP.Domain.Entities;

namespace MYP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Snippet> Snippets { get; }
    DbSet<Tag> Tags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
