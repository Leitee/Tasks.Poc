namespace Tasks.Poc.Application.TodoLists.Commands;

using MediatR;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Domain.Enums;

public record AddTodoItemCommand(
    Guid TodoListId,
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate) : IRequest<Guid>;

public class AddTodoItemHandler : IRequestHandler<AddTodoItemCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddTodoItemHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoList = await _unitOfWork.TodoListRepository.GetByIdWithItemsAsync(request.TodoListId, cancellationToken);

        if (todoList == null)
        {
            throw new InvalidOperationException($"Todo list with ID {request.TodoListId} not found.");
        }

        var itemId = todoList.AddItem(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        await _unitOfWork.TodoListRepository.UpdateAsync(todoList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return itemId.Value;
    }
}