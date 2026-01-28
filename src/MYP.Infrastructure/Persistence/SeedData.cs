using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MYP.Domain.Entities;

namespace MYP.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            await SeedUsersAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Users already seeded");
            return;
        }

        logger.LogInformation("Seeding users...");

        var users = new List<User>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Email = "admin@myp.local",
                Username = "admin",
                PasswordHash = HashPassword("Admin123!"),
                DisplayName = "Administrator",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Email = "demo@myp.local",
                Username = "demo",
                PasswordHash = HashPassword("Demo123!"),
                DisplayName = "Demo User",
                FirstName = "Demo",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
