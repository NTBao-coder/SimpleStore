using Microsoft.AspNetCore.Identity;

namespace SimpleStore.Identity.API.Data;

public static class IdentitySeeder
{
    public const string DemoEmail = "demo@simplestore.local";
    public const string AdminEmail = "admin@simplestore.local";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "Customer", "Admin" })
        {
            if (!await roles.RoleExistsAsync(role))
                Ensure(await roles.CreateAsync(new IdentityRole(role)));
        }

        await EnsureUserAsync(users, DemoEmail, "Demo123!", "Demo User", "Customer");
        await EnsureUserAsync(users, AdminEmail, "Admin123!", "Store Administrator", "Admin");
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, string email,
        string password, string fullName, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName };
            Ensure(await users.CreateAsync(user, password));
        }

        if (!await users.IsInRoleAsync(user, role))
            Ensure(await users.AddToRoleAsync(user, role));
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
