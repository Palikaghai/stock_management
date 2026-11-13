using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryStore _inventory;

    public DashboardController(UserManager<ApplicationUser> userManager, InventoryStore inventory)
    {
        _userManager = userManager;
        _inventory = inventory;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.UserFullName = user?.FullName ?? user?.UserName ?? "User";

        var products = _inventory.Products;
        var transactions = _inventory.Transactions;
        var categories = _inventory.Categories;
        var months = Enumerable.Range(0, 6).Select(i => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(i - 5)).ToArray();
        return View(new DashboardViewModel
        {
            TotalProducts = products.Count,
            TotalStockValue = products.Sum(p => p.OnHand * p.UnitCost),
            LowStockCount = products.Count(p => p.OnHand <= p.ReorderLevel),
            TotalSuppliers = _inventory.Suppliers.Count,
            Products = products,
            RecentTransactions = transactions.OrderByDescending(t => t.Date).Take(5).ToArray(),
            LowStockProducts = products.Where(p => p.OnHand <= p.ReorderLevel).OrderBy(p => p.OnHand).ToArray(),
            CategoryLabels = categories,
            CategoryValues = categories.Select(c => products.Where(p => p.Category == c).Sum(p => p.OnHand)).ToArray(),
            MovementLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
            StockInValues = [18, 12, 24, 8, 15, 10, 20],
            StockOutValues = [9, 14, 11, 17, 8, 12, 6],
            ValuationValues = categories.Select(c => products.Where(p => p.Category == c).Sum(p => p.OnHand * p.UnitCost)).ToArray(),
            MonthLabels = months.Select(m => m.ToString("MMM")).ToArray(),
            MonthlyInValues = months.Select(m => transactions.Where(t => t.Type == "IN" && t.Date.Year == m.Year && t.Date.Month == m.Month).Sum(t => t.Quantity)).ToArray(),
            MonthlyOutValues = months.Select(m => transactions.Where(t => t.Type == "OUT" && t.Date.Year == m.Year && t.Date.Month == m.Month).Sum(t => t.Quantity)).ToArray()
        });
    }
}
