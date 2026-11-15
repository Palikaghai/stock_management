using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Services;

/// <summary>
/// Thread-safe in-memory data store for Product and Category entities.
/// Seeds baseline data from existing InventoryData while providing full dynamic CRUD.
/// Serves as the operational data provider pending Member 1's EF Core / LocalDB integration.
/// </summary>
public class SmartStockDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<Category> _categories = [];
    private readonly List<Product> _products = [];
    private int _nextCategoryId = 1;
    private int _nextProductId = 1;

    public SmartStockDataStore()
    {
        SeedData();
    }

    private void SeedData()
    {
        lock (_syncRoot)
        {
            // Seed Categories from InventoryData.Categories
            var categoryNames = InventoryData.Categories;
            var categoryMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);

            foreach (var catName in categoryNames)
            {
                var category = new Category
                {
                    Id = _nextCategoryId++,
                    Name = catName,
                    Description = $"Standard inventory category for {catName.ToLowerInvariant()} items.",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                };
                _categories.Add(category);
                categoryMap[catName] = category;
            }

            // Seed Products from InventoryData.Products
            foreach (var p in InventoryData.Products)
            {
                var categoryId = categoryMap.TryGetValue(p.Category, out var c) ? c.Id : 1;
                var product = new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    CategoryId = categoryId,
                    UnitPrice = p.UnitCost,
                    Quantity = p.OnHand,
                    ReorderLevel = p.ReorderLevel,
                    Supplier = p.Supplier,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                };
                _products.Add(product);
                if (p.Id >= _nextProductId)
                {
                    _nextProductId = p.Id + 1;
                }
            }
        }
    }

    // --- CATEGORY OPERATIONS ---

    public List<Category> GetCategories()
    {
        lock (_syncRoot)
        {
            return _categories.Select(CloneCategory).ToList();
        }
    }

    public Category? GetCategoryById(int id)
    {
        lock (_syncRoot)
        {
            var cat = _categories.FirstOrDefault(c => c.Id == id);
            return cat != null ? CloneCategory(cat) : null;
        }
    }

    public bool IsCategoryNameUnique(string name, int? excludeId = null)
    {
        lock (_syncRoot)
        {
            return !_categories.Any(c =>
                c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || c.Id != excludeId.Value));
        }
    }

    public Category AddCategory(Category category)
    {
        lock (_syncRoot)
        {
            category.Id = _nextCategoryId++;
            category.CreatedAt = DateTime.UtcNow;
            _categories.Add(CloneCategory(category));
            return CloneCategory(category);
        }
    }

    public bool UpdateCategory(Category category)
    {
        lock (_syncRoot)
        {
            var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing == null) return false;

            existing.Name = category.Name.Trim();
            existing.Description = category.Description?.Trim();
            return true;
        }
    }

    public (bool Success, string? ErrorMessage) DeleteCategory(int id)
    {
        lock (_syncRoot)
        {
            var existing = _categories.FirstOrDefault(c => c.Id == id);
            if (existing == null) return (false, "Category not found.");

            // Referential integrity: prevent deletion if category has associated products
            var associatedCount = _products.Count(p => p.CategoryId == id);
            if (associatedCount > 0)
            {
                return (false, $"Cannot delete category '{existing.Name}' because it has {associatedCount} associated product(s). Please reassign or delete these products first.");
            }

            _categories.Remove(existing);
            return (true, null);
        }
    }

    // --- PRODUCT OPERATIONS ---

    public List<Product> GetProducts()
    {
        lock (_syncRoot)
        {
            var categories = _categories.ToDictionary(c => c.Id);
            return _products.Select(p =>
            {
                var clone = CloneProduct(p);
                if (categories.TryGetValue(p.CategoryId, out var cat))
                {
                    clone.Category = CloneCategory(cat);
                }
                return clone;
            }).ToList();
        }
    }

    public Product? GetProductById(int id)
    {
        lock (_syncRoot)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

            var clone = CloneProduct(product);
            var cat = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
            if (cat != null)
            {
                clone.Category = CloneCategory(cat);
            }
            return clone;
        }
    }

    public bool IsSkuUnique(string sku, int? excludeId = null)
    {
        lock (_syncRoot)
        {
            return !_products.Any(p =>
                p.Sku.Equals(sku.Trim(), StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || p.Id != excludeId.Value));
        }
    }

    public Product AddProduct(Product product)
    {
        lock (_syncRoot)
        {
            product.Id = _nextProductId++;
            product.CreatedAt = DateTime.UtcNow;
            product.Sku = product.Sku.Trim().ToUpperInvariant();
            product.Name = product.Name.Trim();
            _products.Add(CloneProduct(product));

            var cat = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
            var result = CloneProduct(product);
            if (cat != null) result.Category = CloneCategory(cat);
            return result;
        }
    }

    public bool UpdateProduct(Product product)
    {
        lock (_syncRoot)
        {
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing == null) return false;

            existing.Name = product.Name.Trim();
            existing.Sku = product.Sku.Trim().ToUpperInvariant();
            existing.CategoryId = product.CategoryId;
            existing.UnitPrice = product.UnitPrice;
            existing.Quantity = product.Quantity;
            existing.ReorderLevel = product.ReorderLevel;
            existing.Supplier = product.Supplier?.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }

    public (bool Success, string? ErrorMessage) DeleteProduct(int id)
    {
        lock (_syncRoot)
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null) return (false, "Product not found.");

            // Foreign key & business rule: prevent deletion if transaction history exists
            var hasTransactions = InventoryData.Transactions.Any(t => t.ProductId == id);
            if (hasTransactions)
            {
                return (false, $"Cannot delete product '{existing.Name}' (SKU: {existing.Sku}) because transaction history exists for it. Foreign key constraints protect transaction integrity.");
            }

            _products.Remove(existing);
            return (true, null);
        }
    }

    public int GetProductCountForCategory(int categoryId)
    {
        lock (_syncRoot)
        {
            return _products.Count(p => p.CategoryId == categoryId);
        }
    }

    private static Category CloneCategory(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        CreatedAt = c.CreatedAt
    };

    private static Product CloneProduct(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Sku = p.Sku,
        CategoryId = p.CategoryId,
        UnitPrice = p.UnitPrice,
        Quantity = p.Quantity,
        ReorderLevel = p.ReorderLevel,
        Supplier = p.Supplier,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}

