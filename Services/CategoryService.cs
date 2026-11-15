using SmartStock.Models;

namespace SmartStock.Services;

/// <summary>
/// Implementation of ICategoryService.
/// Executes asynchronous operations against the data store.
/// Handles validation and category-product association rules.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly SmartStockDataStore _dataStore;

    public CategoryService(SmartStockDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        var categories = _dataStore.GetCategories();
        foreach (var c in categories)
        {
            var count = _dataStore.GetProductCountForCategory(c.Id);
            // Attach product count by adding empty product placeholders or by service query
            for (int i = 0; i < count; i++)
            {
                c.Products.Add(new Product { CategoryId = c.Id });
            }
        }
        return Task.FromResult<IReadOnlyList<Category>>(categories);
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        var category = _dataStore.GetCategoryById(id);
        if (category != null)
        {
            var count = _dataStore.GetProductCountForCategory(id);
            for (int i = 0; i < count; i++)
            {
                category.Products.Add(new Product { CategoryId = id });
            }
        }
        return Task.FromResult(category);
    }

    public Task<bool> IsCategoryNameUniqueAsync(string name, int? currentId = null)
    {
        var isUnique = _dataStore.IsCategoryNameUnique(name, currentId);
        return Task.FromResult(isUnique);
    }

    public Task<Category> CreateAsync(Category category)
    {
        var created = _dataStore.AddCategory(category);
        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(Category category)
    {
        var updated = _dataStore.UpdateCategory(category);
        return Task.FromResult(updated);
    }

    public Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var result = _dataStore.DeleteCategory(id);
        return Task.FromResult(result);
    }

    public Task<(Category? Category, IReadOnlyList<Product> Products)> GetCategoryWithProductsAsync(int categoryId)
    {
        var category = _dataStore.GetCategoryById(categoryId);
        if (category == null)
        {
            return Task.FromResult<(Category?, IReadOnlyList<Product>)>((null, []));
        }

        var products = _dataStore.GetProducts()
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToList();

        return Task.FromResult<(Category?, IReadOnlyList<Product>)>((category, products));
    }
}

