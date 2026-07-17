using ToDo.Application.Todos.Dtos;

namespace ToDo.Application.Todos.Services;

/// <summary>Use-case orchestration for the To-Do feature.</summary>
public interface ITodoService
{
    Task<IReadOnlyList<TodoSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TodoDetailDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<TodoDetailDto> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
