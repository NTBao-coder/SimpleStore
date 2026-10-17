namespace SimpleStore.Catalog.API.Client;

public static class CatalogQueries
{
    // Selection lists need all categories; traverse bounded API pages instead of silently truncating.
    public static async Task<List<CategoryDto>> GetAllCategoriesAsync(this ICatalogApiClient client,
        CancellationToken cancellationToken = default)
    {
        var items = new List<CategoryDto>();
        PagedResult<CategoryDto> result;
        var page = 1;
        do
        {
            result = await client.GetCategoriesAsync(page++, 100, cancellationToken);
            items.AddRange(result.Items);
        } while (result.Page < result.TotalPages);
        return items;
    }
}
