namespace SmartStock.Models;

/// <summary>
/// Domain model for product categories in SmartStock.
/// Managed by Member 2 (Product & Category Management Module).
/// Prepared for EF Core mapping once Member 1 configures ApplicationDbContext.
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for associated products.
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

