namespace Tasks.Poc.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Poc.Domain.Entities;
using Tasks.Poc.Domain.ValueObjects;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");

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
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                x => x!.Value,
                x => string.IsNullOrEmpty(x) ? null : new TodoDescription(x))
            .HasMaxLength(1000);

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt);

        builder.Property(x => x.DueDate);

        builder.Property<EntityId>("TodoListId")
            .HasConversion(
                x => x.Value,
                x => new EntityId(x))
            .IsRequired();

        builder.Ignore(x => x.IsOverdue);
    }
}
