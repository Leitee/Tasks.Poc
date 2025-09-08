namespace Tasks.Poc.Application.TodoLists.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;

public record GetTodoListWithItemsQuery(Guid TodoListId) : IRequest<TodoListWithItemsDto?>;

public class GetTodoListWithItemsHandler : IRequestHandler<GetTodoListWithItemsQuery, TodoListWithItemsDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTodoListWithItemsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TodoListWithItemsDto?> Handle(GetTodoListWithItemsQuery request, CancellationToken cancellationToken)
    {
        var todoList = await _unitOfWork.TodoListRepository.GetByIdWithItemsAsync(request.TodoListId, cancellationToken);

        return todoList?.ToDto();
    }
}
