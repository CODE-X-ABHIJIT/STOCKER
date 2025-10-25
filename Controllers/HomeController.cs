using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOCKER.Data;
using STOCKER.Models;
using System.Diagnostics;
using System.Globalization;

namespace STOCKER.Controllers
{
    [Authorize(Roles = "Admin,Cashier")]

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalStockValue = await _context.Products
                .Include(p => p.Inventory)
                .SumAsync(p => p.BuyingPrice * p.Inventory.QuantityInStock);

            var todaySales = await _context.SalesInvoices
                .Where(i => i.Date.Date == today )
                .SelectMany(i => i.InvoiceDetails)
                .SumAsync(d => d.Quantity * d.UnitPrice);

            var monthSales = await _context.SalesInvoices
                .Where(i => i.Date >= firstOfMonth)
                .SelectMany(i => i.InvoiceDetails)
                .SumAsync(d => d.Quantity * d.UnitPrice);

            var totalCustomers = await _context.SalesInvoices
                
                .Select(i => i.CustomerMobile)
                .Distinct()
                .CountAsync();

            var dailySales = await _context.SalesInvoices
                .Where(i => i.Date >= today.AddDays(-6) && i.PaymentStatus == "Paid")
                .GroupBy(i => i.Date.Date)
                .Select(g => new
                {
                    DateLabel = g.Key.ToString("dd MMM"),
                    Amount = g.SelectMany(i => i.InvoiceDetails).Sum(d => d.Quantity * d.UnitPrice)
                })
                .ToListAsync();

            var monthlySales = await _context.SalesInvoices
                .Where(i => i.Date.Year == today.Year && i.PaymentStatus == "Paid")
                .GroupBy(i => i.Date.Month)
                .Select(g => new
                {
                    MonthLabel = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                    Amount = g.SelectMany(i => i.InvoiceDetails).Sum(d => d.Quantity * d.UnitPrice)
                })
                .ToListAsync();

            // ✅ Total Profit Calculation (only for Paid invoices)
            var profitQuery = await _context.SalesInvoices
                .Where(i => i.PaymentStatus == "Paid")
                .SelectMany(i => i.InvoiceDetails)
                .Include(d => d.Products)
                .ToListAsync();

            var totalProfit = profitQuery.Sum(d =>
                (d.UnitPrice - d.Products.BuyingPrice) * d.Quantity
            );

            var totalUnpaid = await _context.SalesInvoices
                 .Where(i => i.PaymentStatus == "Unpaid")
                 .SelectMany(i => i.InvoiceDetails)
                 .SumAsync(d => d.Quantity * d.UnitPrice);

            ViewBag.TotalUnpaid = totalUnpaid;
            ViewBag.TotalStockValue = totalStockValue;
            ViewBag.TodaySales = todaySales;
            ViewBag.MonthSales = monthSales;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.DailySales = dailySales;
            ViewBag.MonthlySales = monthlySales;
            ViewBag.TotalProfit = totalProfit; // ✅ Send to view

            return View();
        }

    }

}
