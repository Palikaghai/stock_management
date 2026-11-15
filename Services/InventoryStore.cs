using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Services;

public sealed class InventoryStore
{
    private static readonly object InventoryLock = new();
    private readonly ApplicationDbContext _dbContext;

    public InventoryStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<InventoryProduct> Products =>
        _dbContext.InventoryProducts.AsNoTracking()
            .OrderBy(product => product.Name)
            .ToArray()
            .Select(ToProduct)
            .ToArray();

    public IReadOnlyList<InventoryTransaction> Transactions =>
        _dbContext.InventoryTransactions.AsNoTracking()
            .OrderByDescending(transaction => transaction.Date)
            .ToArray()
            .Select(ToTransaction)
            .ToArray();

    public IReadOnlyList<string> Categories =>
        _dbContext.InventoryProducts.AsNoTracking()
            .Select(product => product.Category)
            .Distinct()
            .ToArray()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToArray();

    public IReadOnlyList<string> Suppliers =>
        _dbContext.InventorySuppliers.AsNoTracking()
            .Select(supplier => supplier.Name)
            .OrderBy(name => name)
            .ToArray();

    public void Initialize()
    {
        if (!_dbContext.InventoryProducts.Any())
        {
            foreach (var supplier in InventoryData.Suppliers)
            {
                EnsureSupplierExists(supplier);
            }

            _dbContext.SaveChanges();

            foreach (var product in InventoryData.Products)
            {
                _dbContext.InventoryProducts.Add(new InventoryProductEntity
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku,
                    Category = product.Category,
                    Supplier = product.Supplier,
                    OnHand = product.OnHand,
                    ReorderLevel = product.ReorderLevel,
                    UnitCost = product.UnitCost
                });
            }

            foreach (var transaction in InventoryData.Transactions)
            {
                _dbContext.InventoryTransactions.Add(new InventoryTransactionEntity
                {
                    Date = transaction.Date,
                    Type = transaction.Type,
                    ProductId = transaction.ProductId,
                    Quantity = transaction.Quantity,
                    Reference = transaction.Reference
                });
            }

            _dbContext.SaveChanges();
        }
        else
        {
            foreach (var supplier in _dbContext.InventoryProducts.AsNoTracking()
                         .Select(product => product.Supplier).Distinct().ToArray())
            {
                EnsureSupplierExists(supplier);
            }

            _dbContext.SaveChanges();
        }
    }

    public InventoryProduct CreateProduct(string name, string sku, string category, string supplier, int reorderLevel, decimal unitCost)
    {
        lock (InventoryLock)
        {
            var normalizedName = RequireValue(name, "Product name is required.");
            var normalizedSku = RequireValue(sku, "SKU is required.");
            var normalizedCategory = RequireValue(category, "Category is required.");
            var normalizedSupplier = RequireValue(supplier, "Supplier is required.");
            EnsureSkuAvailable(normalizedSku, null);
            EnsureSupplierExists(normalizedSupplier);

            var product = new InventoryProductEntity
            {
                Name = normalizedName,
                Sku = normalizedSku,
                Category = normalizedCategory,
                Supplier = normalizedSupplier,
                OnHand = 0,
                ReorderLevel = reorderLevel,
                UnitCost = unitCost
            };
            _dbContext.InventoryProducts.Add(product);
            _dbContext.SaveChanges();
            return ToProduct(product);
        }
    }

    public void UpdateProduct(int id, string name, string sku, string category, string supplier, int reorderLevel, decimal unitCost)
    {
        lock (InventoryLock)
        {
            var product = _dbContext.InventoryProducts.Find(id)
                ?? throw new KeyNotFoundException($"Product {id} was not found.");
            var normalizedSku = RequireValue(sku, "SKU is required.");
            EnsureSkuAvailable(normalizedSku, id);
            EnsureSupplierExists(RequireValue(supplier, "Supplier is required."));

            product.Name = RequireValue(name, "Product name is required.");
            product.Sku = normalizedSku;
            product.Category = RequireValue(category, "Category is required.");
            product.Supplier = supplier.Trim();
            product.ReorderLevel = reorderLevel;
            product.UnitCost = unitCost;
            _dbContext.SaveChanges();
        }
    }

    public void DeleteProduct(int id)
    {
        lock (InventoryLock)
        {
            var product = _dbContext.InventoryProducts.Find(id)
                ?? throw new KeyNotFoundException($"Product {id} was not found.");
            if (_dbContext.InventoryTransactions.Any(transaction => transaction.ProductId == id))
            {
                throw new InvalidOperationException("This product has transaction history and cannot be deleted.");
            }

            _dbContext.InventoryProducts.Remove(product);
            _dbContext.SaveChanges();
        }
    }

    public void CreateSupplier(string name)
    {
        lock (InventoryLock)
        {
            var normalizedName = RequireValue(name, "Supplier name is required.");
            if (_dbContext.InventorySuppliers.Any(supplier => supplier.Name == normalizedName))
            {
                throw new InvalidOperationException("A supplier with this name already exists.");
            }

            _dbContext.InventorySuppliers.Add(new InventorySupplierEntity { Name = normalizedName });
            _dbContext.SaveChanges();
        }
    }

    public void UpdateSupplier(string currentName, string newName)
    {
        lock (InventoryLock)
        {
            var normalizedCurrentName = RequireValue(currentName, "Supplier name is required.");
            var normalizedNewName = RequireValue(newName, "Supplier name is required.");
            var supplier = _dbContext.InventorySuppliers
                .FirstOrDefault(candidate => candidate.Name == normalizedCurrentName)
                ?? throw new KeyNotFoundException("The supplier was not found.");

            if (string.Equals(supplier.Name, normalizedNewName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (_dbContext.InventorySuppliers.Any(candidate => candidate.Name == normalizedNewName))
            {
                throw new InvalidOperationException("A supplier with this name already exists.");
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            _dbContext.InventorySuppliers.Add(new InventorySupplierEntity { Name = normalizedNewName });
            _dbContext.SaveChanges();

            var products = _dbContext.InventoryProducts
                .Where(product => product.Supplier == supplier.Name)
                .ToArray();
            foreach (var product in products)
            {
                product.Supplier = normalizedNewName;
            }

            _dbContext.SaveChanges();
            _dbContext.InventorySuppliers.Remove(supplier);
            _dbContext.SaveChanges();
            transaction.Commit();
        }
    }

    public void DeleteSupplier(string name)
    {
        lock (InventoryLock)
        {
            var normalizedName = RequireValue(name, "Supplier name is required.");
            var supplier = _dbContext.InventorySuppliers
                .FirstOrDefault(candidate => candidate.Name == normalizedName)
                ?? throw new KeyNotFoundException("The supplier was not found.");

            if (_dbContext.InventoryProducts.Any(product => product.Supplier == supplier.Name))
            {
                throw new InvalidOperationException("This supplier is assigned to products. Reassign or delete those products first.");
            }

            _dbContext.InventorySuppliers.Remove(supplier);
            _dbContext.SaveChanges();
        }
    }

    public InventoryProduct? FindProduct(int id)
    {
        var product = _dbContext.InventoryProducts.AsNoTracking().FirstOrDefault(candidate => candidate.Id == id);
        return product == null ? null : ToProduct(product);
    }

    public bool TryRecordMovement(int productId, string type, int quantity, string? reference, out string error)
    {
        lock (InventoryLock)
        {
            var product = _dbContext.InventoryProducts.Find(productId);
            if (product == null)
            {
                error = "Select a valid product.";
                return false;
            }

            if (quantity <= 0)
            {
                error = "Quantity must be greater than zero.";
                return false;
            }

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

            var movementReference = string.IsNullOrWhiteSpace(reference)
                ? $"{(isStockIn ? "IN" : "OUT")}-{Guid.NewGuid():N}"[..11].ToUpperInvariant()
                : reference.Trim();
            if (movementReference.Length > 60)
            {
                error = "Reference must be 60 characters or fewer.";
                return false;
            }

            product.OnHand = isStockIn ? product.OnHand + quantity : product.OnHand - quantity;
            _dbContext.InventoryTransactions.Add(new InventoryTransactionEntity
            {
                Date = DateTime.Now,
                Type = type,
                ProductId = productId,
                Quantity = quantity,
                Reference = movementReference
            });
            _dbContext.SaveChanges();
            error = string.Empty;
            return true;
        }
    }

    private void EnsureSupplierExists(string name)
    {
        if (!_dbContext.InventorySuppliers.Any(supplier => supplier.Name == name))
        {
            _dbContext.InventorySuppliers.Add(new InventorySupplierEntity { Name = name });
        }
    }

    private void EnsureSkuAvailable(string sku, int? exceptId)
    {
        if (_dbContext.InventoryProducts.Any(product => product.Id != exceptId && product.Sku == sku))
        {
            throw new InvalidOperationException("A product with this SKU already exists.");
        }
    }

    private static string RequireValue(string value, string error)
    {
        var normalizedValue = value?.Trim();
        return string.IsNullOrWhiteSpace(normalizedValue)
            ? throw new InvalidOperationException(error)
            : normalizedValue;
    }

    private static InventoryProduct ToProduct(InventoryProductEntity product) =>
        new(product.Id, product.Name, product.Sku, product.Category, product.Supplier, product.OnHand, product.ReorderLevel, product.UnitCost);

    private static InventoryTransaction ToTransaction(InventoryTransactionEntity transaction) =>
        new(transaction.Date, transaction.Type, transaction.ProductId, transaction.Quantity, transaction.Reference);
}
