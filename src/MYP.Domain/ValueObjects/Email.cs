using System.Text.RegularExpressions;

namespace MYP.Domain.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = GenerateEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var trimmedEmail = email.Trim();

        if (!IsValidFormat(trimmedEmail))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        return new Email(trimmedEmail);
    }

    public static bool TryCreate(string email, out Email? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(email))
            return false;

        var trimmedEmail = email.Trim();

        if (!IsValidFormat(trimmedEmail))
            return false;

        result = new Email(trimmedEmail);
        return true;
    }

    public static bool IsValidFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email);
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    public static bool operator ==(Email? left, Email? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Email? left, Email? right) => !(left == right);

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex GenerateEmailRegex();
}
