using FluentAssertions;
using MYP.Domain.ValueObjects;

namespace MYP.Domain.Tests.ValueObjects;

public class PasswordTests
{
    [Theory]
    [InlineData("Password1!")]
    [InlineData("SecureP@ss123")]
    [InlineData("MyStr0ng#Pass")]
    [InlineData("C0mplex!ty")]
    public void Create_WithValidPassword_ShouldSucceed(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.Value.Should().Be(password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldThrow(string? password)
    {
        // Act
        var action = () => Password.Create(password!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Create_WithTooShort_ShouldThrow()
    {
        // Act
        var action = () => Password.Create("Aa1!");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*at least 8 characters*");
    }

    [Fact]
    public void Create_WithoutUppercase_ShouldThrow()
    {
        // Act
        var action = () => Password.Create("password1!");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*uppercase letter*");
    }

    [Fact]
    public void Create_WithoutLowercase_ShouldThrow()
    {
        // Act
        var action = () => Password.Create("PASSWORD1!");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*lowercase letter*");
    }

    [Fact]
    public void Create_WithoutDigit_ShouldThrow()
    {
        // Act
        var action = () => Password.Create("Password!");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*digit*");
    }

    [Fact]
    public void Create_WithoutSpecialChar_ShouldThrow()
    {
        // Act
        var action = () => Password.Create("Password1");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*special character*");
    }

    [Fact]
    public void TryCreate_WithValidPassword_ShouldReturnTrue()
    {
        // Act
        var success = Password.TryCreate("Password1!", out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void TryCreate_WithInvalidPassword_ShouldReturnFalseAndErrors()
    {
        // Act
        var success = Password.TryCreate("weak", out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Act
        var errors = Password.Validate("short");

        // Assert
        errors.Should().HaveCountGreaterThan(1);
        errors.Should().Contain(e => e.Contains("8 characters"));
        errors.Should().Contain(e => e.Contains("uppercase"));
        errors.Should().Contain(e => e.Contains("digit"));
        errors.Should().Contain(e => e.Contains("special"));
    }

    [Fact]
    public void IsValid_WithValidPassword_ShouldReturnTrue()
    {
        // Act
        var isValid = Password.IsValid("Password1!");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidPassword_ShouldReturnFalse()
    {
        // Act
        var isValid = Password.IsValid("weak");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldMaskPassword()
    {
        // Arrange
        var password = Password.Create("Password1!");

        // Act
        var result = password.ToString();

        // Assert
        result.Should().Be("********");
        result.Should().NotContain("Password1!");
    }
}
