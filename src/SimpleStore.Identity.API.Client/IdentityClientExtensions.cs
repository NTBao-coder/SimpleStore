using Microsoft.Extensions.DependencyInjection;

namespace SimpleStore.Identity.API.Client;

public static class IdentityClientExtensions
{
    public static IHttpClientBuilder AddIdentityApiClient(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenSessionStore, DistributedTokenSessionStore>();
        services.AddScoped<IApiBearerTokenProvider, ApiBearerTokenProvider>();
        services.AddScoped<JwtCookieAuthenticationEvents>();
        return services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
            client.BaseAddress = new Uri("https+http://identity-api"));
    }
}
