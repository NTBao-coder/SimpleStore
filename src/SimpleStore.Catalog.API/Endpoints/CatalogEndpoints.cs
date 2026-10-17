using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Catalog.API.Client;
using SimpleStore.Catalog.API.Data;
using SimpleStore.Catalog.API.Models;

namespace SimpleStore.Catalog.API.Endpoints;

public static class CatalogEndpoints
{
    private static readonly Expression<Func<Product, ProductDto>> ProductProjection = p => new ProductDto
    {
        Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price,
        Stock = p.Stock, ImageUrl = p.ImageUrl, CategoryId = p.CategoryId, CategoryName = p.Category.Name
    };
    private static readonly Expression<Func<Category, CategoryDto>> CategoryProjection = c => new CategoryDto
    {
        Id = c.Id, Name = c.Name, Description = c.Description, ProductCount = c.Products.Count
    };

    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/catalog/products");
        products.MapGet("", async (CatalogDbContext db, int page = 1, int pageSize = 20,
            int? categoryId = null, string? search = null, CancellationToken ct = default) =>
        {
            if (!ValidPage(page, pageSize)) return Results.BadRequest("Invalid pagination.");
            var query = db.Products.AsNoTracking();
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            var count = await query.CountAsync(ct);
            var items = await query.OrderBy(p => p.Name).ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).Select(ProductProjection).ToListAsync(ct);
            return Results.Ok(new PagedResult<ProductDto>(items, count, page, pageSize));
        });
        products.MapGet("/count", async (CatalogDbContext db, CancellationToken ct) => await db.Products.CountAsync(ct));
        products.MapGet("/{id:int}", async (int id, CatalogDbContext db, CancellationToken ct) =>
        {
            var value = await db.Products.AsNoTracking().Where(p => p.Id == id).Select(ProductProjection).SingleOrDefaultAsync(ct);
            return value is null ? Results.NotFound() : Results.Ok(value);
        });
        products.MapPost("", async (ProductDto value, CatalogDbContext db, CancellationToken ct) =>
        {
            if (!ValidProduct(value) || !await db.Categories.AnyAsync(c => c.Id == value.CategoryId, ct))
                return Results.BadRequest("Name, non-negative price/stock and an existing category are required.");
            var product = new Product();
            Apply(product, value);
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);
            var result = await db.Products.Where(p => p.Id == product.Id).Select(ProductProjection).SingleAsync(ct);
            return Results.Created($"/api/catalog/products/{product.Id}", result);
        });
        products.MapPut("/{id:int}", async (int id, ProductDto value, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products.FindAsync([id], ct);
            if (product is null) return Results.NotFound();
            if (!ValidProduct(value) || !await db.Categories.AnyAsync(c => c.Id == value.CategoryId, ct))
                return Results.BadRequest("Name, non-negative price/stock and an existing category are required.");
            Apply(product, value);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        products.MapDelete("/{id:int}", async (int id, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products.FindAsync([id], ct);
            if (product is null) return Results.NotFound();
            db.Products.Remove(product);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var categories = app.MapGroup("/api/catalog/categories");
        categories.MapGet("", async (CatalogDbContext db, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        {
            if (!ValidPage(page, pageSize)) return Results.BadRequest("Invalid pagination.");
            var count = await db.Categories.CountAsync(ct);
            var items = await db.Categories.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).Select(CategoryProjection).ToListAsync(ct);
            return Results.Ok(new PagedResult<CategoryDto>(items, count, page, pageSize));
        });
        categories.MapGet("/count", async (CatalogDbContext db, CancellationToken ct) => await db.Categories.CountAsync(ct));
        categories.MapGet("/{id:int}", async (int id, CatalogDbContext db, CancellationToken ct) =>
        {
            var value = await db.Categories.AsNoTracking().Where(c => c.Id == id).Select(CategoryProjection).SingleOrDefaultAsync(ct);
            return value is null ? Results.NotFound() : Results.Ok(value);
        });
        categories.MapPost("", async (CategoryDto value, CatalogDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(value.Name)) return Results.BadRequest("Name is required.");
            var category = new Category { Name = value.Name.Trim(), Description = value.Description ?? string.Empty };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/catalog/categories/{category.Id}", new CategoryDto
            { Id = category.Id, Name = category.Name, Description = category.Description });
        });
        categories.MapPut("/{id:int}", async (int id, CategoryDto value, CatalogDbContext db, CancellationToken ct) =>
        {
            var category = await db.Categories.FindAsync([id], ct);
            if (category is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(value.Name)) return Results.BadRequest("Name is required.");
            category.Name = value.Name.Trim();
            category.Description = value.Description ?? string.Empty;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        categories.MapDelete("/{id:int}", async (int id, CatalogDbContext db, CancellationToken ct) =>
        {
            var category = await db.Categories.FindAsync([id], ct);
            if (category is null) return Results.NotFound();
            if (await db.Products.AnyAsync(p => p.CategoryId == id, ct))
                return Results.Conflict("Remove the category's products first.");
            db.Categories.Remove(category);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static bool ValidPage(int page, int size) => page > 0 && size is > 0 and <= 100 && (long)(page - 1) * size <= int.MaxValue;
    private static bool ValidProduct(ProductDto p) => !string.IsNullOrWhiteSpace(p.Name) && p.Price >= 0 && p.Stock >= 0;
    private static void Apply(Product product, ProductDto value)
    {
        product.Name = value.Name.Trim();
        product.Description = value.Description ?? string.Empty;
        product.Price = value.Price;
        product.Stock = value.Stock;
        product.ImageUrl = value.ImageUrl ?? string.Empty;
        product.CategoryId = value.CategoryId;
    }
}
