namespace SmartStock.Models;

/// <summary>
/// Domain model for products in SmartStock.
/// Managed by Member 2 (Product & Category Management Module).
/// Prepared for EF Core mapping once Member 1 configures ApplicationDbContext.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public int ReorderLevel { get; set; }

    public string? Supplier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

