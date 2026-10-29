using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleStore.Identity.API.Client;
using SimpleStore.Identity.API.Data;

namespace SimpleStore.Identity.API.Services;

public sealed class JwtTokenService(
    JwtSettings settings,
    UserManager<ApplicationUser> users,
    IdentityDbContext db) : IJwtTokenService
{
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(30);

    public async Task<AuthResponse> CreateSessionAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(AccessLifetime);
        var roles = await users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(settings.SigningKey), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now, expiresAt, credentials);
        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawRefresh),
            ExpiresAt = DateTimeOffset.UtcNow.Add(RefreshLifetime)
        };
        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            new IdentityUserDto(user.Id, user.Email ?? string.Empty, user.FullName, roles.ToArray()),
            new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt, rawRefresh);
    }

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
