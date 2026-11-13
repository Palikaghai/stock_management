using System.ComponentModel.DataAnnotations;

namespace SmartStock.ViewModels;

public sealed class ProductFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Supplier { get; set; } = string.Empty;

    [Range(0, 1000000)]
    [Display(Name = "Reorder level")]
    public int ReorderLevel { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000")]
    [Display(Name = "Unit cost")]
    public decimal UnitCost { get; set; }
}

public sealed class ProductListViewModel
{
    public IReadOnlyList<InventoryProduct> Products { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<string> Suppliers { get; init; } = [];
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Supplier { get; init; }
}

public sealed class InventoryGroupViewModel
{
    public string Name { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public int UnitsOnHand { get; init; }
    public decimal StockValue { get; init; }
    public int LowStockCount { get; init; }
}

public sealed class StockMovementViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a product.")]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    [StringLength(60)]
    [Display(Name = "Reference (optional)")]
    public string? Reference { get; set; }

    public IReadOnlyList<InventoryProduct> Products { get; init; } = [];
    public IReadOnlyList<InventoryTransaction> RecentTransactions { get; init; } = [];
}

public sealed class TransactionListViewModel
{
    public string? Type { get; init; }
    public IReadOnlyList<InventoryTransaction> Transactions { get; init; } = [];
    public IReadOnlyList<InventoryProduct> Products { get; init; } = [];
}
