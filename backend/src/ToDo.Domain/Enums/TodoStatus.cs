namespace ToDo.Domain.Enums;

/// <summary>
/// The lifecycle state of a <see cref="Entities.TodoItem"/>.
/// Replaces the previous boolean completion flag so a task can express that
/// work has started but is not yet finished.
/// </summary>
public enum TodoStatus
{
    /// <summary>Not started yet.</summary>
    Pending = 0,

    /// <summary>Being worked on.</summary>
    InProgress = 1,

    /// <summary>Finished.</summary>
    Done = 2
}
