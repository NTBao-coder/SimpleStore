using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SimpleStore.Catalog.API.Client;

public static class CatalogClientExtensions
{
    public static IHttpClientBuilder AddCatalogApiClient(this IHostApplicationBuilder builder,
        string serviceName = "catalog-api") =>
        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
            client.BaseAddress = new Uri($"https+http://{serviceName}"));
}
