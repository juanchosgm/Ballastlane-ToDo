using ToDo.Domain.Entities;

namespace ToDo.Infrastructure.Persistence;

/// <summary>Seeds a few sample tasks so the UI is not empty on first run.</summary>
public static class TodoDbSeeder
{
    public static async Task SeedAsync(TodoDbContext context)
    {
        if (context.Todos.Any()) return;

        var samples = new[]
        {
            new TodoItem("Welcome to your To-Do board", "Open the details view to read the full description. This field is only shown here."),
            new TodoItem("Create a task", "Use the + New Task button, fill the reactive form and save."),
            new TodoItem("Complete a task", "Toggle the checkbox to mark a task as done.")
        };

        samples[2].MarkCompleted();

        await context.Todos.AddRangeAsync(samples);
        await context.SaveChangesAsync();
    }
}
