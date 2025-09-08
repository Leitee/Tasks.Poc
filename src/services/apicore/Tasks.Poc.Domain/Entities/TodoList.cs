namespace Tasks.Poc.Domain.Entities;

using Tasks.Poc.Domain.Common;
using Tasks.Poc.Domain.Enums;
using Tasks.Poc.Domain.Events;
using Tasks.Poc.Domain.ValueObjects;

public class TodoList : AggregateRoot<EntityId>
{
    public TodoTitle Title { get; private set; }
    public TodoDescription? Description { get; private set; }
    public EntityId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<TodoItem> _items = [];
    public IReadOnlyList<TodoItem> Items => _items.AsReadOnly();

    private TodoList() { }

    private TodoList(
        EntityId id,
        TodoTitle title,
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

    public static TodoList Create(TodoTitle title, TodoDescription? description, EntityId ownerId)
    {
        var todoList = new TodoList(EntityId.New(), title, description, ownerId, DateTime.UtcNow);
        
        todoList.RaiseDomainEvent(new TodoListCreatedEvent(
            todoList.Id,
            ownerId,
            title,
            DateTime.UtcNow));

        return todoList;
    }

    public void UpdateTitle(TodoTitle newTitle)
    {
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(TodoDescription? newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public EntityId AddItem(TodoItemTitle title, TodoDescription? description = null, Priority priority = Priority.Medium, DateTime? dueDate = null)
    {
        var item = TodoItem.Create(title, description, priority, dueDate);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
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
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TodoItemCompletedEvent(Id, itemId, DateTime.UtcNow));
    }

    public void ReopenItem(EntityId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        item.Reopen();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(EntityId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Todo item with ID {itemId} not found.");
        }

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItem(EntityId itemId, TodoItemTitle? title = null, TodoDescription? description = null, Priority? priority = null, DateTime? dueDate = null)
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
        
        UpdatedAt = DateTime.UtcNow;
    }

    public int TotalItems => _items.Count;
    public int CompletedItems => _items.Count(i => i.IsCompleted);
    public int PendingItems => _items.Count(i => !i.IsCompleted);
    public int OverdueItems => _items.Count(i => i.IsOverdue);
    
    public bool IsCompleted => TotalItems > 0 && CompletedItems == TotalItems;
    public double CompletionPercentage => TotalItems == 0 ? 0 : (double)CompletedItems / TotalItems * 100;
}