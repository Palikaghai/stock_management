namespace SmartStock.ViewModels;

public sealed class ReportsViewModel
{
    public string ReportType { get; init; } = "overall";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Category { get; init; }
    public string? Supplier { get; init; }
    public int? ProductId { get; init; }
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<string> Suppliers { get; init; } = [];
    public IReadOnlyList<InventoryProduct> Products { get; init; } = [];
    public IReadOnlyList<ReportRow> Rows { get; init; } = [];
    public string ReportTitle { get; init; } = "";
    public decimal TotalValue { get; init; }
}

public sealed record ReportRow(string Product, string Sku, string Category, string Supplier, int Quantity, decimal UnitCost, decimal Value, string Status, DateTime? Date, string? Reference);
