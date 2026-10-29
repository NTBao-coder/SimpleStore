using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SimpleStore.Catalog.API.Auth;

public sealed class JwtSettings(string issuer, string audience, byte[] signingKey)
{
    public string Issuer { get; } = issuer;
    public string Audience { get; } = audience;
    public byte[] SigningKey { get; } = signingKey;

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var value = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Jwt:Issuer, Jwt:Audience and Jwt:Key must be configured.");
        var key = Convert.FromBase64String(value);
        if (key.Length < 32) throw new InvalidOperationException("Jwt:Key must decode to at least 32 bytes.");
        return new JwtSettings(issuer, audience, key);
    }

    public TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true, ValidIssuer = Issuer,
        ValidateAudience = true, ValidAudience = Audience,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(SigningKey),
        NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role, ClockSkew = TimeSpan.FromSeconds(30)
    };
}
