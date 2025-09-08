using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;

namespace Tasks.Poc.Infrastructure.Persistence.Configurations;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        builder.ToTable("TodoLists");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                x => x.Value,
                x => new EntityId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasConversion(
                x => x.Value,
                x => new Title(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                x => x!.Value,
                x => string.IsNullOrEmpty(x) ? null : new TodoDescription(x))
            .HasMaxLength(1000);

        builder.Property(x => x.OwnerId)
            .HasConversion(
                x => x.Value,
                x => new EntityId(x))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastModifiedAt);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("TodoListId")
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.TotalItems);
        builder.Ignore(x => x.CompletedItems);
        builder.Ignore(x => x.PendingItems);
        builder.Ignore(x => x.OverdueItems);
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.CompletionPercentage);

        builder.Ignore(x => x.DomainEvents);
    }
}