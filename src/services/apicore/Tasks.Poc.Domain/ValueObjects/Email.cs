using System.Text.RegularExpressions;

namespace Tasks.Poc.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        Value = ValidateAndNormalize(value);
    }

    private static string ValidateAndNormalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        
        if (!EmailRegex.IsMatch(normalized))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }

        return normalized;
    }

    public override string ToString() => Value;
    
    public static implicit operator Email(string value) => new(value);
    public static implicit operator string(Email email) => email.Value;
}