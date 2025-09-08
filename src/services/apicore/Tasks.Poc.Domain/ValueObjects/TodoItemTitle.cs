namespace Tasks.Poc.Domain.ValueObjects;

public record TodoItemTitle
{
    public string Value { get; }

    public TodoItemTitle(string value)
    {
        Value = ValidateAndTrim(value);
    }

    private static string ValidateAndTrim(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Todo item title cannot be null or empty.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length < 1)
        {
            throw new ArgumentException("Todo item title must be at least 1 character long.", nameof(value));
        }
        
        if (trimmed.Length > 300)
        {
            throw new ArgumentException("Todo item title cannot exceed 300 characters.", nameof(value));
        }

        return trimmed;
    }

    public override string ToString() => Value;
    
    public static implicit operator TodoItemTitle(string value) => new(value);
    public static implicit operator string(TodoItemTitle title) => title.Value;
}