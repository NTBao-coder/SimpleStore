using SimpleStore.Identity.API.Data;
using SimpleStore.Identity.API.Client;

namespace SimpleStore.Identity.API.Services;

public interface IJwtTokenService
{
    Task<AuthResponse> CreateSessionAsync(ApplicationUser user, CancellationToken cancellationToken);
}
