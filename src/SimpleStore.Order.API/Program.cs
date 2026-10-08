var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();
app.MapGet("/", () => "SimpleStore.Order.API — v-skeleton placeholder");
app.MapDefaultEndpoints();
app.Run();
