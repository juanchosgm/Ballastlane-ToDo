using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Interfaces;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Persistence;

namespace ToDo.Infrastructure.Repositories;

public sealed class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context) => _context = context;

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Todos
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        await _context.Todos.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _context.Todos.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _context.Todos.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
