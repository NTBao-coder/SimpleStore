using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleStore.Identity.API.Client;

namespace SimpleStore.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly IIdentityApiClient _identity;
    private readonly ITokenSessionStore _sessions;
    public LogoutModel(IIdentityApiClient identity, ITokenSessionStore sessions) => (_identity, _sessions) = (identity, sessions);

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        var sessionId = User.FindFirst(JwtCookieAuthenticationEvents.SessionIdClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _sessions.GetAsync(sessionId, HttpContext.RequestAborted);
            if (session is not null) await _identity.LogoutAsync(session.RefreshToken, HttpContext.RequestAborted);
            await _sessions.RemoveAsync(sessionId, HttpContext.RequestAborted);
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }
}
