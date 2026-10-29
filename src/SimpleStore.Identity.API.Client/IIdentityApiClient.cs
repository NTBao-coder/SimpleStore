namespace SimpleStore.Identity.API.Client;

public interface IIdentityApiClient
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetMeAsync(CancellationToken cancellationToken = default);
    Task UpdateMeAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdentityUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task SetLockedAsync(string id, bool locked, CancellationToken cancellationToken = default);
}
