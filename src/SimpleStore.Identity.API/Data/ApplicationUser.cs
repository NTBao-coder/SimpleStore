using Microsoft.AspNetCore.Identity;

namespace SimpleStore.Identity.API.Data;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
