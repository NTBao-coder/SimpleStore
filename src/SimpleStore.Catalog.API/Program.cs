using SimpleStore.Catalog.API;
using SimpleStore.Catalog.API.Data;
using SimpleStore.Catalog.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");
var app = builder.Build();
app.MapDefaultEndpoints();
app.MapCatalogEndpoints();
using (var scope = app.Services.CreateScope())
{
    await CatalogSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<CatalogDbContext>());
}
app.Run();
