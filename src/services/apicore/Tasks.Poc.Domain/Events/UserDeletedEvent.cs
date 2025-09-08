namespace Tasks.Poc.Domain.Events;

using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.SharedKernel.Base;
using Tasks.Poc.SharedKernel.Helpers;

public sealed record UserDeletedEvent(EntityId UserId) : IDomainEvent
{
    public EntityId UserId { get; } = UserId;
    public DateTime DateOccurred { get; } = DateTimeHelper.UtcNow();
}
