using FluentAssertions;
using FluentValidation.TestHelper;
using MYP.Application.Features.Auth.Commands.Register;

namespace MYP.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Email Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: email!,
            Username: "testuser",
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: email,
            Username: "testuser",
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email format is invalid.");
    }

    #endregion

    #region Username Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyUsername_ShouldHaveError(string? username)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: username!,
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Validate_WithTooShortUsername_ShouldHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "ab",
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must be at least 3 characters.");
    }

    [Fact]
    public void Validate_WithTooLongUsername_ShouldHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: new string('a', 51),
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must not exceed 50 characters.");
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user@name")]
    [InlineData("user.name")]
    public void Validate_WithInvalidUsernameCharacters_ShouldHaveError(string username)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: username,
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username can only contain letters, numbers, underscores and hyphens.");
    }

    [Theory]
    [InlineData("validuser")]
    [InlineData("valid_user")]
    [InlineData("valid-user")]
    [InlineData("ValidUser123")]
    public void Validate_WithValidUsername_ShouldNotHaveError(string username)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: username,
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    #endregion

    #region Password Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string? password)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: password!,
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigits!!")]
    [InlineData("NoSpecial1")]
    public void Validate_WithWeakPassword_ShouldHaveError(string password)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: password,
            DisplayName: "Test User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region DisplayName Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyDisplayName_ShouldHaveError(string? displayName)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1!",
            DisplayName: displayName!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DisplayName)
            .WithErrorMessage("Display name is required.");
    }

    [Fact]
    public void Validate_WithTooShortDisplayName_ShouldHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1!",
            DisplayName: "A"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DisplayName)
            .WithErrorMessage("Display name must be at least 2 characters.");
    }

    [Fact]
    public void Validate_WithTooLongDisplayName_ShouldHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1!",
            DisplayName: new string('a', 101)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DisplayName)
            .WithErrorMessage("Display name must not exceed 100 characters.");
    }

    #endregion
}
