using System.Text.Json;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace SimpleStore.Identity.API.Client;

public sealed class DistributedTokenSessionStore(IDistributedCache cache) : ITokenSessionStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

    public async Task<TokenSession?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync(Key(sessionId), cancellationToken);
        return value is null ? null : JsonSerializer.Deserialize<TokenSession>(value);
    }

    public Task SaveAsync(string sessionId, TokenSession session, CancellationToken cancellationToken = default) =>
        cache.SetStringAsync(Key(sessionId), JsonSerializer.Serialize(session),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Lifetime }, cancellationToken);

    public Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(Key(sessionId), cancellationToken);

    public async Task<TokenSession?> GetValidAsync(string sessionId, IIdentityApiClient identity,
        CancellationToken cancellationToken = default)
    {
        var gate = RefreshLocks.GetOrAdd(sessionId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var session = await GetAsync(sessionId, cancellationToken);
            if (session is null || session.ExpiresAt > DateTime.UtcNow.AddMinutes(1)) return session;
            var response = await identity.RefreshAsync(session.RefreshToken, cancellationToken);
            if (response is null)
            {
                await RemoveAsync(sessionId, cancellationToken);
                return null;
            }
            var refreshed = new TokenSession(response.AccessToken, response.RefreshToken, response.ExpiresAt);
            await SaveAsync(sessionId, refreshed, cancellationToken);
            return refreshed;
        }
        finally
        {
            gate.Release();
        }
    }

    private static string Key(string sessionId) => $"identity-session:{sessionId}";
}
