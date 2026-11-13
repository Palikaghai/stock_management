using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly InventoryStore _inventory;

    public ReportsController(InventoryStore inventory)
    {
        _inventory = inventory;
    }

    private static readonly IReadOnlyDictionary<string, string> Titles = new Dictionary<string, string>
    {
        ["overall"] = "Overall inventory", ["category"] = "Category-wise products", ["supplier"] = "Supplier-wise inventory",
        ["stock-in"] = "Stock IN", ["stock-out"] = "Stock OUT", ["low-stock"] = "Low stock"
    };

    public IActionResult Index(string reportType = "overall", DateTime? startDate = null, DateTime? endDate = null,
        string? category = null, string? supplier = null, int? productId = null) =>
        View(Build(reportType, startDate, endDate, category, supplier, productId));

    public IActionResult Export(string reportType = "overall", DateTime? startDate = null, DateTime? endDate = null,
        string? category = null, string? supplier = null, int? productId = null)
    {
        var report = Build(reportType, startDate, endDate, category, supplier, productId);
        var csv = new StringBuilder("Product,SKU,Category,Supplier,Quantity,Unit cost,Value,Status,Date,Reference\r\n");
        foreach (var r in report.Rows)
        {
            var fields = new[] { r.Product, r.Sku, r.Category, r.Supplier, r.Quantity.ToString(CultureInfo.InvariantCulture),
                r.UnitCost.ToString("0.00", CultureInfo.InvariantCulture), r.Value.ToString("0.00", CultureInfo.InvariantCulture),
                r.Status, r.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "", r.Reference ?? "" };
            csv.AppendLine(string.Join(",", fields.Select(v => "\"" + v.Replace("\"", "\"\"") + "\"")));
        }
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8", $"smartstock-{report.ReportType}-{DateTime.Today:yyyyMMdd}.csv");
    }

    private ReportsViewModel Build(string type, DateTime? from, DateTime? to, string? category, string? supplier, int? productId)
    {
        if (!Titles.ContainsKey(type)) type = "overall";
        var products = _inventory.Products;
        var transactions = _inventory.Transactions;
        var categories = _inventory.Categories;
        var suppliers = _inventory.Suppliers;
        var lookup = products.ToDictionary(p => p.Id);
        IEnumerable<ReportRow> rows;
        if (type is "stock-in" or "stock-out")
        {
            var direction = type == "stock-in" ? "IN" : "OUT";
            rows = transactions.Where(t => t.Type == direction)
                .Where(t => !from.HasValue || t.Date.Date >= from.Value.Date)
                .Where(t => !to.HasValue || t.Date.Date <= to.Value.Date)
                .Select(t => { var p = lookup[t.ProductId]; return new ReportRow(p.Name, p.Sku, p.Category, p.Supplier,
                    t.Quantity, p.UnitCost, t.Quantity * p.UnitCost, direction, t.Date, t.Reference); });
        }
        else
        {
            rows = products.Where(p => type != "low-stock" || p.OnHand <= p.ReorderLevel)
                .Select(p => new ReportRow(p.Name, p.Sku, p.Category, p.Supplier, p.OnHand, p.UnitCost,
                    p.OnHand * p.UnitCost, p.OnHand <= p.ReorderLevel ? "Low stock" : "In stock", null, null));
        }

        var result = rows.Where(r => string.IsNullOrWhiteSpace(category) || r.Category == category)
            .Where(r => string.IsNullOrWhiteSpace(supplier) || r.Supplier == supplier)
            .Where(r => !productId.HasValue || lookup.TryGetValue(productId.Value, out var p) && p.Sku == r.Sku)
            .OrderBy(r => r.Product).ToArray();
        return new ReportsViewModel
        {
            ReportType = type, StartDate = from, EndDate = to, Category = category, Supplier = supplier, ProductId = productId,
            Categories = categories, Suppliers = suppliers, Products = products, Rows = result,
            ReportTitle = Titles[type], TotalValue = result.Sum(r => r.Value)
        };
    }
}
