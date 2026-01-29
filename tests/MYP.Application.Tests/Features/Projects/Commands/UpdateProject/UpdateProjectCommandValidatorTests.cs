using FluentAssertions;
using FluentValidation.TestHelper;
using MYP.Application.Features.Projects.Commands.UpdateProject;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidatorTests
{
    private readonly UpdateProjectCommandValidator _validator;

    public UpdateProjectCommandValidatorTests()
    {
        _validator = new UpdateProjectCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
            Name: "Valid Project",
            Description: "A valid description",
            Color: "#ff5733"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateProjectCommand(
            Id: Guid.Empty,
            Name: "Valid Name",
            Description: null,
            Color: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Project ID is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        // Arrange
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
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
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
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
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
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

    [Theory]
    [InlineData("invalid")]
    [InlineData("ff5733")]
    [InlineData("#ff573")]
    public void Validate_WithInvalidColor_ShouldHaveError(string color)
    {
        // Arrange
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
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
    public void Validate_WithValidColor_ShouldNotHaveError(string color)
    {
        // Arrange
        var command = new UpdateProjectCommand(
            Id: Guid.NewGuid(),
            Name: "Valid Name",
            Description: null,
            Color: color
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }
}
