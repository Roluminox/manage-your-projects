using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Tags.Commands.CreateTag;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Tag.NameMaxLength)
            .WithMessage($"Name must not exceed {Tag.NameMaxLength} characters.");

        RuleFor(x => x.Color)
            .MaximumLength(Tag.ColorMaxLength)
            .WithMessage($"Color must not exceed {Tag.ColorMaxLength} characters.")
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color (e.g., #FF0000).")
            .When(x => x.Color != null);
    }
}
