using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Snippets.Commands.CreateSnippet;

public class CreateSnippetCommandValidator : AbstractValidator<CreateSnippetCommand>
{
    private static readonly string[] SupportedLanguages =
    {
        "csharp", "typescript", "javascript", "sql", "python",
        "bash", "json", "yaml", "html", "css", "go", "rust", "java",
        "xml", "markdown", "plaintext"
    };

    public CreateSnippetCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(Snippet.TitleMaxLength)
            .WithMessage($"Title must not exceed {Snippet.TitleMaxLength} characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(Snippet.LanguageMaxLength)
            .WithMessage($"Language must not exceed {Snippet.LanguageMaxLength} characters.")
            .Must(BeValidLanguage)
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}");

        RuleFor(x => x.Description)
            .MaximumLength(Snippet.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Snippet.DescriptionMaxLength} characters.")
            .When(x => x.Description != null);
    }

    private static bool BeValidLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            return false;

        return SupportedLanguages.Contains(language.ToLowerInvariant());
    }
}
