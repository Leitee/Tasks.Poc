namespace Tasks.Poc.Application.TodoLists.Commands;

using MediatR;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Domain.Entities;

public record CreateTodoListCommand(
    Guid UserId,
    string Title,
    string? Description) : IRequest<Guid>;

public class CreateTodoListHandler : IRequestHandler<CreateTodoListCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTodoListHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found.");
        }

        var todoList = TodoList.Create(
            request.Title,
            request.Description,
            request.UserId);

        await _unitOfWork.TodoListRepository.AddAsync(todoList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return todoList.Id;
    }
}