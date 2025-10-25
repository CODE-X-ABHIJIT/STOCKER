using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using STOCKER.Data;
using STOCKER.Extensions.GroceryStoreApp.Extensions;
using STOCKER.Models;
using System.Security.Claims;

namespace STOCKER.Controllers
{
    //[Authorize(Roles = "Admin,Cashier")]

    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Step 1: Product Listing + Search
        public async Task<IActionResult> Index(string q)
        {
            var products = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                products = products.Where(p => p.Name.Contains(q));
            }

            var model = await products.Include(p => p.Inventory).ToListAsync();
            return View(model);
        }

        // ✅ Add multiple items to cart at once (from Proceed to Review)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMultipleToBillingCart([FromBody] List<CartAddRequest> items)
        {
            if (items == null || !items.Any())
                return BadRequest("No items received.");

            // ✅ Always start with a clean cart for this action
            var newCart = new List<CartItem>();

            foreach (var item in items)
            {
                if (item.qty <= 0) continue;

                var product = _context.Products.FirstOrDefault(p => p.ProductId == item.productId);
                if (product == null) continue;

                newCart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = item.qty,
                    UnitPrice = product.Price
                });
            }

            // ✅ Replace session cart with new one
            HttpContext.Session.SetObject("BillingCart", newCart);

            return Json(new { success = true });
        }

        public IActionResult AddToBillingCart([FromBody] CartAddRequest model)
        {
            if (model == null || model.qty <= 0)
                return BadRequest("Invalid data.");

            var product = _context.Products.Include(p => p.Inventory)
                                .FirstOrDefault(p => p.ProductId == model.productId);

            if (product == null)
                return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("BillingCart") ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(c => c.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += model.qty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = model.qty,
                    UnitPrice = product.Price
                });
            }

            HttpContext.Session.SetObject("BillingCart", cart);

            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        // Step 2: Review Cart
        public IActionResult Review()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("BillingCart") ?? new List<CartItem>();
            return View(cart);
        }

        // Step 3: Confirm and Generate Invoice
        
        [Route("Billing/Confirm")]
        [HttpPost]
        public async Task<IActionResult> Confirm(string CustomerName, string Mobile, string Address, string PaymentStatus)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("BillingCart") ?? new List<CartItem>();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // 1️⃣ Validate products
            var validProductIds = cart.Select(c => c.ProductId).Distinct().ToList();
            var productMap = await _context.Products
                .Where(p => validProductIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var filteredCart = cart
                .Where(item => productMap.ContainsKey(item.ProductId))
                .ToList();

            if (!filteredCart.Any())
                return BadRequest("Cart contains no valid products.");

            // 2️⃣ Create and save invoice FIRST
            var invoice = new SalesInvoice
            {
                Date = DateTime.Now,
                UserId = userId,
                CustomerName = CustomerName,
                CustomerMobile = Mobile,
                CustomerAddress = Address,
                PaymentStatus = PaymentStatus,
                PaidDate = PaymentStatus == "Paid" ? DateTime.Now : null,
                InvoiceDetails = filteredCart.Select(c => new InvoiceDetail
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    Products = productMap[c.ProductId]
                }).ToList()
            };

            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync(); // ✅ Save early to get InvoiceId

            // 3️⃣ Update inventory & add sell transactions
            foreach (var item in filteredCart)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.QuantityInStock -= item.Quantity;
                }

                var transaction = new InventoryTransaction
                {
                    ProductId = item.ProductId,
                    QuantitySold = item.Quantity,
                    SellingPrice = item.UnitPrice,
                    Timestamp = DateTime.Now,
                    Type = "Sell",
                    SalesInvoiceId = invoice.InvoiceId // ✅ Now assign real InvoiceId
                };

                _context.InventoryTransaction.Add(transaction);
            }

            // 4️⃣ Save all
            try
            {
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("BillingCart");
                return RedirectToAction("InvoiceSummary", new { id = invoice.InvoiceId });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Save failed: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // View Summary
        public IActionResult InvoiceSummary(int id)
        {
            var invoice = _context.SalesInvoices
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Products)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }


        // GET: /Billing/AllInvoices
        public async Task<IActionResult> AllInvoices(string search, string status)
        {
            var invoicesQuery = _context.SalesInvoices
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Products)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                invoicesQuery = invoicesQuery.Where(i =>
                    i.CustomerName.Contains(search) ||
                    i.InvoiceId.ToString().Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                invoicesQuery = invoicesQuery.Where(i => i.PaymentStatus == status);
            }

            var invoices = await invoicesQuery
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return View(invoices);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var invoice = await _context.SalesInvoices.FindAsync(id);
            if (invoice == null || invoice.PaymentStatus == "Paid")
                return NotFound();

            invoice.PaymentStatus = "Paid";
            invoice.PaidDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("InvoiceSummary", new { id });
        }


    }
}
