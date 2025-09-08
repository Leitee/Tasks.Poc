using Tasks.Poc.Domain.Common;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Domain.Events;

public record TodoListCreatedEvent(
    EntityId TodoListId,
    EntityId OwnerId,
    TodoTitle Title,
    DateTime OccurredOn) : IDomainEvent;