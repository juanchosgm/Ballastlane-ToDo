using System.Security.Claims;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Api.Common;

/// <summary>
/// Reads the authenticated user's id out of the current request's <see cref="ClaimsPrincipal"/>.
/// Identity stores the user id in the <see cref="ClaimTypes.NameIdentifier"/> claim.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("The request is not authenticated.");
}
