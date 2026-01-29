using FluentValidation;
using MYP.Domain.Entities;

namespace MYP.Application.Features.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Column ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Column name is required.")
            .MaximumLength(Column.NameMaxLength)
            .WithMessage($"Column name must not exceed {Column.NameMaxLength} characters.");
    }
}
