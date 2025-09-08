namespace Tasks.Poc.Domain.Entities;

using Tasks.Poc.Domain.Enums;
using Tasks.Poc.Domain.Events;
using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.SharedKernel.Base;

public class TodoList : AuditableEntity<EntityId>, IAggregateRoot
{
    public Title Title { get; private set; }
    public TodoDescription? Description { get; private set; }
    public EntityId OwnerId { get; private set; }

    private readonly List<TodoItem> _items = [];
    public IReadOnlyList<TodoItem> Items => _items.AsReadOnly();

    private TodoList() { }

    private TodoList(
        EntityId id,
        Title title,
        TodoDescription? description,
        EntityId ownerId,
        DateTime createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        OwnerId = ownerId;
        CreatedAt = createdAt;
    }

    public static TodoList Create(Title title, TodoDescription? description, EntityId ownerId)
    {
        var todoList = new TodoList(EntityId.New(), title, description, ownerId, DateTime.UtcNow);
        
        todoList.RegisterDomainEvent(new TodoListCreatedEvent(
            todoList.Id,
            ownerId,
            title));

        return todoList;
    }

    public void UpdateTitle(Title newTitle)
    {
        Title = newTitle;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(TodoDescription? newDescription)
    {
        Description = newDescription;
        LastModifiedAt = DateTime.UtcNow;
    }

    public EntityId AddItem(Title title, TodoDescription? description = null, Priority priority = Priority.Medium, DateTime? dueDate = null)
    {
        var item = TodoItem.Create(title, description, priority, dueDate);
        _items.Add(item);
        LastModifiedAt = DateTime.UtcNow;
        return item.Id;
    }

    public void CompleteItem(EntityId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        item.Complete();
        LastModifiedAt = DateTime.UtcNow;

        RegisterDomainEvent(new TodoItemCompletedEvent(Id, itemId));
    }

    public void ReopenItem(EntityId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        item.Reopen();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveItem(EntityId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        _items.Remove(item);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateItem(EntityId itemId, Title? title = null, TodoDescription? description = null, Priority? priority = null, DateTime? dueDate = null)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        if (title != null)
        {
            item.UpdateTitle(title);
        }
        
        if (description != null)
        {
            item.UpdateDescription(description);
        }
        
        if (priority.HasValue)
        {
            item.UpdatePriority(priority.Value);
        }
        
        item.UpdateDueDate(dueDate);
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public int TotalItems => _items.Count;
    public int CompletedItems => _items.Count(i => i.IsCompleted);
    public int PendingItems => _items.Count(i => !i.IsCompleted);
    public int OverdueItems => _items.Count(i => i.IsOverdue);
    
    public bool IsCompleted => TotalItems > 0 && CompletedItems == TotalItems;
    public double CompletionPercentage => TotalItems == 0 ? 0 : (double)CompletedItems / TotalItems * 100;
}
