namespace Tasks.Poc.Application.TodoLists.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;

public record GetUserTodoListsQuery(Guid UserId) : IRequest<List<TodoListDto>>;

public class GetUserTodoListsHandler : IRequestHandler<GetUserTodoListsQuery, List<TodoListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserTodoListsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TodoListDto>> Handle(GetUserTodoListsQuery request, CancellationToken cancellationToken)
    {
        var todoLists = await _unitOfWork.TodoListRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return todoLists.Select(todoList => new TodoListDto(
            todoList.Id.Value,
            todoList.Title.Value,
            todoList.Description?.Value,
            todoList.OwnerId.Value,
            todoList.CreatedAt,
            todoList.UpdatedAt,
            todoList.TotalItems,
            todoList.CompletedItems,
            todoList.PendingItems,
            todoList.OverdueItems,
            todoList.CompletionPercentage,
            todoList.IsCompleted))
            .ToList();
    }
}
