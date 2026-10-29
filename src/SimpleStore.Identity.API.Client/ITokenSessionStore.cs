namespace SimpleStore.Identity.API.Client;

public sealed record TokenSession(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public interface ITokenSessionStore
{
    Task<TokenSession?> GetAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<TokenSession?> GetValidAsync(string sessionId, IIdentityApiClient identity, CancellationToken cancellationToken = default);
    Task SaveAsync(string sessionId, TokenSession session, CancellationToken cancellationToken = default);
    Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default);
}
