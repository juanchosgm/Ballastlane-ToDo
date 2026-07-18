using ToDo.Api.Common;
using ToDo.Application.Todos.Dtos;
using ToDo.Application.Todos.Services;

namespace ToDo.Api.Endpoints;

/// <summary>Minimal API endpoint definitions for the To-Do feature.</summary>
public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("Todos")
            .RequireAuthorization();

        group.MapGet("/", async (ITodoService service, CancellationToken ct) =>
                Results.Ok(await service.GetAllAsync(ct)))
            .WithName("GetTodos")
            .WithSummary("List all To-Dos (summary, without description).")
            .Produces<IReadOnlyList<TodoSummaryDto>>();

        group.MapGet("/{id:guid}", async (Guid id, ITodoService service, CancellationToken ct) =>
                Results.Ok(await service.GetByIdAsync(id, ct)))
            .WithName("GetTodoById")
            .WithSummary("Get the full details of a single To-Do (includes description).")
            .Produces<TodoDetailDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateTodoRequest request, ITodoService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.CreatedAtRoute("GetTodoById", new { id = created.Id }, created);
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoRequest>>()
            .WithName("CreateTodo")
            .WithSummary("Create a new To-Do.")
            .Produces<TodoDetailDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateTodoRequest request, ITodoService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .AddEndpointFilter<ValidationFilter<UpdateTodoRequest>>()
            .WithName("UpdateTodo")
            .WithSummary("Update an existing To-Do.")
            .Produces<TodoDetailDto>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", async (Guid id, ITodoService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .WithName("DeleteTodo")
            .WithSummary("Delete a To-Do.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
