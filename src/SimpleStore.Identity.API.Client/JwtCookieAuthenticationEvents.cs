using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SimpleStore.Identity.API.Client;

public sealed class JwtCookieAuthenticationEvents(IIdentityApiClient identity, ITokenSessionStore sessions) : CookieAuthenticationEvents
{
    public const string SessionIdClaim = "identity_session";

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var sessionId = context.Principal?.FindFirst(SessionIdClaim)?.Value;
        var session = string.IsNullOrWhiteSpace(sessionId) ? null :
            await sessions.GetValidAsync(sessionId, identity, context.HttpContext.RequestAborted);
        if (sessionId is null || session is null)
        {
            context.RejectPrincipal();
            return;
        }

    }

    public static ClaimsPrincipal CreatePrincipal(AuthResponse response, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.User.Id),
            new(ClaimTypes.Name, response.User.FullName),
            new(ClaimTypes.Email, response.User.Email),
            new(SessionIdClaim, sessionId)
        };
        claims.AddRange(response.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
