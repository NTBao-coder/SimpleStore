using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleStore.Identity.API.Client;

namespace SimpleStore.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IIdentityApiClient _identity;
    public IndexModel(IIdentityApiClient identity) => _identity = identity;

    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required, Display(Name = "Full name"), StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _identity.GetMeAsync(HttpContext.RequestAborted);
        if (user is null) return NotFound();

        Email = user.Email ?? string.Empty;
        Input = new InputModel { FullName = user.FullName };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _identity.GetMeAsync(HttpContext.RequestAborted);
        if (user is null) return NotFound();

        if (!ModelState.IsValid)
        {
            Email = user.Email ?? string.Empty;
            return Page();
        }

        if (Input.FullName != user.FullName)
        {
            await _identity.UpdateMeAsync(new UpdateUserRequest(Input.FullName), HttpContext.RequestAborted);
        }

        StatusMessage = "Profile updated.";
        return RedirectToPage();
    }
}
