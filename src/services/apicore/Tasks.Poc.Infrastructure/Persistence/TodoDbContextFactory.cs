using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tasks.Poc.Infrastructure.Persistence;

public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        
        // Use a temporary connection string for design-time operations
        optionsBuilder.UseNpgsql("Host=localhost;Database=design_time_temp;Username=postgres;Password=password", 
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("TwarzPocMigrationsHistory", "system");
            });

        return new TodoDbContext(optionsBuilder.Options);
    }
}