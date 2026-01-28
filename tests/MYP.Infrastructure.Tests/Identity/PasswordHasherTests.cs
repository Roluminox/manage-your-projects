using FluentAssertions;
using MYP.Infrastructure.Identity;

namespace MYP.Infrastructure.Tests.Identity;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    #region Hash Tests

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ShouldReturnBCryptHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void Hash_ShouldGenerateDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Hash_ShouldGenerateDifferentHashesForDifferentPasswords()
    {
        // Arrange
        var password1 = "TestPassword123!";
        var password2 = "DifferentPassword456!";

        // Act
        var hash1 = _passwordHasher.Hash(password1);
        var hash2 = _passwordHasher.Hash(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongCasePassword = "testpassword123!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongCasePassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("WithUpperAndLower")]
    [InlineData("with spaces in password")]
    [InlineData("特殊字符密码")]
    [InlineData("émojis🔐🔑")]
    public void HashAndVerify_WithVariousPasswords_ShouldWork(string password)
    {
        // Act
        var hash = _passwordHasher.Hash(password);
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
