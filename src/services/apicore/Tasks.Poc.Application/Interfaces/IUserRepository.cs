using Tasks.Poc.Domain.Common;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Application.Interfaces;

public interface IUserRepository : IRepository<User, EntityId>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
}