using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tasks.Poc.Infrastructure.Persistence.Seeders;

namespace Tasks.Poc.Infrastructure.Persistence;

public class DevelopmentSeedingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevelopmentSeedingService> _logger;

    public DevelopmentSeedingService(
        IServiceProvider serviceProvider,
        ILogger<DevelopmentSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        try
        {
            _logger.LogInformation("Starting development database seeding...");

            // Ensure database is created (for development scenarios)
            await context.Database.EnsureCreatedAsync(cancellationToken);

            // Seed development data
            var developmentSeeder = scope.ServiceProvider.GetRequiredService<DevelopmentSeeder>();
            await developmentSeeder.SeedAsync(cancellationToken);

            _logger.LogInformation("Development database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding development data");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}