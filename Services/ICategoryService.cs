using SmartStock.Models;

namespace SmartStock.Services;

/// <summary>
/// Service abstraction for Category management operations.
/// Follows Dependency Injection and Asynchronous pattern.
/// Compatible with EF Core ApplicationDbContext when Member 1 configures it.
/// </summary>
public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> IsCategoryNameUniqueAsync(string name, int? currentId = null);
    Task<Category> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(Category? Category, IReadOnlyList<Product> Products)> GetCategoryWithProductsAsync(int categoryId);
}

