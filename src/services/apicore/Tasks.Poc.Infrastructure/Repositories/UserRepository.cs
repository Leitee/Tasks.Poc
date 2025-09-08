namespace Tasks.Poc.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.Infrastructure.Persistence;

public class UserRepository : Repository<User, EntityId>, IUserRepository
{
    public UserRepository(TodoDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
                .OrderBy(u => u.Name)
                .ToListAsync(cancellationToken);
}