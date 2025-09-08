namespace Tasks.Poc.Application.TodoLists.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Domain.Entities;

public record GetTodoListWithItemsQuery(Guid TodoListId) : IRequest<TodoListWithItemsDto?>;

public class GetTodoListWithItemsHandler : IRequestHandler<GetTodoListWithItemsQuery, TodoListWithItemsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper<TodoList, TodoListWithItemsDto> _todoListMapper;

    public GetTodoListWithItemsHandler(IUnitOfWork unitOfWork, IMapper<TodoList, TodoListWithItemsDto> todoListMapper)
    {
        _unitOfWork = unitOfWork;
        _todoListMapper = todoListMapper;
    }

    public async Task<TodoListWithItemsDto?> Handle(GetTodoListWithItemsQuery request, CancellationToken cancellationToken)
    {
        var todoList = await _unitOfWork.TodoListRepository.GetByIdWithItemsAsync(request.TodoListId, cancellationToken);

        return _todoListMapper.Map(todoList);
    }
}
