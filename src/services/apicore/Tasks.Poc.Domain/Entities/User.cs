namespace Tasks.Poc.Domain.Entities;

using Tasks.Poc.Domain.Events;
using Tasks.Poc.Domain.ValueObjects;
using Tasks.Poc.SharedKernel.Base;

public class User : AuditableEntity<EntityId>, IAggregateRoot, ISoftDelete
{
    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private readonly List<TodoList> _todoLists = [];
    public IReadOnlyList<TodoList> TodoLists => _todoLists.AsReadOnly();

    private User() { }

    private User(EntityId id, UserName name, Email email, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }

    public static User Create(UserName name, Email email)
    {
        var user = new User(EntityId.New(), name, email, DateTime.UtcNow);
        return user;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateName(UserName newName)
    {
        Name = newName;
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
    }

    public void Delete()
    {
        IsDeleted = true;
        RegisterDomainEvent(new UserDeletedEvent(Id));
    }
}
