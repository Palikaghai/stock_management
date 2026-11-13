using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly InventoryStore _inventory;

    public ProductsController(InventoryStore inventory)
    {
        _inventory = inventory;
    }

    public IActionResult Index(string? search, string? category, string? supplier)
    {
        var products = _inventory.Products.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products.Where(product =>
                product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                product.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            products = products.Where(product => string.Equals(product.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(supplier))
        {
            products = products.Where(product => string.Equals(product.Supplier, supplier, StringComparison.OrdinalIgnoreCase));
        }

        return View(new ProductListViewModel
        {
            Products = products.OrderBy(product => product.Name).ToArray(),
            Categories = _inventory.Categories,
            Suppliers = _inventory.Suppliers,
            Search = search,
            Category = category,
            Supplier = supplier
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new ProductFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _inventory.CreateProduct(model.Name, model.Sku, model.Category, model.Supplier, model.ReorderLevel, model.UnitCost);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Sku), exception.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Product created. Record an IN movement to add stock.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _inventory.FindProduct(id);
        if (product == null)
        {
            return NotFound();
        }

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Category = product.Category,
            Supplier = product.Supplier,
            ReorderLevel = product.ReorderLevel,
            UnitCost = product.UnitCost
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _inventory.UpdateProduct(model.Id, model.Name, model.Sku, model.Category, model.Supplier, model.ReorderLevel, model.UnitCost);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Sku), exception.Message);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Product details updated.";
        return RedirectToAction(nameof(Index));
    }
}
