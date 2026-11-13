using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly InventoryStore _inventory;

    public SuppliersController(InventoryStore inventory)
    {
        _inventory = inventory;
    }

    public IActionResult Index()
    {
        var groups = _inventory.Products
            .GroupBy(product => product.Supplier, StringComparer.OrdinalIgnoreCase)
            .Select(group => new InventoryGroupViewModel
            {
                Name = group.Key,
                ProductCount = group.Count(),
                UnitsOnHand = group.Sum(product => product.OnHand),
                StockValue = group.Sum(product => product.OnHand * product.UnitCost),
                LowStockCount = group.Count(product => product.OnHand <= product.ReorderLevel)
            })
            .OrderBy(group => group.Name)
            .ToArray();

        return View(groups);
    }
}
