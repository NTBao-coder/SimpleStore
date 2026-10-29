using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleStore.Identity.API.Client;

namespace SimpleStore.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IIdentityApiClient _identity;
    private readonly ITokenSessionStore _sessions;
    public RegisterModel(IIdentityApiClient identity, ITokenSessionStore sessions) => (_identity, _sessions) = (identity, sessions);

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Display(Name = "Full name"), StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _identity.RegisterAsync(
            new RegisterRequest(Input.Email, Input.FullName, Input.Password), HttpContext.RequestAborted);
        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Registration failed. Check the email and password requirements.");
            return Page();
        }

        var sessionId = Guid.NewGuid().ToString("N");
        await _sessions.SaveAsync(sessionId, new TokenSession(response.AccessToken, response.RefreshToken, response.ExpiresAt));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            JwtCookieAuthenticationEvents.CreatePrincipal(response, sessionId), new AuthenticationProperties { AllowRefresh = true });
        return LocalRedirect(returnUrl);
    }
}
