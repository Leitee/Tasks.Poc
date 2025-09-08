using Tasks.Poc.Contracts.DTOs;

namespace Tasks.Poc.Web;

public class TodoApiClient(HttpClient httpClient)
{
    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<UserDto>>("/api/users", cancellationToken) ?? [];
    }

    public async Task<Guid> CreateUserAsync(string name, string email, CancellationToken cancellationToken = default)
    {
        var request = new { Name = name, Email = email };
        var response = await httpClient.PostAsJsonAsync("/api/users", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task<List<TodoListDto>> GetUserTodoListsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<TodoListDto>>($"/api/users/{userId}/todos", cancellationToken) ?? [];
    }

    public async Task<Guid> CreateTodoListAsync(Guid userId, string title, string? description, CancellationToken cancellationToken = default)
    {
        var request = new { Title = title, Description = description };
        var response = await httpClient.PostAsJsonAsync($"/api/users/{userId}/todos", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task<TodoListWithItemsDto?> GetTodoListWithItemsAsync(Guid todoListId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoListWithItemsDto>($"/api/todos/{todoListId}", cancellationToken);
    }

    public async Task<Guid> AddTodoItemAsync(Guid todoListId, string title, string? description, string priority, DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        var request = new { Title = title, Description = description, Priority = priority, DueDate = dueDate };
        var response = await httpClient.PostAsJsonAsync($"/api/todos/{todoListId}/items", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task CompleteTodoItemAsync(Guid todoListId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsync($"/api/todos/{todoListId}/items/{itemId}/complete", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
