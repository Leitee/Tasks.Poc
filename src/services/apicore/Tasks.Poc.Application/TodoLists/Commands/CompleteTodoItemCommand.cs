namespace Tasks.Poc.Application.TodoLists.Commands;

using MediatR;
using Tasks.Poc.Application.Interfaces;


public record CompleteTodoItemCommand(
    Guid TodoListId,
    Guid ItemId) : IRequest;

public class CompleteTodoItemHandler : IRequestHandler<CompleteTodoItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTodoItemHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoList = await _unitOfWork.TodoListRepository.GetByIdWithItemsAsync(request.TodoListId, cancellationToken);

        if (todoList == null)
        {
            throw new InvalidOperationException($"Todo list with ID {request.TodoListId} not found.");
        }

        todoList.CompleteItem(request.ItemId);

        await _unitOfWork.TodoListRepository.UpdateAsync(todoList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}