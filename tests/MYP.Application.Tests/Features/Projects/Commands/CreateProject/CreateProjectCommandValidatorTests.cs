using FluentAssertions;
using FluentValidation.TestHelper;
using MYP.Application.Features.Projects.Commands.CreateProject;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Project",
            Description: "A valid description",
            Color: "#ff5733"
        );

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
        var command = new CreateProjectCommand(
            Name: name!,
            Description: null,
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name is required.");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: new string('a', Project.NameMaxLength + 1),
            Description: null,
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Project name must not exceed {Project.NameMaxLength} characters.");
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: new string('a', Project.DescriptionMaxLength + 1),
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage($"Description must not exceed {Project.DescriptionMaxLength} characters.");
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: null,
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ff5733")]
    [InlineData("#ff573")]
    [InlineData("#ff57333")]
    [InlineData("#gggggg")]
    public void Validate_WithInvalidColor_ShouldHaveError(string color)
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: null,
            Color: color
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Color)
            .WithErrorMessage("Color must be a valid hex color (e.g., #ff5733).");
    }

    [Theory]
    [InlineData("#ff5733")]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#123abc")]
    [InlineData("#ABC123")]
    public void Validate_WithValidColor_ShouldNotHaveError(string color)
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: null,
            Color: color
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyColor_ShouldNotHaveError(string? color)
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: null,
            Color: color
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    [Fact]
    public void Validate_WithNameAtMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: new string('a', Project.NameMaxLength),
            Description: null,
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithDescriptionAtMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Valid Name",
            Description: new string('a', Project.DescriptionMaxLength),
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
