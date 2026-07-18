using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ToDo.Infrastructure.Identity;

namespace ToDo.Infrastructure.Persistence;

/// <summary>
/// Seeds a single demo user so the app is usable out of the box. Credentials can be
/// overridden through the "SeedUser:Email" / "SeedUser:Password" configuration keys.
/// </summary>
public static class IdentitySeeder
{
    public const string DefaultEmail = "demo@ballastlane.com";
    public const string DefaultPassword = "Passw0rd!";

    public static async Task SeedAsync(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        var email = configuration["SeedUser:Email"] ?? DefaultEmail;
        var password = configuration["SeedUser:Password"] ?? DefaultPassword;

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed demo user: {errors}");
        }
    }
}
