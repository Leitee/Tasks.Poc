namespace Tasks.Poc.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Infrastructure.Persistence;
using Tasks.Poc.Infrastructure.Persistence.Seeders;
using Tasks.Poc.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Database Configuration
        services.AddDbContext<TodoDbContext>(options =>
        {
            var aspireConnectionString = configuration.GetConnectionString("todosdb");
            options.UseNpgsql(aspireConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("TwarzPocMigrationsHistory", "system");
            });
            
            // Enable sensitive data logging only in development
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        
        // Repositories (kept for backward compatibility during transition)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITodoListRepository, TodoListRepository>();

        // Database Seeders - only in development
        if (builder.Environment.IsDevelopment())
        {
            services.AddScoped<DevelopmentSeeder>();
            services.AddHostedService<DevelopmentSeedingService>();
        }

        return services;
    }
}