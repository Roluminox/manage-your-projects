using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Id.Should().Be(Guid.Empty);
        user.Email.Should().BeEmpty();
        user.Username.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.IsActive.Should().BeTrue();
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void User_ShouldSetProperties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var now = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = id,
            Email = email,
            Username = username,
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            CreatedAt = now
        };

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.Username.Should().Be(username);
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.CreatedAt.Should().Be(now);
    }
}
