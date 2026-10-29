using SimpleStore.Catalog.API;
using SimpleStore.Catalog.API.Data;
using SimpleStore.Catalog.API.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleStore.Catalog.API.Auth;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");
var jwt = JwtSettings.FromConfiguration(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = jwt.ValidationParameters());
builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireRole("Admin")));
var app = builder.Build();
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapCatalogEndpoints();
using (var scope = app.Services.CreateScope())
{
    await CatalogSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<CatalogDbContext>());
}
app.Run();
