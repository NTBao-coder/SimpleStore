using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Identity.API.Client;
using SimpleStore.Identity.API.Data;
using SimpleStore.Identity.API.Services;

namespace SimpleStore.Identity.API.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/identity");
        api.MapPost("/register", async (RegisterRequest request, UserManager<ApplicationUser> users,
            IJwtTokenService tokens, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName) || request.Password.Length < 6)
                return Results.BadRequest(new { error = "Email, full name and a password of at least 6 characters are required." });
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(), Email = request.Email.Trim(), EmailConfirmed = true,
                FullName = request.FullName.Trim()
            };
            var result = await users.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(new { errors = result.Errors.Select(error => error.Description) });
            var role = await users.AddToRoleAsync(user, "Customer");
            if (!role.Succeeded) return Results.Problem("The customer role could not be assigned.");
            return Results.Ok(await tokens.CreateSessionAsync(user, ct));
        }).AllowAnonymous();

        api.MapPost("/login", async (LoginRequest request, UserManager<ApplicationUser> users,
            IJwtTokenService tokens, CancellationToken ct) =>
        {
            var user = await users.FindByEmailAsync(request.Email);
            if (user is null || await users.IsLockedOutAsync(user)) return Results.Unauthorized();
            if (!await users.CheckPasswordAsync(user, request.Password))
            {
                await users.AccessFailedAsync(user);
                return Results.Unauthorized();
            }
            await users.ResetAccessFailedCountAsync(user);
            return Results.Ok(await tokens.CreateSessionAsync(user, ct));
        }).AllowAnonymous();

        api.MapPost("/refresh", async (RefreshRequest request, IdentityDbContext db,
            UserManager<ApplicationUser> users, IJwtTokenService tokens, CancellationToken ct) =>
        {
            var hash = JwtTokenService.Hash(request.RefreshToken);
            var existing = await db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == hash, ct);
            if (existing is null || existing.RevokedAt is not null || existing.ExpiresAt <= DateTimeOffset.UtcNow)
                return Results.Unauthorized();
            var user = await users.FindByIdAsync(existing.UserId);
            if (user is null || await users.IsLockedOutAsync(user)) return Results.Unauthorized();
            existing.RevokedAt = DateTimeOffset.UtcNow;
            var response = await tokens.CreateSessionAsync(user, ct);
            existing.ReplacedByTokenHash = JwtTokenService.Hash(response.RefreshToken);
            await db.SaveChangesAsync(ct);
            return Results.Ok(response);
        }).AllowAnonymous();

        api.MapPost("/logout", async (LogoutRequest request, IdentityDbContext db, CancellationToken ct) =>
        {
            var hash = JwtTokenService.Hash(request.RefreshToken);
            var token = await db.RefreshTokens.SingleOrDefaultAsync(value => value.TokenHash == hash, ct);
            if (token is not null && token.RevokedAt is null)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            return Results.NoContent();
        }).AllowAnonymous();

        api.MapGet("/me", async (ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
        {
            var user = await users.GetUserAsync(principal);
            return user is null ? Results.NotFound() : Results.Ok(await ToDtoAsync(users, user));
        }).RequireAuthorization();
        api.MapPut("/me", async (UpdateUserRequest request, ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(request.FullName)) return Results.BadRequest("Full name is required.");
            user.FullName = request.FullName.Trim();
            var result = await users.UpdateAsync(user);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors);
        }).RequireAuthorization();

        var admin = api.MapGroup("/users").RequireAuthorization(policy => policy.RequireRole("Admin"));
        admin.MapGet("", async (UserManager<ApplicationUser> users) =>
        {
            var values = await users.Users.AsNoTracking().OrderBy(user => user.Email).ToListAsync();
            var result = new List<IdentityUserDto>(values.Count);
            foreach (var user in values) result.Add(await ToDtoAsync(users, user));
            return Results.Ok(result);
        });
        admin.MapPut("/{id}", async (string id, UpdateUserRequest request, UserManager<ApplicationUser> users) =>
        {
            var user = await users.FindByIdAsync(id);
            if (user is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(request.FullName)) return Results.BadRequest("Full name is required.");
            user.FullName = request.FullName.Trim();
            var result = await users.UpdateAsync(user);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors);
        });
        admin.MapPost("/{id}/lock", async (string id, UserManager<ApplicationUser> users) => await SetLockAsync(users, id, true));
        admin.MapPost("/{id}/unlock", async (string id, UserManager<ApplicationUser> users) => await SetLockAsync(users, id, false));
    }

    private static async Task<IResult> SetLockAsync(UserManager<ApplicationUser> users, string id, bool locked)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return Results.NotFound();
        var result = locked
            ? await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100))
            : await users.SetLockoutEndDateAsync(user, null);
        return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors);
    }

    private static async Task<IdentityUserDto> ToDtoAsync(UserManager<ApplicationUser> users, ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.FullName, (await users.GetRolesAsync(user)).ToArray(),
            user.PhoneNumber, user.LockoutEnd > DateTimeOffset.UtcNow);
}
