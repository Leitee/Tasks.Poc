namespace Tasks.Poc.SharedKernel.Base;

public interface IDomainEvent
{
    public DateTime DateOccurred { get; }
}
