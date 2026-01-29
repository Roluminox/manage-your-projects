using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Column name is required.")
            .MaximumLength(Column.NameMaxLength)
            .WithMessage($"Column name must not exceed {Column.NameMaxLength} characters.");
    }
}
