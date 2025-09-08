namespace Tasks.Poc.Infrastructure.Persistence.Seeders;

using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasks.Poc.Domain.Entities;

public class DevelopmentSeeder : IDbSeeder
{
    private readonly TodoDbContext _context;
    private readonly ILogger<DevelopmentSeeder> _logger;

    public DevelopmentSeeder(TodoDbContext context, ILogger<DevelopmentSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting development data seeding...");

        if (await _context.Users.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Database already contains data. Skipping development seeding.");
            return;
        }

        var users = await SeedUsersAsync(cancellationToken);
        await SeedTodoListsAsync(users, cancellationToken);

        _logger.LogInformation("Development data seeding completed successfully.");
    }

    private async Task<List<User>> SeedUsersAsync(CancellationToken cancellationToken)
    {
        var userFaker = new Faker<User>()
            .CustomInstantiator(f => User.Create(
                f.Internet.UserName(),
                f.Internet.Email()
            ));

        // Generate 10-15 random users
        var users = userFaker.Generate(new Faker().Random.Int(10, 15));
        
        // Add a few specific users for testing
        users.Add(User.Create("System Administrator", "admin@twarz.com"));
        users.Add(User.Create("Demo User", "demo@example.com"));
        users.Add(User.Create("John Doe", "john.doe@test.com"));

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Seeded {Count} development users", users.Count);
        return users;
    }

    private async Task SeedTodoListsAsync(List<User> users, CancellationToken cancellationToken)
    {
        var faker = new Faker();
        var todoLists = new List<TodoList>();

        var todoListCategories = new[]
        {
            "Work", "Personal", "Shopping", "Health", "Travel", "Learning", 
            "Home", "Finance", "Projects", "Goals", "Ideas", "Errands"
        };

        var workTasks = new[]
        {
            "Review pull requests", "Update documentation", "Attend team standup", 
            "Deploy to staging", "Fix bug in authentication", "Write unit tests",
            "Refactor legacy code", "Schedule team meeting", "Update dependencies",
            "Code review for junior developer", "Plan sprint backlog", "Database optimization"
        };

        var personalTasks = new[]
        {
            "Grocery shopping", "Exercise for 30 minutes", "Call family",
            "Read for 1 hour", "Meal prep for the week", "Clean the house",
            "Pay bills", "Schedule doctor appointment", "Walk the dog",
            "Learn new skill", "Organize closet", "Plan weekend activities"
        };

        var shoppingTasks = new[]
        {
            "Buy groceries", "Get new shoes", "Replace phone charger",
            "Buy birthday gift", "Get car serviced", "Purchase office supplies",
            "Buy winter clothes", "Get prescription refilled", "Buy home decor"
        };

        var tasksByCategory = new Dictionary<string, string[]>
        {
            { "Work", workTasks },
            { "Personal", personalTasks },
            { "Shopping", shoppingTasks }
        };

        // Generate 2-5 todo lists per user
        foreach (var user in users)
        {
            var listsCount = faker.Random.Int(2, 5);
            
            for (int i = 0; i < listsCount; i++)
            {
                var category = faker.PickRandom(todoListCategories);
                var firstName = ((string)user.Name).Split(' ')[0];
                var listName = $"{firstName}'s {category} Tasks";
                
                var todoList = TodoList.Create(listName, null, user.Id);

                // Add 3-8 random items to each list
                var itemsCount = faker.Random.Int(3, 8);
                var availableTasks = tasksByCategory.ContainsKey(category) 
                    ? tasksByCategory[category] 
                    : personalTasks;

                var selectedTasks = faker.PickRandom(availableTasks, Math.Min(itemsCount, availableTasks.Length));
                
                foreach (var task in selectedTasks)
                {
                    todoList.AddItem(task);
                }

                // Randomly complete some items (20-60% chance per item)
                foreach (var item in todoList.Items)
                {
                    if (faker.Random.Bool(0.4f)) // 40% chance to be completed
                    {
                        item.Complete();
                    }
                }

                todoLists.Add(todoList);
            }
        }

        _context.TodoLists.AddRange(todoLists);
        await _context.SaveChangesAsync(cancellationToken);
        
        var totalItems = todoLists.Sum(tl => tl.Items.Count);
        var completedItems = todoLists.Sum(tl => tl.Items.Count(i => i.IsCompleted));
        
        _logger.LogInformation(
            "Seeded {ListCount} todo lists with {ItemCount} total items ({CompletedCount} completed)", 
            todoLists.Count, totalItems, completedItems);
    }
}