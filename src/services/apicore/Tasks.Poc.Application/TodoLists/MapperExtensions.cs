namespace Tasks.Poc.Application.TodoLists;

using System.Collections.Generic;
using System.Linq;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public static partial class MapperExtensions
{
    public static TodoItemDto? ToDto(this TodoItem item)
    {
        if (item == null)
        {
            return null;
        }

        return new(
            item.Id,
            item.Title,
            item.Description?.Value,
            item.IsCompleted,
            item.Priority.ToString(),
            item.CreatedAt,
            item.CompletedAt,
            item.DueDate,
            item.IsOverdue);
    }

    public static TodoListWithItemsDto? ToDto(this TodoList todoList)
    {
        if (todoList == null)
        {
            return null;
        }

        var itemDtos = todoList.Items.ToDtos();

        return new TodoListWithItemsDto(
            todoList.Id,
            todoList.Title,
            todoList.Description?.Value,
            todoList.OwnerId,
            todoList.CreatedAt,
            todoList.UpdatedAt,
            todoList.TotalItems,
            todoList.CompletedItems,
            todoList.PendingItems,
            todoList.OverdueItems,
            todoList.CompletionPercentage,
            todoList.IsCompleted,
            itemDtos);
    }

    public static IEnumerable<TodoItemDto> ToDtos(this IEnumerable<TodoItem> items) =>
        items.Select(static item => item.ToDto()!);
}
