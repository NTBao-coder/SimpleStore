using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStore.Identity.API.Client;

public sealed class ApiBearerTokenProvider(IHttpContextAccessor contextAccessor, IServiceProvider services,
    ITokenSessionStore sessions) : IApiBearerTokenProvider
{
    public async Task<string?> GetCurrentTokenAsync(CancellationToken cancellationToken = default)
    {
        var principal = contextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true && services.GetService<AuthenticationStateProvider>() is { } authProvider)
            principal = (await authProvider.GetAuthenticationStateAsync()).User;
        var sessionId = principal?.FindFirst(JwtCookieAuthenticationEvents.SessionIdClaim)?.Value;
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        var identity = services.GetRequiredService<IIdentityApiClient>();
        return (await sessions.GetValidAsync(sessionId, identity, cancellationToken))?.AccessToken;
    }
}
