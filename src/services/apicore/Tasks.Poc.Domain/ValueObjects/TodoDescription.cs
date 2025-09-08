namespace Tasks.Poc.Domain.ValueObjects;

public record TodoDescription
{
    public string Value { get; }

    public TodoDescription(string value)
    {
        Value = ValidateAndTrim(value);
    }

    private static string ValidateAndTrim(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        
        if (trimmed.Length > 1000)
        {
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(value));
        }

        return trimmed;
    }

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;
    
    public static implicit operator TodoDescription(string? value) => new(value ?? string.Empty);
    public static implicit operator string(TodoDescription description) => description.Value;
}