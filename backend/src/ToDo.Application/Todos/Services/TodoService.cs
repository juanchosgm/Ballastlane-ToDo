using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;
using ToDo.Application.Todos.Dtos;
using ToDo.Domain.Entities;

namespace ToDo.Application.Todos.Services;

public sealed class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ICurrentUser _currentUser;

    public TodoService(ITodoRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TodoSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(_currentUser.UserId, cancellationToken);
        return items.Select(i => i.ToSummaryDto()).ToList();
    }

    public async Task<TodoDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, _currentUser.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(TodoItem), id);
        return item.ToDetailDto();
    }

    public async Task<TodoDetailDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var item = new TodoItem(request.Title, request.Description, _currentUser.UserId, request.DueDate);
        await _repository.AddAsync(item, cancellationToken);
        return item.ToDetailDto();
    }

    public async Task<TodoDetailDto> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, _currentUser.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(TodoItem), id);

        item.Update(request.Title, request.Description, request.Status, request.DueDate);
        await _repository.UpdateAsync(item, cancellationToken);
        return item.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, _currentUser.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(TodoItem), id);
        await _repository.DeleteAsync(item, cancellationToken);
    }
}
