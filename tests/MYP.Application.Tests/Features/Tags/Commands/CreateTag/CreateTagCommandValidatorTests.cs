using FluentAssertions;
using FluentValidation.TestHelper;
using MYP.Application.Features.Tags.Commands.CreateTag;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Features.Tags.Commands.CreateTag;

public class CreateTagCommandValidatorTests
{
    private readonly CreateTagCommandValidator _validator;

    public CreateTagCommandValidatorTests()
    {
        _validator = new CreateTagCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTagCommand(Name: "Valid Tag", Color: "#FF0000");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        // Arrange
        var command = new CreateTagCommand(Name: name!, Color: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: new string('a', Tag.NameMaxLength + 1),
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must not exceed {Tag.NameMaxLength} characters.");
    }

    [Fact]
    public void Validate_WithNullColor_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateTagCommand(Name: "Valid Tag", Color: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#ff0000")]
    [InlineData("#123ABC")]
    public void Validate_WithValidHexColor_ShouldNotHaveError(string color)
    {
        // Arrange
        var command = new CreateTagCommand(Name: "Valid Tag", Color: color);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    [Theory]
    [InlineData("FF0000")]       // Missing #
    [InlineData("#FFF")]         // Too short
    [InlineData("#FFFFFFF")]     // Too long
    [InlineData("#GGGGGG")]      // Invalid hex chars
    [InlineData("red")]          // Named color
    public void Validate_WithInvalidHexColor_ShouldHaveError(string color)
    {
        // Arrange
        var command = new CreateTagCommand(Name: "Valid Tag", Color: color);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Color)
            .WithErrorMessage("Color must be a valid hex color (e.g., #FF0000).");
    }
}
