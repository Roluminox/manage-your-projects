using FluentValidation;

namespace MYP.Application.Features.Columns.Commands.ReorderColumns;

public class ReorderColumnsCommandValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.ColumnIds)
            .NotEmpty().WithMessage("Column IDs are required.");
    }
}
