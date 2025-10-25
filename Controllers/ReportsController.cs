using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOCKER.Data;
using STOCKER.Models;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> SalesStockReport(
     DateTime? fromDate,
     DateTime? toDate,
     string status = "All",
     string mode = "custom")
    {

        DateTime today = DateTime.Today;

        switch (mode?.ToLower())
        {
            case "daily":
                fromDate = today;
                toDate = today;
                break;

            case "monthly":
                fromDate = new DateTime(today.Year, today.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                break;

            case "yearly":
                fromDate = new DateTime(today.Year, 1, 1);
                toDate = new DateTime(today.Year, 12, 31);
                break;

            default:
                fromDate ??= DateTime.MinValue;
                toDate ??= DateTime.MaxValue;
                break;
        }
        if (toDate.HasValue && toDate.Value != DateTime.MaxValue)
        {
            toDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
        }
        var invoicesQuery = _context.SalesInvoices
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Products)
            .Where(i => i.Date >= fromDate && i.Date <= toDate);

        if (status == "Paid" || status == "Unpaid")
            invoicesQuery = invoicesQuery.Where(i => i.PaymentStatus == status);

        var invoices = await invoicesQuery.OrderByDescending(i => i.Date).ToListAsync();

        // Stock Adds
        var stockAdds = await _context.InventoryTransaction
            .Include(t => t.Product)
            .Where(t => t.Type.ToLower() == "add"
                        && t.Timestamp >= fromDate
                        && t.Timestamp <= toDate)
            .ToListAsync();

        // Stock Sells
        var stockSells = await _context.InventoryTransaction
            .Include(t => t.Product)
            .Include(t => t.SalesInvoice)
            .Where(t => t.Type.ToLower() == "sell"
                        && t.Timestamp >= fromDate
                        && t.Timestamp <= toDate)
            .ToListAsync();

        // Get all relevant InventoryTransactions (filtering by date/status if needed)
        var inventoryTransactions = await _context.InventoryTransaction
    .Include(t => t.Product)
    .Where(t => t.Timestamp >= fromDate && t.Timestamp <= toDate)
    .ToListAsync();


        // Group for added stock summary
        var stockAddSummary = inventoryTransactions
            .Where(t => t.Type == "Add")
            .GroupBy(t => t.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                TotalQuantity = g.Sum(x => x.QuantityAdded),
                TotalCost = g.Sum(x => x.QuantityAdded * x.BuyingPrice)
            })
            .ToList();

        // Group for sold stock summary
        var stockSellSummary = inventoryTransactions
            .Where(t => t.Type == "Sell")
            .GroupBy(t => t.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                TotalQuantity = g.Sum(x => x.QuantitySold),
                TotalRevenue = g.Sum(x => x.QuantitySold * x.SellingPrice)
            })
            .ToList();

        // Totals
        decimal totalSales = invoices.Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice));
        decimal totalPaid = invoices
            .Where(i => i.PaymentStatus == "Paid")
            .Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice));
        decimal totalUnpaid = invoices
            .Where(i => i.PaymentStatus == "Unpaid")
            .Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice));

        // Pass Data to View
        ViewBag.Invoices = invoices;
        ViewBag.StockAdds = stockAdds;
        ViewBag.StockSells = stockSells;
        ViewBag.StockAddSummary = stockAddSummary;
        ViewBag.StockSellSummary = stockSellSummary;
        ViewBag.TotalSales = totalSales;
        ViewBag.TotalPaid = totalPaid;
        ViewBag.TotalUnpaid = totalUnpaid;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        ViewBag.Mode = mode;

        return View();
    }

}
