namespace Tasks.Poc.Infrastructure.Persistence;

public interface IDbSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}