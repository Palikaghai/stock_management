using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartStock.ViewModels;

/// <summary>
/// Display model for an individual product in tables and lists.
/// </summary>
public class ProductItemViewModel
{
    public int Id { get; set; }

    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [Display(Name = "Category")]
    public string CategoryName { get; set; } = string.Empty;

    [Display(Name = "Unit Price")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Reorder Level")]
    public int ReorderLevel { get; set; }

    public string? Supplier { get; set; }

    public bool IsLowStock => Quantity <= ReorderLevel;
}

/// <summary>
/// Simple category dropdown helper item.
/// </summary>
public record CategoryDropdownItem(int Id, string Name);

/// <summary>
/// Container view model for product list page with search, category filtering, and pagination.
/// </summary>
public class ProductListViewModel
{
    public IReadOnlyList<ProductItemViewModel> Products { get; set; } = [];

    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 8));

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string? SearchTerm { get; set; }
    public int? CategoryFilter { get; set; }

    public IReadOnlyList<CategoryDropdownItem> AvailableCategories { get; set; } = [];

    // Summary counts for stats bar
    public int TotalProductCount { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalStockValue { get; set; }
}

/// <summary>
/// Form view model for creating and updating a product.
/// </summary>
public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "SKU must be between 2 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "SKU can only contain letters, numbers, hyphens, and underscores.")]
    [Display(Name = "SKU (Stock Keeping Unit)")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category from the list.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Unit price is required.")]
    [Range(0, 10000000.00, ErrorMessage = "Unit price cannot be negative.")]
    [Display(Name = "Unit Price (₹)")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, 10000000, ErrorMessage = "Quantity cannot be negative.")]
    [Display(Name = "On-Hand Quantity")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Reorder level is required.")]
    [Range(0, 10000000, ErrorMessage = "Reorder level cannot be negative.")]
    [Display(Name = "Reorder Level")]
    public int ReorderLevel { get; set; }

    [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters.")]
    [Display(Name = "Supplier (Optional)")]
    public string? Supplier { get; set; }

    public IEnumerable<SelectListItem>? CategoryOptions { get; set; }

    public bool IsEdit => Id > 0;
}

