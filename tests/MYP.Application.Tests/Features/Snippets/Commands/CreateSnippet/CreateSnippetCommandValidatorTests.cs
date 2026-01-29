using FluentAssertions;
using FluentValidation.TestHelper;
using MYP.Application.Features.Snippets.Commands.CreateSnippet;
using MYP.Domain.Entities;

namespace MYP.Application.Tests.Features.Snippets.Commands.CreateSnippet;

public class CreateSnippetCommandValidatorTests
{
    private readonly CreateSnippetCommandValidator _validator;

    public CreateSnippetCommandValidatorTests()
    {
        _validator = new CreateSnippetCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: "A valid description",
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyTitle_ShouldHaveError(string? title)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: title!,
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: new string('a', Snippet.TitleMaxLength + 1),
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage($"Title must not exceed {Snippet.TitleMaxLength} characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyCode_ShouldHaveError(string? code)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: code!,
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyLanguage_ShouldHaveError(string? language)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: language!,
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language)
            .WithErrorMessage("Language is required.");
    }

    [Fact]
    public void Validate_WithLanguageExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: new string('a', Snippet.LanguageMaxLength + 1),
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Theory]
    [InlineData("unsupported")]
    [InlineData("perl")]
    [InlineData("ruby")]
    [InlineData("INVALID")]
    public void Validate_WithUnsupportedLanguage_ShouldHaveError(string language)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: language,
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("typescript")]
    [InlineData("javascript")]
    [InlineData("sql")]
    [InlineData("python")]
    [InlineData("bash")]
    [InlineData("json")]
    [InlineData("yaml")]
    [InlineData("html")]
    [InlineData("css")]
    [InlineData("go")]
    [InlineData("rust")]
    [InlineData("java")]
    [InlineData("xml")]
    [InlineData("markdown")]
    [InlineData("plaintext")]
    public void Validate_WithSupportedLanguage_ShouldNotHaveError(string language)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "some code",
            Language: language,
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Theory]
    [InlineData("CSharp")]
    [InlineData("TYPESCRIPT")]
    [InlineData("JavaScript")]
    public void Validate_WithDifferentCasingLanguage_ShouldNotHaveError(string language)
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "some code",
            Language: language,
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: new string('a', Snippet.DescriptionMaxLength + 1),
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage($"Description must not exceed {Snippet.DescriptionMaxLength} characters.");
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithValidDescriptionAtMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateSnippetCommand(
            Title: "Valid Title",
            Code: "console.log('Hello');",
            Language: "javascript",
            Description: new string('a', Snippet.DescriptionMaxLength),
            TagIds: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
