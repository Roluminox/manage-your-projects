using Microsoft.EntityFrameworkCore;
using MYP.Domain.Entities;

namespace MYP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
