namespace SmartStock.Models;

public sealed class InventoryProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public int OnHand { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class InventorySupplierEntity
{
    public string Name { get; set; } = string.Empty;
}

public sealed class InventoryTransactionEntity
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
}
