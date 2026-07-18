using ToDo.Domain.Entities;
using ToDo.Domain.Enums;

namespace ToDo.Application.Todos.Dtos;

/// <summary>Lightweight projection used by the list view. Deliberately omits the Description.</summary>
public sealed record TodoSummaryDto(
    Guid Id,
    string Title,
    TodoStatus Status,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Full projection used by the details view. This is the only DTO that exposes the Description.</summary>
public sealed record TodoDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Payload for creating a To-Do.</summary>
public sealed record CreateTodoRequest(string Title, string? Description, DateTimeOffset? DueDate);

/// <summary>Payload for updating an existing To-Do.</summary>
public sealed record UpdateTodoRequest(string Title, string? Description, TodoStatus Status, DateTimeOffset? DueDate);

public static class TodoMappings
{
    public static TodoSummaryDto ToSummaryDto(this TodoItem item) =>
        new(item.Id, item.Title, item.Status, item.DueDate, item.CreatedAt, item.UpdatedAt);

    public static TodoDetailDto ToDetailDto(this TodoItem item) =>
        new(item.Id, item.Title, item.Description, item.Status, item.DueDate, item.CreatedAt, item.UpdatedAt);
}
