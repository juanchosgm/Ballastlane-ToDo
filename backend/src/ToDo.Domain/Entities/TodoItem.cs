using ToDo.Domain.Enums;

namespace ToDo.Domain.Entities;

/// <summary>
/// Core domain entity representing a single To-Do task.
/// State transitions are encapsulated behind behaviour methods so the entity
/// can never be constructed or mutated into an invalid state.
/// </summary>
public class TodoItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>Lifecycle state of the task (Pending / InProgress / Done).</summary>
    public TodoStatus Status { get; private set; }

    /// <summary>Optional date by which the task should be completed.</summary>
    public DateTimeOffset? DueDate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>Identifier of the user that owns this task. Tasks are never shared across users.</summary>
    public string UserId { get; private set; } = string.Empty;

    // Required by EF Core to materialise entities.
    private TodoItem() { }

    public TodoItem(string title, string? description, string userId, DateTimeOffset? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        Rename(title, description);
        Status = TodoStatus.Pending;
        DueDate = dueDate;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the editable fields of the task and stamps the change time.</summary>
    public void Update(string title, string? description, TodoStatus status, DateTimeOffset? dueDate)
    {
        Rename(title, description);
        Status = status;
        DueDate = dueDate;
        Touch();
    }

    public void MarkCompleted()
    {
        if (Status == TodoStatus.Done) return;
        Status = TodoStatus.Done;
        Touch();
    }

    public void MarkPending()
    {
        if (Status == TodoStatus.Pending) return;
        Status = TodoStatus.Pending;
        Touch();
    }

    private void Rename(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
