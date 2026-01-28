using FluentAssertions;
using MYP.Domain.ValueObjects;

namespace MYP.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.com")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM", "test@example.com")]
    [InlineData("User@Domain.Com", "user@domain.com")]
    public void Create_ShouldNormalizeToLowercase(string input, string expected)
    {
        // Act
        var result = Email.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldThrow(string? email)
    {
        // Act
        var action = () => Email.Create(email!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    [InlineData("invalid@domain")]
    [InlineData("invalid@.com")]
    [InlineData("invalid@domain.")]
    public void Create_WithInvalidFormat_ShouldThrow(string email)
    {
        // Act
        var action = () => Email.Create(email);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*format is invalid*");
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnTrue()
    {
        // Act
        var success = Email.TryCreate("test@example.com", out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ShouldReturnFalse()
    {
        // Act
        var success = Email.TryCreate("invalid", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldBeTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldBeFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }
}
