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
        var productGroups = _inventory.Products
            .GroupBy(product => product.Supplier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        var groups = _inventory.Suppliers
            .Select(supplier => productGroups.TryGetValue(supplier, out var products)
                ? new InventoryGroupViewModel
                {
                    Name = supplier,
                    ProductCount = products.Length,
                    UnitsOnHand = products.Sum(product => product.OnHand),
                    StockValue = products.Sum(product => product.OnHand * product.UnitCost),
                    LowStockCount = products.Count(product => product.OnHand <= product.ReorderLevel)
                }
                : new InventoryGroupViewModel { Name = supplier })
            .OrderBy(group => group.Name)
            .ToArray();

        return View(groups);
    }

    [HttpGet]
    public IActionResult Create() => View(new SupplierFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _inventory.CreateSupplier(model.Name);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Supplier created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(string name)
    {
        var supplier = _inventory.Suppliers
            .FirstOrDefault(candidate => string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase));
        if (supplier == null)
        {
            return NotFound();
        }

        return View(new SupplierFormViewModel { Name = supplier, OriginalName = supplier });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _inventory.UpdateSupplier(model.OriginalName, model.Name);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string name)
    {
        try
        {
            _inventory.DeleteSupplier(name);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Supplier deleted.";
        return RedirectToAction(nameof(Index));
    }
}
