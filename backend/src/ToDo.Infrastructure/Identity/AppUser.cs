using Microsoft.AspNetCore.Identity;

namespace ToDo.Infrastructure.Identity;

/// <summary>
/// Application user. Extends the framework <see cref="IdentityUser"/> (string primary key)
/// so we get the full ASP.NET Core Identity machinery — password hashing, lockout, tokens —
/// without hand-rolling any of it.
/// </summary>
public sealed class AppUser : IdentityUser
{
}
