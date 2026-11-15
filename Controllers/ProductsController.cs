using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartStock.Models;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

/// <summary>
/// Controller for Product Management (CRUD).
/// Handled exclusively by Member 2.
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    // GET: Products
    [HttpGet]
    public async Task<IActionResult> Index(string? search = null, int? categoryId = null, int page = 1)
    {
        const int pageSize = 8;
        var (items, totalCount) = await _productService.GetProductsAsync(search, categoryId, page, pageSize);
        var categories = await _categoryService.GetAllAsync();
        var (totalCatalogCount, lowStockCount, totalStockValue) = await _productService.GetMetricsAsync();

        var model = new ProductListViewModel
        {
            Products = items.Select(p => new ProductItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "Unassigned",
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity,
                ReorderLevel = p.ReorderLevel,
                Supplier = p.Supplier
            }).ToList(),
            CurrentPage = page > 0 ? page : 1,
            PageSize = pageSize,
            TotalItems = totalCount,
            SearchTerm = search,
            CategoryFilter = categoryId,
            AvailableCategories = categories.Select(c => new CategoryDropdownItem(c.Id, c.Name)).OrderBy(c => c.Name).ToList(),
            TotalProductCount = totalCatalogCount,
            LowStockCount = lowStockCount,
            TotalStockValue = totalStockValue
        };

        return View(model);
    }

    // GET: Products/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel
        {
            Quantity = 0,
            ReorderLevel = 10,
            UnitPrice = 0.00m,
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(model);
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        // Business logic validations
        if (model.UnitPrice < 0)
        {
            ModelState.AddModelError(nameof(model.UnitPrice), "Unit price cannot be negative.");
        }

        if (model.Quantity < 0)
        {
            ModelState.AddModelError(nameof(model.Quantity), "Quantity cannot be negative.");
        }

        if (model.ReorderLevel < 0)
        {
            ModelState.AddModelError(nameof(model.ReorderLevel), "Reorder level cannot be negative.");
        }

        // Validate SKU uniqueness
        if (!string.IsNullOrWhiteSpace(model.Sku))
        {
            var isSkuUnique = await _productService.IsSkuUniqueAsync(model.Sku);
            if (!isSkuUnique)
            {
                ModelState.AddModelError(nameof(model.Sku), $"A product with SKU '{model.Sku.ToUpperInvariant()}' already exists. SKU must be unique.");
            }
        }

        // Validate category reference
        var category = await _categoryService.GetByIdAsync(model.CategoryId);
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected category does not exist.");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Sku = model.Sku.Trim().ToUpperInvariant(),
            CategoryId = model.CategoryId,
            UnitPrice = model.UnitPrice,
            Quantity = model.Quantity,
            ReorderLevel = model.ReorderLevel,
            Supplier = string.IsNullOrWhiteSpace(model.Supplier) ? null : model.Supplier.Trim()
        };

        await _productService.CreateAsync(product);

        TempData["SuccessMessage"] = $"Product '{product.Name}' (SKU: {product.Sku}) was successfully added.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0) return NotFound();

        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = "The requested product could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            CategoryId = product.CategoryId,
            UnitPrice = product.UnitPrice,
            Quantity = product.Quantity,
            ReorderLevel = product.ReorderLevel,
            Supplier = product.Supplier,
            CategoryOptions = await BuildCategoryOptionsAsync(product.CategoryId)
        };

        return View(model);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        // Business logic validations
        if (model.UnitPrice < 0)
        {
            ModelState.AddModelError(nameof(model.UnitPrice), "Unit price cannot be negative.");
        }

        if (model.Quantity < 0)
        {
            ModelState.AddModelError(nameof(model.Quantity), "Quantity cannot be negative.");
        }

        if (model.ReorderLevel < 0)
        {
            ModelState.AddModelError(nameof(model.ReorderLevel), "Reorder level cannot be negative.");
        }

        // Validate SKU uniqueness (excluding current product)
        if (!string.IsNullOrWhiteSpace(model.Sku))
        {
            var isSkuUnique = await _productService.IsSkuUniqueAsync(model.Sku, id);
            if (!isSkuUnique)
            {
                ModelState.AddModelError(nameof(model.Sku), $"Another product with SKU '{model.Sku.ToUpperInvariant()}' already exists.");
            }
        }

        // Validate category reference
        var category = await _categoryService.GetByIdAsync(model.CategoryId);
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected category does not exist.");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var product = new Product
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            Sku = model.Sku.Trim().ToUpperInvariant(),
            CategoryId = model.CategoryId,
            UnitPrice = model.UnitPrice,
            Quantity = model.Quantity,
            ReorderLevel = model.ReorderLevel,
            Supplier = string.IsNullOrWhiteSpace(model.Supplier) ? null : model.Supplier.Trim()
        };

        var updated = await _productService.UpdateAsync(product);
        if (!updated)
        {
            TempData["ErrorMessage"] = "Product update failed. The product may have been removed.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Product '{product.Name}' (SKU: {product.Sku}) was successfully updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return NotFound();

        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = "The requested product could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ProductItemViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "Unassigned",
            UnitPrice = product.UnitPrice,
            Quantity = product.Quantity,
            ReorderLevel = product.ReorderLevel,
            Supplier = product.Supplier
        };

        return View(model);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, errorMessage) = await _productService.DeleteAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Unable to delete product due to database relationship restrictions.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Product was successfully deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> BuildCategoryOptionsAsync(int? selectedId = null)
    {
        var categories = await _categoryService.GetAllAsync();
        return categories.OrderBy(c => c.Name).Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = selectedId.HasValue && c.Id == selectedId.Value
        });
    }
}

