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
    public bool IsCompleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Required by EF Core to materialise entities.
    private TodoItem() { }

    public TodoItem(string title, string? description)
    {
        Id = Guid.NewGuid();
        Rename(title, description);
        IsCompleted = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the editable fields of the task and stamps the change time.</summary>
    public void Update(string title, string? description, bool isCompleted)
    {
        Rename(title, description);
        IsCompleted = isCompleted;
        Touch();
    }

    public void MarkCompleted()
    {
        if (IsCompleted) return;
        IsCompleted = true;
        Touch();
    }

    public void MarkPending()
    {
        if (!IsCompleted) return;
        IsCompleted = false;
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
