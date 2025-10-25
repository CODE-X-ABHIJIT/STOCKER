using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using STOCKER.Data;
using STOCKER.Extensions.GroceryStoreApp.Extensions;
using STOCKER.Models;

namespace STOCKER.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");
            return cart ?? new List<CartItem>();
        }

        public IActionResult Index()
        {
            return View(GetCart());
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item == null)
                cart.Add(new CartItem { ProductId = id, ProductName = product.Name, Quantity = 1, UnitPrice = product.Price });
            else
                item.Quantity++;

            HttpContext.Session.SetObject("Cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = GetCart();

            var invoice = new SalesInvoice
            {
                Date = DateTime.Now,
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                InvoiceDetails = cart.Select(c => new InvoiceDetail
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList()
            };

            _context.SalesInvoices.Add(invoice);

            foreach (var item in cart)
            {
                var inv = _context.Inventories.First(i => i.ProductId == item.ProductId);
                inv.QuantityInStock -= item.Quantity;
            }

            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("InvoiceSummary", new { id = invoice.InvoiceId });
        }

        public IActionResult InvoiceSummary(int id)
        {
            var invoice = _context.SalesInvoices
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Products)
                .FirstOrDefault(i => i.InvoiceId == id);

            return View(invoice);
        }
        public IActionResult DownloadInvoice(int id)
        {
            var invoice = GetInvoiceById(id);
            return new ViewAsPdf("InvoiceSummary", model: invoice)
            {
                FileName = $"Invoice_{id}.pdf"
            };
        }
            private SalesInvoice GetInvoiceById(int id)
        {
            return _context.SalesInvoices
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Products)
                .FirstOrDefault(i => i.InvoiceId == id);
        }
    }

}
