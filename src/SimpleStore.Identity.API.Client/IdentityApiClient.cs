using System.Net;
using System.Net.Http.Json;

namespace SimpleStore.Identity.API.Client;

public sealed class IdentityApiClient(HttpClient httpClient, IApiBearerTokenProvider tokens) : IIdentityApiClient
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        await PostAuthAsync("/api/identity/login", request, cancellationToken);

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        await PostAuthAsync("/api/identity/register", request, cancellationToken);

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        await PostAuthAsync("/api/identity/refresh", new RefreshRequest(refreshToken), cancellationToken);

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/identity/logout", new LogoutRequest(refreshToken), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IdentityUserDto?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthenticatedAsync(HttpMethod.Get, "/api/identity/me", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IdentityUserDto>(cancellationToken: cancellationToken);
    }

    public async Task UpdateMeAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthenticatedAsync(HttpMethod.Put, "/api/identity/me", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IdentityUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthenticatedAsync(HttpMethod.Get, "/api/identity/users", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IdentityUserDto[]>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthenticatedAsync(HttpMethod.Put, $"/api/identity/users/{Uri.EscapeDataString(id)}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetLockedAsync(string id, bool locked, CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthenticatedAsync(HttpMethod.Post, $"/api/identity/users/{Uri.EscapeDataString(id)}/{(locked ? "lock" : "unlock")}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<AuthResponse?> PostAuthAsync<T>(string path, T request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAuthenticatedAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        var token = await tokens.GetCurrentTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await httpClient.SendAsync(request, cancellationToken);
    }
}
