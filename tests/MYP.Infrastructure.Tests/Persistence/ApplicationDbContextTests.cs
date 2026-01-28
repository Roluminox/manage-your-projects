using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Domain.Entities;
using MYP.Infrastructure.Persistence;
using MYP.Infrastructure.Tests.Common.Fakers;

namespace MYP.Infrastructure.Tests.Persistence;

public class ApplicationDbContextTests
{
    [Fact]
    public async Task AddUser_ShouldPersistUser_WithInMemoryDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userFaker = new UserFaker();
        var user = userFaker.Generate();

        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDbContext(options))
        {
            var savedUser = await context.Users.FindAsync(user.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be(user.Email);
            savedUser.Username.Should().Be(user.Username);
        }
    }

    [Fact]
    public async Task Users_DbSet_ShouldBeQueryable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userFaker = new UserFaker();
        var users = userFaker.Generate(3);

        await using (var context = new ApplicationDbContext(options))
        {
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        await using (var context = new ApplicationDbContext(options))
        {
            var count = await context.Users.CountAsync();
            count.Should().Be(3);

            var activeUsers = await context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
            activeUsers.Should().HaveCount(3);
        }
    }
}
