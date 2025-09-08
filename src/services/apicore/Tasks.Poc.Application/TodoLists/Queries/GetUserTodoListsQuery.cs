namespace Tasks.Poc.Application.TodoLists.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Domain.Entities;

public record GetUserTodoListsQuery(Guid UserId) : IRequest<List<TodoListDto>>;

public class GetUserTodoListsHandler : IRequestHandler<GetUserTodoListsQuery, List<TodoListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper<TodoList, TodoListDto> _todoListMapper;

    public GetUserTodoListsHandler(IUnitOfWork unitOfWork, IMapper<TodoList, TodoListDto> todoListMapper)
    {
        _unitOfWork = unitOfWork;
        _todoListMapper = todoListMapper;
    }

    public async Task<List<TodoListDto>> Handle(GetUserTodoListsQuery request, CancellationToken cancellationToken)
    {
        var todoLists = await _unitOfWork.TodoListRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return _todoListMapper.Map(todoLists).ToList();
    }
}
