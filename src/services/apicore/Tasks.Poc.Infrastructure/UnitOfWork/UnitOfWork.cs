namespace Tasks.Poc.Infrastructure.UnitOfWork;

using Microsoft.EntityFrameworkCore.Storage;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Infrastructure.Persistence;
using Tasks.Poc.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TodoDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IUserRepository? _userRepository;
    private ITodoListRepository? _todoListRepository;

    public UnitOfWork(TodoDbContext context)
    {
        _context = context;
    }

    public IUserRepository UserRepository => 
        _userRepository ??= new UserRepository(_context);

    public ITodoListRepository TodoListRepository => 
        _todoListRepository ??= new TodoListRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}