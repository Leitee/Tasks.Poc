namespace Tasks.Poc.Domain.Entities;

using Tasks.Poc.Domain.Enums;
using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.SharedKernel.Base;

public class TodoItem : Entity<EntityId>
{
    public Title Title { get; private set; }
    public TodoDescription? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DueDate { get; private set; }

    private TodoItem() { }

    private TodoItem(
        EntityId id,
        Title title,
        TodoDescription? description,
        Priority priority,
        DateTime createdAt,
        DateTime? dueDate = null)
    {
        Id = id;
        Title = title;
        Description = description;
        Priority = priority;
        IsCompleted = false;
        CreatedAt = createdAt;
        DueDate = dueDate;
    }

    public static TodoItem Create(
        Title title,
        TodoDescription? description = null,
        Priority priority = Priority.Medium,
        DateTime? dueDate = null)
    {
        return new TodoItem(EntityId.New(), title, description, priority, DateTime.UtcNow, dueDate);
    }

    public void Complete()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (!IsCompleted)
        {
            return;
        }

        IsCompleted = false;
        CompletedAt = null;
    }

    public void UpdateTitle(Title newTitle)
    {
        Title = newTitle;
    }

    public void UpdateDescription(TodoDescription? newDescription)
    {
        Description = newDescription;
    }

    public void UpdatePriority(Priority newPriority)
    {
        Priority = newPriority;
    }

    public void UpdateDueDate(DateTime? newDueDate)
    {
        DueDate = newDueDate;
    }

    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsCompleted;
}
