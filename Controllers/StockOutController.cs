using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class StockOutController : Controller
{
    private readonly InventoryStore _inventory;

    public StockOutController(InventoryStore inventory)
    {
        _inventory = inventory;
    }

    [HttpGet]
    public IActionResult Index() => View(CreateViewModel(new StockMovementViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(StockMovementViewModel model)
    {
        if (ModelState.IsValid && !_inventory.TryRecordMovement(model.ProductId, "OUT", model.Quantity, model.Reference, out var error))
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (!ModelState.IsValid)
        {
            return View(CreateViewModel(model));
        }

        TempData["SuccessMessage"] = "Stock issued and inventory updated.";
        return RedirectToAction(nameof(Index));
    }

    private StockMovementViewModel CreateViewModel(StockMovementViewModel model) => new()
    {
        ProductId = model.ProductId,
        Quantity = model.Quantity,
        Reference = model.Reference,
        Products = _inventory.Products.OrderBy(product => product.Name).ToArray(),
        RecentTransactions = _inventory.Transactions.Where(transaction => transaction.Type == "OUT").Take(8).ToArray()
    };
}
