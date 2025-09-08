using Tasks.Poc.Domain.Common;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Application.Interfaces;

public interface ITodoListRepository : IRepository<TodoList, EntityId>
{
    Task<List<TodoList>> GetByUserIdAsync(EntityId userId, CancellationToken cancellationToken = default);
    Task<TodoList?> GetByIdWithItemsAsync(EntityId id, CancellationToken cancellationToken = default);
    Task<List<TodoList>> GetAllAsync(CancellationToken cancellationToken = default);
}