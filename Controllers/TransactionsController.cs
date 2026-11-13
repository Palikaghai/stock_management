using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly InventoryStore _inventory;

    public TransactionsController(InventoryStore inventory)
    {
        _inventory = inventory;
    }

    public IActionResult Index(string? type = null)
    {
        if (!string.IsNullOrWhiteSpace(type) && type is not ("IN" or "OUT"))
        {
            type = null;
        }

        return View(new TransactionListViewModel
        {
            Type = type,
            Transactions = _inventory.Transactions
                .Where(transaction => type == null || transaction.Type == type)
                .ToArray(),
            Products = _inventory.Products
        });
    }
}
