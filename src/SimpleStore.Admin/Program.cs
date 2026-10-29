using SimpleStore.Catalog.API.Client;
using SimpleStore.Admin.Components;
using SimpleStore.Identity.API.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using SimpleStore.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.EventsType = typeof(JwtCookieAuthenticationEvents);
    });
builder.Services.AddAuthorization();

builder.AddCatalogApiClient();
builder.Services.AddIdentityApiClient();
builder.AddNpgsqlDbContext<OrderDbContext>("orderdb");

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapPost("/auth/login", async (HttpContext context, IIdentityApiClient identity, ITokenSessionStore sessions) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var response = await identity.LoginAsync(new LoginRequest(form["email"].ToString(), form["password"].ToString()), context.RequestAborted);
    if (response is null || !response.User.Roles.Contains("Admin"))
    {
        context.Response.Redirect("/login?error=1");
        return;
    }
    var sessionId = Guid.NewGuid().ToString("N");
    await sessions.SaveAsync(sessionId, new TokenSession(response.AccessToken, response.RefreshToken, response.ExpiresAt), context.RequestAborted);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        JwtCookieAuthenticationEvents.CreatePrincipal(response, sessionId), new AuthenticationProperties { AllowRefresh = true });
    context.Response.Redirect("/");
});
app.MapPost("/auth/logout", async (HttpContext context, IIdentityApiClient identity, ITokenSessionStore sessions) =>
{
    var sessionId = context.User.FindFirst(JwtCookieAuthenticationEvents.SessionIdClaim)?.Value;
    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        var session = await sessions.GetAsync(sessionId, context.RequestAborted);
        if (session is not null) await identity.LogoutAsync(session.RefreshToken, context.RequestAborted);
        await sessions.RemoveAsync(sessionId, context.RequestAborted);
    }
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
