namespace Tasks.Poc.Domain.ValueObjects;

public record UserName
{
    public string Value { get; }

    public UserName(string value)
    {
        Value = ValidateAndTrim(value);
    }

    private static string ValidateAndTrim(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("User name cannot be null or empty.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length < 2)
        {
            throw new ArgumentException("User name must be at least 2 characters long.", nameof(value));
        }
        
        if (trimmed.Length > 100)
        {
            throw new ArgumentException("User name cannot exceed 100 characters.", nameof(value));
        }

        return trimmed;
    }

    public override string ToString() => Value;
    
    public static implicit operator UserName(string value) => new(value);
    public static implicit operator string(UserName userName) => userName.Value;
}