namespace SmartStock.ViewModels;

public sealed record InventoryProduct(int Id, string Name, string Sku, string Category, string Supplier, int OnHand, int ReorderLevel, decimal UnitCost);
public sealed record InventoryTransaction(DateTime Date, string Type, int ProductId, int Quantity, string Reference);

public static class InventoryData
{
    public static IReadOnlyList<InventoryProduct> Products { get; } =
    [
        new(1, "Wireless Mouse", "ACC-1042", "Accessories", "TechSource Co.", 18, 12, 24.50m),
        new(2, "Mechanical Keyboard", "ACC-1088", "Accessories", "TechSource Co.", 7, 10, 68m),
        new(3, "27-inch Monitor", "DSP-2011", "Displays", "Northstar Supply", 24, 8, 189m),
        new(4, "USB-C Dock", "ACC-1120", "Accessories", "Apex Distribution", 5, 8, 112m),
        new(5, "Laptop Stand", "FUR-3024", "Office furniture", "Northstar Supply", 31, 10, 39m),
        new(6, "Business Laptop", "CMP-4018", "Computers", "Apex Distribution", 14, 6, 920m),
        new(7, "Noise-canceling Headset", "AUD-5022", "Audio", "Soundwave Partners", 9, 10, 84m),
        new(8, "Webcam Pro", "ACC-1146", "Accessories", "TechSource Co.", 42, 15, 57m),
        new(9, "Portable SSD 1TB", "STO-6012", "Storage", "Apex Distribution", 16, 8, 96m),
        new(10, "Desk Lamp", "FUR-3061", "Office furniture", "Northstar Supply", 3, 8, 28m)
    ];

    public static IReadOnlyList<string> Categories { get; } = Products.Select(x => x.Category).Distinct().Order().ToArray();
    public static IReadOnlyList<string> Suppliers { get; } = Products.Select(x => x.Supplier).Distinct().Order().ToArray();
    public static IReadOnlyList<InventoryTransaction> Transactions { get; } = CreateTransactions();

    private static IReadOnlyList<InventoryTransaction> CreateTransactions()
    {
        var today = DateTime.Today;
        return
        [
            new(today.AddHours(-2), "IN", 3, 12, "PO-2841"), new(today.AddHours(-5), "OUT", 2, 4, "SO-1187"),
            new(today.AddDays(-1), "IN", 8, 20, "PO-2836"), new(today.AddDays(-1).AddHours(-3), "OUT", 7, 3, "SO-1182"),
            new(today.AddDays(-2), "OUT", 1, 6, "SO-1176"), new(today.AddDays(-3), "IN", 6, 5, "PO-2828"),
            new(today.AddDays(-4), "IN", 5, 10, "PO-2821"), new(today.AddDays(-5), "OUT", 4, 2, "SO-1163"),
            new(today.AddDays(-8), "IN", 9, 8, "PO-2809"), new(today.AddDays(-12), "OUT", 6, 2, "SO-1148"),
            new(today.AddDays(-18), "IN", 2, 6, "PO-2788"), new(today.AddDays(-24), "OUT", 10, 2, "SO-1129"),
            new(today.AddDays(-38), "IN", 4, 5, "PO-2744"), new(today.AddDays(-48), "OUT", 3, 4, "SO-1081"),
            new(today.AddDays(-65), "IN", 1, 12, "PO-2683"), new(today.AddDays(-76), "OUT", 8, 9, "SO-1034"),
            new(today.AddDays(-95), "IN", 7, 7, "PO-2618"), new(today.AddDays(-112), "OUT", 5, 6, "SO-0975")
        ];
    }
}
