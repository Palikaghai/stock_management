using SmartStock.Models;

namespace SmartStock.Services;

/// <summary>
/// Service abstraction for Product management operations.
/// Follows Dependency Injection and Asynchronous pattern.
/// Compatible with EF Core ApplicationDbContext when Member 1 configures it.
/// </summary>
public interface IProductService
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(string? search, int? categoryId, int page, int pageSize);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<bool> IsSkuUniqueAsync(string sku, int? currentId = null);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(int TotalCount, int LowStockCount, decimal TotalStockValue)> GetMetricsAsync();
}

