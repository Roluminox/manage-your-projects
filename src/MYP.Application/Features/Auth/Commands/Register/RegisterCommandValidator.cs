using FluentValidation;
using MYP.Domain.ValueObjects;

namespace MYP.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(BeValidEmail).WithMessage("Email format is invalid.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores and hyphens.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Must(BeValidPassword).WithMessage(GetPasswordRequirements);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
    }

    private static bool BeValidEmail(string email)
    {
        return Email.IsValidFormat(email);
    }

    private static bool BeValidPassword(string password)
    {
        return Password.IsValid(password);
    }

    private static string GetPasswordRequirements(RegisterCommand command)
    {
        var errors = Password.Validate(command.Password);
        return string.Join(" ", errors);
    }
}
