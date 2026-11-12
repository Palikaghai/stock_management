namespace SmartStock.ViewModels;

public sealed class DashboardViewModel
{
    public int TotalProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int LowStockCount { get; init; }
    public int TotalSuppliers { get; init; }
    public IReadOnlyList<InventoryTransaction> RecentTransactions { get; init; } = [];
    public IReadOnlyList<InventoryProduct> LowStockProducts { get; init; } = [];
    public IReadOnlyList<string> CategoryLabels { get; init; } = [];
    public IReadOnlyList<int> CategoryValues { get; init; } = [];
    public IReadOnlyList<string> MovementLabels { get; init; } = [];
    public IReadOnlyList<int> StockInValues { get; init; } = [];
    public IReadOnlyList<int> StockOutValues { get; init; } = [];
    public IReadOnlyList<decimal> ValuationValues { get; init; } = [];
    public IReadOnlyList<string> MonthLabels { get; init; } = [];
    public IReadOnlyList<int> MonthlyInValues { get; init; } = [];
    public IReadOnlyList<int> MonthlyOutValues { get; init; } = [];
}
