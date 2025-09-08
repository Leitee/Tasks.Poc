namespace Tasks.Poc.Domain.ValueObjects;

public record TodoTitle
{
    public string Value { get; }

    public TodoTitle(string value)
    {
        Value = ValidateAndTrim(value);
    }

    private static string ValidateAndTrim(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Todo title cannot be null or empty.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length < 1)
        {
            throw new ArgumentException("Todo title must be at least 1 character long.", nameof(value));
        }
        
        if (trimmed.Length > 200)
        {
            throw new ArgumentException("Todo title cannot exceed 200 characters.", nameof(value));
        }

        return trimmed;
    }

    public override string ToString() => Value;
    
    public static implicit operator TodoTitle(string value) => new(value);
    public static implicit operator string(TodoTitle title) => title.Value;
}