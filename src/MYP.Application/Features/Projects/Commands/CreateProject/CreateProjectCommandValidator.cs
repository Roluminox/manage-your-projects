using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(Project.NameMaxLength)
            .WithMessage($"Project name must not exceed {Project.NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Project.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Project.DescriptionMaxLength} characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color (e.g., #ff5733).")
            .When(x => !string.IsNullOrWhiteSpace(x.Color));
    }
}
