namespace Tasks.Poc.Application.TodoLists.Mappers;

using System.Collections.Generic;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public class TodoListMapper : IMapper<TodoList, TodoListDto>
{
    private readonly IMapper<TodoItem, TodoItemDto> _todoItemMapper;

    public TodoListMapper(IMapper<TodoItem, TodoItemDto> todoItemMapper)
    {
        _todoItemMapper = todoItemMapper;
    }

    public TodoListDto? Map(TodoList? source)
    {
        if (source == null)
        {
            return null;
        }

        var itemDtos = _todoItemMapper.Map(source.Items);

        return new TodoListDto(
            source.Id,
            source.Title,
            source.Description?.Value,
            source.OwnerId,
            source.CreatedAt,
            source.LastModifiedAt,
            source.TotalItems,
            source.CompletedItems,
            source.PendingItems,
            source.OverdueItems,
            source.CompletionPercentage,
            source.IsCompleted);
    }

    public IReadOnlyList<TodoListDto> Map(IReadOnlyList<TodoList> sources) =>
        sources.Select(source => Map(source)!).ToList().AsReadOnly();
}
