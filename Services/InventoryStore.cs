using SmartStock.ViewModels;

namespace SmartStock.Services;

public sealed class InventoryStore
{
    private readonly object _sync = new();
    private readonly List<InventoryProduct> _products = InventoryData.Products.ToList();
    private readonly List<InventoryTransaction> _transactions = InventoryData.Transactions.ToList();
    private int _nextProductId = InventoryData.Products.Max(product => product.Id) + 1;

    public IReadOnlyList<InventoryProduct> Products
    {
        get { lock (_sync) return _products.ToArray(); }
    }

    public IReadOnlyList<InventoryTransaction> Transactions
    {
        get { lock (_sync) return _transactions.OrderByDescending(transaction => transaction.Date).ToArray(); }
    }

    public IReadOnlyList<string> Categories
    {
        get { lock (_sync) return _products.Select(product => product.Category).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray(); }
    }

    public IReadOnlyList<string> Suppliers
    {
        get { lock (_sync) return _products.Select(product => product.Supplier).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray(); }
    }

    public InventoryProduct? FindProduct(int id)
    {
        lock (_sync)
        {
            return _products.FirstOrDefault(product => product.Id == id);
        }
    }

    public InventoryProduct CreateProduct(string name, string sku, string category, string supplier, int reorderLevel, decimal unitCost)
    {
        lock (_sync)
        {
            EnsureSkuAvailable(sku, null);
            var product = new InventoryProduct(_nextProductId++, name.Trim(), sku.Trim(), category.Trim(), supplier.Trim(), 0, reorderLevel, unitCost);
            _products.Add(product);
            return product;
        }
    }

    public void UpdateProduct(int id, string name, string sku, string category, string supplier, int reorderLevel, decimal unitCost)
    {
        lock (_sync)
        {
            var index = _products.FindIndex(product => product.Id == id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Product {id} was not found.");
            }

            EnsureSkuAvailable(sku, id);
            var current = _products[index];
            _products[index] = current with
            {
                Name = name.Trim(),
                Sku = sku.Trim(),
                Category = category.Trim(),
                Supplier = supplier.Trim(),
                ReorderLevel = reorderLevel,
                UnitCost = unitCost
            };
        }
    }

    public bool TryRecordMovement(int productId, string type, int quantity, string? reference, out string error)
    {
        lock (_sync)
        {
            var index = _products.FindIndex(product => product.Id == productId);
            if (index < 0)
            {
                error = "Select a valid product.";
                return false;
            }

            if (quantity <= 0)
            {
                error = "Quantity must be greater than zero.";
                return false;
            }

            var product = _products[index];
            var isStockIn = string.Equals(type, "IN", StringComparison.Ordinal);
            if (!isStockIn && !string.Equals(type, "OUT", StringComparison.Ordinal))
            {
                error = "The stock movement type is invalid.";
                return false;
            }

            if (!isStockIn && quantity > product.OnHand)
            {
                error = $"Only {product.OnHand} units are available for {product.Name}.";
                return false;
            }

            if (isStockIn && quantity > int.MaxValue - product.OnHand)
            {
                error = "The resulting stock quantity is too large.";
                return false;
            }

            var updatedQuantity = isStockIn ? product.OnHand + quantity : product.OnHand - quantity;
            _products[index] = product with { OnHand = updatedQuantity };
            var movementReference = string.IsNullOrWhiteSpace(reference)
                ? $"{(isStockIn ? "IN" : "OUT")}-{Guid.NewGuid():N}"[..11].ToUpperInvariant()
                : reference.Trim();
            _transactions.Add(new InventoryTransaction(DateTime.Now, type, productId, quantity, movementReference));
            error = string.Empty;
            return true;
        }
    }

    private void EnsureSkuAvailable(string sku, int? exceptId)
    {
        if (_products.Any(product => product.Id != exceptId && string.Equals(product.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A product with this SKU already exists.");
        }
    }
}
