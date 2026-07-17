namespace ToDo.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested aggregate cannot be located. Translated to an
/// HTTP 404 ProblemDetails response by the global exception handler.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"\"{name}\" ({key}) was not found.")
    {
    }
}
