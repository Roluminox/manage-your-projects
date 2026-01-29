using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MYP.Application.Common.Interfaces;
using MYP.Domain.Entities;
using MYP.Domain.Interfaces;

namespace MYP.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Snippet> Snippets => Set<Snippet>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
