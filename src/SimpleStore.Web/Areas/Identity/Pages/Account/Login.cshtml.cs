using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleStore.Identity.API.Client;

namespace SimpleStore.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IIdentityApiClient _identity;
    private readonly ITokenSessionStore _sessions;
    public LoginModel(IIdentityApiClient identity, ITokenSessionStore sessions) => (_identity, _sessions) = (identity, sessions);

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var response = await _identity.LoginAsync(new LoginRequest(Input.Email, Input.Password), HttpContext.RequestAborted);
        if (response is not null)
        {
            var sessionId = Guid.NewGuid().ToString("N");
            await _sessions.SaveAsync(sessionId, new TokenSession(response.AccessToken, response.RefreshToken, response.ExpiresAt));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                JwtCookieAuthenticationEvents.CreatePrincipal(response, sessionId),
                new AuthenticationProperties { IsPersistent = Input.RememberMe, AllowRefresh = true });
            return LocalRedirect(returnUrl);
        }
        ModelState.AddModelError(string.Empty, "Invalid email or password, or the account is locked.");

        ReturnUrl = returnUrl;
        return Page();
    }

    public IActionResult OnPostPasskeyRequestOptionsAsync() => NotFound();

    // Receives the signed assertion (raw WebAuthn credential JSON) and signs the user in.
    public IActionResult OnPostPasskeyAssertAsync([FromBody] JsonElement credential, string? returnUrl = null)
    {
        return NotFound();
    }
}
