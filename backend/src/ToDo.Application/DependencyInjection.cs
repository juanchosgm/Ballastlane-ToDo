using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Application.Todos.Services;
using ToDo.Application.Todos.Validators;

namespace ToDo.Application;

/// <summary>Composition root for the Application layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();

        // Register every AbstractValidator defined in this assembly.
        services.AddValidatorsFromAssemblyContaining<CreateTodoRequestValidator>(ServiceLifetime.Scoped);

        return services;
    }
}
