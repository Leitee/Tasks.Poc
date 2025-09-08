namespace Tasks.Poc.Domain.Events;

using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.SharedKernel.Base;
using Tasks.Poc.SharedKernel.Helpers;

public sealed record TodoListCreatedEvent(
    EntityId TodoListId,
    EntityId OwnerId,
    Title Title) : IDomainEvent
{
    public EntityId TodoListId { get; } = TodoListId;
    public EntityId OwnerId { get; } = OwnerId;
    public Title Title { get; } = Title;
    public DateTime DateOccurred { get; } = DateTimeHelper.UtcNow();
}
