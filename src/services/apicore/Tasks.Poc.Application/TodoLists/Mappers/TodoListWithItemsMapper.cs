namespace Tasks.Poc.Application.TodoLists.Mappers;

using System.Collections.Generic;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public class TodoListWithItemsMapper : IMapper<TodoList, TodoListWithItemsDto>
{
    private readonly IMapper<TodoItem, TodoItemDto> _todoItemMapper;

    public TodoListWithItemsMapper(IMapper<TodoItem, TodoItemDto> todoItemMapper)
    {
        _todoItemMapper = todoItemMapper;
    }

    public TodoListWithItemsDto? Map(TodoList? source)
    {
        if (source == null)
        {
            return null;
        }

        var itemDtos = _todoItemMapper.Map(source.Items);

        return new TodoListWithItemsDto(
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
            source.IsCompleted,
            itemDtos);
    }

    public IReadOnlyList<TodoListWithItemsDto> Map(IReadOnlyList<TodoList> sources) =>
        sources.Select(source => Map(source)!).ToList().AsReadOnly();
}
