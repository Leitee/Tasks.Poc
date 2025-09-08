using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                x => x.Value,
                x => new EntityId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                x => new UserName(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                x => x.Value,
                x => new Email(x))
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Add index on CreatedAt for performance
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        builder.Property(x => x.LastLoginAt);

        builder.HasMany<TodoList>()
            .WithOne()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}