namespace SimpleStore.Identity.API.Client;

public sealed record RegisterRequest(string Email, string FullName, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record IdentityUserDto(string Id, string Email, string FullName, string[] Roles,
    string? PhoneNumber = null, bool IsLocked = false);
public sealed record AuthResponse(IdentityUserDto User, string AccessToken, DateTime ExpiresAt, string RefreshToken);
public sealed record UpdateUserRequest(string FullName);
