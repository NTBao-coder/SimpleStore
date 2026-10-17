using Microsoft.AspNetCore.Mvc;
using SimpleStore.Catalog.API.Client;

namespace SimpleStore.Web.Controllers;

public class CatalogController : Controller
{
    private readonly ICatalogApiClient _catalog;
    public CatalogController(ICatalogApiClient catalog) => _catalog = catalog;

    public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
    {
        ViewBag.Categories = await _catalog.GetAllCategoriesAsync();
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SearchTerm = search;
        var products = await _catalog.GetProductsAsync(page: page, pageSize: 12, categoryId: categoryId, search: search);
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _catalog.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }
}
