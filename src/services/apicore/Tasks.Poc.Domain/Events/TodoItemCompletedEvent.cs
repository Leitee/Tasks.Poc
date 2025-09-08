using Tasks.Poc.Domain.Common;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Domain.Events;

public record TodoItemCompletedEvent(
    EntityId TodoListId,
    EntityId TodoItemId,
    DateTime OccurredOn) : IDomainEvent;