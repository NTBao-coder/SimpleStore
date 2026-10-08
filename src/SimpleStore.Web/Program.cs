var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();
app.MapGet("/", () => "SimpleStore.Web — v-skeleton placeholder");
app.MapDefaultEndpoints();
app.Run();
