using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleStore.Web.Services;
using SimpleStore.Web.ViewModels;

namespace SimpleStore.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    private readonly ICartService _cart;
    public OrdersController(IOrderService orders, ICartService cart)
    {
        _orders = orders;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var orders = await _orders.GetUserOrdersAsync(userId);
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orders.GetOrderByIdAsync(id, userId);
        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Checkout()
    {
        var items = await _cart.GetCartItemsAsync();
        if (!items.Any()) return RedirectToAction("Index", "Cart");
        var model = new CheckoutViewModel
        {
            CartItems = items,
            Total = items.Sum(i => i.TotalPrice),
            FullName = User.Identity?.Name ?? string.Empty,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CartItems = await _cart.GetCartItemsAsync();
            model.Total = model.CartItems.Sum(i => i.TotalPrice);
            return View(model);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var items = await _cart.GetCartItemsAsync();
        var order = await _orders.CreateOrderAsync(userId, model.ShippingAddress, items);
        await _cart.ClearCartAsync();
        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orders.GetOrderByIdAsync(id, userId);
        if (order == null) return NotFound();
        return View(order);
    }
}
