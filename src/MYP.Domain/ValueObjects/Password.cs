using System.Text.RegularExpressions;

namespace MYP.Domain.ValueObjects;

public sealed partial class Password
{
    public const int MinLength = 8;

    private static readonly Regex UppercaseRegex = GenerateUppercaseRegex();
    private static readonly Regex LowercaseRegex = GenerateLowercaseRegex();
    private static readonly Regex DigitRegex = GenerateDigitRegex();
    private static readonly Regex SpecialCharRegex = GenerateSpecialCharRegex();

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        var errors = Validate(password);
        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors), nameof(password));

        return new Password(password);
    }

    public static bool TryCreate(string password, out Password? result, out IReadOnlyList<string> errors)
    {
        result = null;
        var validationErrors = Validate(password);
        errors = validationErrors;

        if (validationErrors.Count > 0)
            return false;

        result = new Password(password);
        return true;
    }

    public static IReadOnlyList<string> Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password cannot be null or empty.");
            return errors;
        }

        if (password.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters long.");

        if (!UppercaseRegex.IsMatch(password))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!LowercaseRegex.IsMatch(password))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!DigitRegex.IsMatch(password))
            errors.Add("Password must contain at least one digit.");

        if (!SpecialCharRegex.IsMatch(password))
            errors.Add("Password must contain at least one special character.");

        return errors;
    }

    public static bool IsValid(string password) => Validate(password).Count == 0;

    public override string ToString() => "********";

    [GeneratedRegex(@"[A-Z]", RegexOptions.Compiled)]
    private static partial Regex GenerateUppercaseRegex();

    [GeneratedRegex(@"[a-z]", RegexOptions.Compiled)]
    private static partial Regex GenerateLowercaseRegex();

    [GeneratedRegex(@"[0-9]", RegexOptions.Compiled)]
    private static partial Regex GenerateDigitRegex();

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.Compiled)]
    private static partial Regex GenerateSpecialCharRegex();
}
