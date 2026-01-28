using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MYP.Domain.Entities;
using MYP.Infrastructure.Persistence;

namespace MYP.Infrastructure.Tests.Persistence.Configurations;

public class UserConfigurationTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public UserConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void UserConfiguration_ShouldHaveCorrectTableName()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Users");
    }

    [Fact]
    public void UserConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var primaryKey = entityType?.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void UserConfiguration_Email_ShouldBeRequired()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var emailProperty = entityType?.FindProperty("Email");

        // Assert
        emailProperty.Should().NotBeNull();
        emailProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_Email_ShouldHaveMaxLength()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var emailProperty = entityType?.FindProperty("Email");

        // Assert
        emailProperty.Should().NotBeNull();
        emailProperty!.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void UserConfiguration_Email_ShouldHaveUniqueIndex()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var emailIndex = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Email"));

        // Assert
        emailIndex.Should().NotBeNull();
        emailIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_Username_ShouldBeRequired()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var usernameProperty = entityType?.FindProperty("Username");

        // Assert
        usernameProperty.Should().NotBeNull();
        usernameProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_Username_ShouldHaveMaxLength()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var usernameProperty = entityType?.FindProperty("Username");

        // Assert
        usernameProperty.Should().NotBeNull();
        usernameProperty!.GetMaxLength().Should().Be(50);
    }

    [Fact]
    public void UserConfiguration_Username_ShouldHaveUniqueIndex()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var usernameIndex = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Username"));

        // Assert
        usernameIndex.Should().NotBeNull();
        usernameIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_PasswordHash_ShouldBeRequired()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var passwordHashProperty = entityType?.FindProperty("PasswordHash");

        // Assert
        passwordHashProperty.Should().NotBeNull();
        passwordHashProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_DisplayName_ShouldBeRequired()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var displayNameProperty = entityType?.FindProperty("DisplayName");

        // Assert
        displayNameProperty.Should().NotBeNull();
        displayNameProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_DisplayName_ShouldHaveMaxLength()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var displayNameProperty = entityType?.FindProperty("DisplayName");

        // Assert
        displayNameProperty.Should().NotBeNull();
        displayNameProperty!.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void UserConfiguration_AvatarUrl_ShouldBeOptional()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(User));
        var avatarUrlProperty = entityType?.FindProperty("AvatarUrl");

        // Assert
        avatarUrlProperty.Should().NotBeNull();
        avatarUrlProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public async Task UserConfiguration_ShouldAllowSavingValidUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_password",
            DisplayName = "Test User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("test@example.com");
    }
}
