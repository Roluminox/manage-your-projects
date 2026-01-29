using FluentValidation.TestHelper;
using MYP.Application.Features.Columns.Commands.CreateColumn;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandValidatorTests
{
    private readonly CreateColumnCommandValidator _validator;

    public CreateColumnCommandValidatorTests()
    {
        _validator = new CreateColumnCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateColumnCommand(
            ProjectId: Guid.NewGuid(),
            Name: "To Do"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyProjectId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateColumnCommand(
            ProjectId: Guid.Empty,
            Name: "To Do"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectId)
            .WithErrorMessage("Project ID is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        // Arrange
        var command = new CreateColumnCommand(
            ProjectId: Guid.NewGuid(),
            Name: name!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Column name is required.");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateColumnCommand(
            ProjectId: Guid.NewGuid(),
            Name: new string('a', Column.NameMaxLength + 1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Column name must not exceed {Column.NameMaxLength} characters.");
    }
}
