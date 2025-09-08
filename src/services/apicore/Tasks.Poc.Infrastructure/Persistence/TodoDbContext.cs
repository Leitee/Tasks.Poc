using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Tasks.Poc.Domain.Entities;

namespace Tasks.Poc.Infrastructure.Persistence;

public class TodoDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TodoList> TodoLists { get; set; } = null!;
    public DbSet<TodoItem> TodoItems { get; set; } = null!;

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will only be used if no options are provided (for EF Tools)
            optionsBuilder.UseNpgsql(options => 
            {
                options.MigrationsHistoryTable("TwarzPocMigrationsHistory", "system");
            });
        }
        
        base.OnConfiguring(optionsBuilder);
    }
}