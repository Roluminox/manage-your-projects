using FluentValidation.TestHelper;
using MYP.Application.Features.Auth.Commands.Login;

namespace MYP.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "test@example.com",
            Password: "Password1!"
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
        var command = new LoginCommand(
            Email: email!,
            Password: "Password1!"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    #endregion

    #region Password Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string? password)
    {
        // Arrange
        var command = new LoginCommand(
            Email: "test@example.com",
            Password: password!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    #endregion
}
