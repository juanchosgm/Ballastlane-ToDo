using FluentValidation;

namespace ToDo.Api.Common;

/// <summary>
/// Reusable Minimal API endpoint filter that runs the FluentValidation validator
/// registered for <typeparamref name="T"/> before the handler executes.
/// </summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        var model = context.Arguments.OfType<T>().FirstOrDefault();

        if (validator is not null && model is not null)
        {
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
