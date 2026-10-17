using System.Net;
using System.Net.Http.Json;

namespace SimpleStore.Catalog.API.Client;

public class CatalogApiClient(HttpClient http) : ICatalogApiClient
{
    private async Task<T> ReadAsync<T>(string path, CancellationToken ct) =>
        await http.GetFromJsonAsync<T>(path, ct) ?? throw new HttpRequestException("Empty catalog response.");

    private async Task<T?> FindAsync<T>(string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    private async Task<T> CreateAsync<T>(string path, T value, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(path, value, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new HttpRequestException("Empty catalog response.");
    }

    private async Task UpdateAsync<T>(string path, T value, CancellationToken ct)
    {
        using var response = await http.PutAsJsonAsync(path, value, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var response = await http.DeleteAsync(path, ct);
        response.EnsureSuccessStatusCode();
    }

    public Task<PagedResult<ProductDto>> GetProductsAsync(int page = 1, int pageSize = 20, int? categoryId = null, string? search = null, CancellationToken cancellationToken = default) =>
        ReadAsync<PagedResult<ProductDto>>($"api/catalog/products?page={page}&pageSize={pageSize}"
            + (categoryId.HasValue ? $"&categoryId={categoryId.Value}" : string.Empty)
            + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"), cancellationToken);
    public Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindAsync<ProductDto>($"api/catalog/products/{id}", cancellationToken);
    public Task<int> GetProductCountAsync(CancellationToken cancellationToken = default) =>
        ReadAsync<int>("api/catalog/products/count", cancellationToken);
    public Task<ProductDto> CreateProductAsync(ProductDto value, CancellationToken cancellationToken = default) =>
        CreateAsync("api/catalog/products", value, cancellationToken);
    public Task UpdateProductAsync(int id, ProductDto value, CancellationToken cancellationToken = default) =>
        UpdateAsync($"api/catalog/products/{id}", value, cancellationToken);
    public Task DeleteProductAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/catalog/products/{id}", cancellationToken);

    public Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        ReadAsync<PagedResult<CategoryDto>>($"api/catalog/categories?page={page}&pageSize={pageSize}", cancellationToken);
    public Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindAsync<CategoryDto>($"api/catalog/categories/{id}", cancellationToken);
    public Task<int> GetCategoryCountAsync(CancellationToken cancellationToken = default) =>
        ReadAsync<int>("api/catalog/categories/count", cancellationToken);
    public Task<CategoryDto> CreateCategoryAsync(CategoryDto value, CancellationToken cancellationToken = default) =>
        CreateAsync("api/catalog/categories", value, cancellationToken);
    public Task UpdateCategoryAsync(int id, CategoryDto value, CancellationToken cancellationToken = default) =>
        UpdateAsync($"api/catalog/categories/{id}", value, cancellationToken);
    public Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/catalog/categories/{id}", cancellationToken);
}
