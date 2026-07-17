using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Application.Common.Interfaces;
using ToDo.Infrastructure.Persistence;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Infrastructure;

/// <summary>Composition root for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // In-memory provider keeps the development / evaluation setup zero-config.
        services.AddDbContext<TodoDbContext>(options =>
            options.UseInMemoryDatabase("ToDoDb"));

        services.AddScoped<ITodoRepository, TodoRepository>();

        return services;
    }
}
