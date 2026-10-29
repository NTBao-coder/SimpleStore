namespace SimpleStore.Identity.API.Client;

public interface IApiBearerTokenProvider
{
    Task<string?> GetCurrentTokenAsync(CancellationToken cancellationToken = default);
}
