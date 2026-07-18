namespace ToDo.Application.Common.Interfaces;

/// <summary>
/// Exposes the identity of the authenticated caller to the Application layer,
/// without leaking any dependency on ASP.NET Core / HttpContext into the use-cases.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The authenticated user's id. Throws if the request is not authenticated.</summary>
    string UserId { get; }
}
