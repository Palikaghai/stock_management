using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

/// <summary>
/// Controller for Category Management (CRUD).
/// Handled exclusively by Member 2.
/// </summary>
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: Categories
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        var model = new CategoryListViewModel
        {
            Categories = categories.Select(c => new CategoryItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count,
                CreatedAt = c.CreatedAt
            }).OrderBy(c => c.Name).ToList()
        };

        return View(model);
    }

    // GET: Categories/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate uniqueness of category name
        var isUnique = await _categoryService.IsCategoryNameUniqueAsync(model.Name);
        if (!isUnique)
        {
            ModelState.AddModelError(nameof(model.Name), $"A category named '{model.Name}' already exists.");
            return View(model);
        }

        var category = new Category
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim()
        };

        await _categoryService.CreateAsync(category);

        TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully created.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0) return NotFound();

        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "The requested category could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return View(model);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate uniqueness of category name
        var isUnique = await _categoryService.IsCategoryNameUniqueAsync(model.Name, id);
        if (!isUnique)
        {
            ModelState.AddModelError(nameof(model.Name), $"Another category named '{model.Name}' already exists.");
            return View(model);
        }

        var category = new Category
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            Description = model.Description?.Trim()
        };

        var updated = await _categoryService.UpdateAsync(category);
        if (!updated)
        {
            TempData["ErrorMessage"] = "Category update failed. It may have been removed.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return NotFound();

        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "The requested category could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryItemViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products.Count,
            CreatedAt = category.CreatedAt
        };

        return View(model);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, errorMessage) = await _categoryService.DeleteAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Unable to delete category due to database relationship restrictions.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Category was successfully deleted.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Products/5
    // Category-wise product view
    [HttpGet]
    public async Task<IActionResult> Products(int id)
    {
        if (id <= 0) return NotFound();

        var (category, products) = await _categoryService.GetCategoryWithProductsAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "The requested category could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryDetailViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Products = products.Select(p => new ProductItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                CategoryName = category.Name,
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity,
                ReorderLevel = p.ReorderLevel,
                Supplier = p.Supplier
            }).ToList()
        };

        return View(model);
    }
}

