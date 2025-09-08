namespace Tasks.Poc.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.Infrastructure.Persistence;

public class TodoListRepository : Repository<TodoList, EntityId>, ITodoListRepository
{
    public TodoListRepository(TodoDbContext context) : base(context)
    {
    }

    public async Task<List<TodoList>> GetByUserIdAsync(EntityId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(tl => tl.OwnerId == userId)
            .Include(tl => tl.Items)
            .OrderByDescending(tl => tl.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoList?> GetByIdWithItemsAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tl => tl.Items)
            .FirstOrDefaultAsync(tl => tl.Id == id, cancellationToken);
    }

    public async Task<List<TodoList>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tl => tl.Items)
            .OrderByDescending(tl => tl.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public override async Task<TodoList?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        return await GetByIdWithItemsAsync(id, cancellationToken);
    }
}