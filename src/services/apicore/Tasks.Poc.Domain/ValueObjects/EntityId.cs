namespace Tasks.Poc.Domain.ValueObjects;

public record EntityId(Guid Value)
{
    public static EntityId New() => new(Guid.CreateVersion7());
    public static EntityId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator EntityId(Guid value) => new(value);
    public static implicit operator Guid(EntityId entityId) => entityId.Value;
}