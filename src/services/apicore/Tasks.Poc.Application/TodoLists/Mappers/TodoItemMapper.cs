namespace Tasks.Poc.Application.TodoLists.Mappers;

using System.Collections.Generic;
using System.Linq;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public class TodoItemMapper : IMapper<TodoItem, TodoItemDto>
{
    public TodoItemDto? Map(TodoItem? source)
    {
        if (source == null)
        {
            return null;
        }

        return new(
            source.Id,
            source.Title,
            source.Description?.Value,
            source.IsCompleted,
            source.Priority.ToString(),
            source.CreatedAt,
            source.CompletedAt,
            source.DueDate,
            source.IsOverdue);
    }

    public IReadOnlyList<TodoItemDto> Map(IReadOnlyList<TodoItem> sources) =>
        sources.Select(source => Map(source)!).ToList().AsReadOnly();
}
