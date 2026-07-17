using ToDo.Domain.Entities;

namespace ToDo.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction for <see cref="TodoItem"/>. Lives in the Application layer
/// so the domain/use-cases depend on an interface, not on EF Core (Dependency Inversion).
/// </summary>
public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
}
