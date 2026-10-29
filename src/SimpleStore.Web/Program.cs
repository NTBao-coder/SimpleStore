using SimpleStore.Catalog.API.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;
using SimpleStore.Web.Services;
using SimpleStore.Identity.API.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.EventsType = typeof(JwtCookieAuthenticationEvents);
    });
builder.Services.AddAuthorization();

// Add DbContext
builder.AddCatalogApiClient();
builder.Services.AddIdentityApiClient();
builder.AddNpgsqlDbContext<OrderDbContext>("orderdb");

// Session for cart
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

// Register services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var orders = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await orders.Database.MigrateAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
