using System.ComponentModel.DataAnnotations;

namespace SmartStock.ViewModels;

/// <summary>
/// Display model for item in category list.
/// </summary>
public class CategoryItemViewModel
{
    public int Id { get; set; }

    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Products")]
    public int ProductCount { get; set; }

    [Display(Name = "Created On")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Container view model for category index page.
/// </summary>
public class CategoryListViewModel
{
    public IReadOnlyList<CategoryItemViewModel> Categories { get; set; } = [];
    public int TotalCategories => Categories.Count;
    public int TotalAssociatedProducts => Categories.Sum(c => c.ProductCount);
}

/// <summary>
/// Form view model for creating and updating a category.
/// </summary>
public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public bool IsEdit => Id > 0;
}

/// <summary>
/// Detail view model for category-wise product view.
/// </summary>
public class CategoryDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount => Products.Count;
    public IReadOnlyList<ProductItemViewModel> Products { get; set; } = [];
}

