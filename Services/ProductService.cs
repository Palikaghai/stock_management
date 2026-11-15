using SmartStock.Models;

namespace SmartStock.Services;

/// <summary>
/// Implementation of IProductService.
/// Supports search by Name/SKU, category filtering, pagination, and validation.
/// </summary>
public class ProductService : IProductService
{
    private readonly SmartStockDataStore _dataStore;

    public ProductService(SmartStockDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(
        string? search, int? categoryId, int page, int pageSize)
    {
        var query = _dataStore.GetProducts().AsEnumerable();

        // Search by Product Name and SKU (partial, case-insensitive)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // Category-wise filtering
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var list = query.OrderBy(p => p.Name).ToList();
        var totalCount = list.Count;

        // Apply pagination
        var safePage = page > 0 ? page : 1;
        var safePageSize = pageSize > 0 ? pageSize : 8;
        var pagedItems = list
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<Product> Items, int TotalCount)>((pagedItems, totalCount));
    }

    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        var products = _dataStore.GetProducts();
        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _dataStore.GetProductById(id);
        return Task.FromResult(product);
    }

    public Task<bool> IsSkuUniqueAsync(string sku, int? currentId = null)
    {
        var isUnique = _dataStore.IsSkuUnique(sku, currentId);
        return Task.FromResult(isUnique);
    }

    public Task<Product> CreateAsync(Product product)
    {
        var created = _dataStore.AddProduct(product);
        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(Product product)
    {
        var updated = _dataStore.UpdateProduct(product);
        return Task.FromResult(updated);
    }

    public Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var result = _dataStore.DeleteProduct(id);
        return Task.FromResult(result);
    }

    public Task<(int TotalCount, int LowStockCount, decimal TotalStockValue)> GetMetricsAsync()
    {
        var products = _dataStore.GetProducts();
        var count = products.Count;
        var lowStock = products.Count(p => p.Quantity <= p.ReorderLevel);
        var value = products.Sum(p => p.Quantity * p.UnitPrice);
        return Task.FromResult((count, lowStock, value));
    }
}

