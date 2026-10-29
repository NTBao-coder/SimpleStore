using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SimpleStore.Identity.API.Services;

public sealed class JwtSettings(string issuer, string audience, byte[] signingKey)
{
    public string Issuer { get; } = issuer;
    public string Audience { get; } = audience;
    public byte[] SigningKey { get; } = signingKey;

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var encodedKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(encodedKey))
            throw new InvalidOperationException("Jwt:Issuer, Jwt:Audience and Jwt:Key must be configured.");

        byte[] key;
        try { key = Convert.FromBase64String(encodedKey); }
        catch (FormatException exception) { throw new InvalidOperationException("Jwt:Key must be base64 encoded.", exception); }
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must decode to at least 32 bytes.");
        return new JwtSettings(issuer, audience, key);
    }

    public TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(SigningKey),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
}
